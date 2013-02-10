/*************************************************************************
Crytek Source File.
Copyright (C), Crytek Studios, 2001-2005.
-------------------------------------------------------------------------
$Id$
$DateTime$

-------------------------------------------------------------------------
History:
- 23:5:2006   9:27 : Created by Marcio Martins

*************************************************************************/
#include "StdAfx.h"
#include "GameRules.h"
#include "Game.h"
#include "GameCVars.h"

#include "IVehicleSystem.h"
#include "IItemSystem.h"
#include "IMaterialEffects.h"

#include "Audio/GameAudio.h"
#include "Audio/SoundMoods.h"
#include "Audio/BattleStatus.h"
#include "IWorldQuery.h"

#include <StlUtils.h>


#include "Network/Lobby/GameLobby.h"
//------------------------------------------------------------------------
void CGameRules::KnockActorDown( EntityId actorEntityId )
{
		// Forbid fall and play if the actor is playing a hit/death reaction
		IActor *pHitActor = static_cast<IActor*>(gEnv->pGame->GetIGameFramework()->GetIActorSystem()->GetActor( actorEntityId ));
		if (pHitActor)
			pHitActor->Fall();
}

//------------------------------------------------------------------------
// RMI
//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, SvRequestRename)
{
	IActor *pActor = GetActorByEntityId(params.entityId);
	if (!pActor)
		return true;

	RenamePlayer(pActor, params.name.c_str());

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClRenameEntity)
{
	IEntity *pEntity=gEnv->pEntitySystem->GetEntity(params.entityId);
	if (pEntity)
	{
		string old=pEntity->GetName();
		pEntity->SetName(params.name.c_str());

		CryLogAlways("$8%s$o renamed to $8%s", old.c_str(), params.name.c_str());

		// if this was a remote player, check we're not spectating them.
		//	If we are, we need to trigger a spectator hud update for the new name
		EntityId clientId = g_pGame->GetIGameFramework()->GetClientActorId();
		if(gEnv->bMultiplayer && params.entityId != clientId)
		{
			IActor* pClientActor = static_cast<IActor *>(g_pGame->GetIGameFramework()->GetClientActor());
		}
	}

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, SvRequestChatMessage)
{
	SendChatMessage((EChatMessageType)params.type, params.sourceId, params.targetId, params.msg.c_str());

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClChatMessage)
{
	OnChatMessage((EChatMessageType)params.type, params.sourceId, params.targetId, params.msg.c_str(), params.onlyTeam);

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, SvRequestChangeTeam)
{
	IActor *pActor = GetActorByEntityId(params.entityId);
	if (!pActor)
		return true;

	ChangeTeam(pActor, params.teamId);

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClSetTeam)
{
	if (!params.entityId) // ignore these for now
		return true;

	int oldTeam = GetTeam(params.entityId);
	if (oldTeam==params.teamId)
		return true;

	TEntityTeamIdMap::iterator it=m_entityteams.find(params.entityId);
	if (it!=m_entityteams.end())
		m_entityteams.erase(it);

	IActor *pActor=m_pActorSystem->GetActor(params.entityId);
	bool isplayer=pActor!=0;
	if (isplayer && oldTeam)
	{
		TPlayerTeamIdMap::iterator pit=m_playerteams.find(oldTeam);
		assert(pit!=m_playerteams.end());
		stl::find_and_erase(pit->second, params.entityId);
	}

	if (params.teamId)
	{
		m_entityteams.insert(TEntityTeamIdMap::value_type(params.entityId, params.teamId));
		if (isplayer)
		{
			TPlayerTeamIdMap::iterator pit=m_playerteams.find(params.teamId);
			assert(pit!=m_playerteams.end());
			pit->second.push_back(params.entityId);
		}
	}

	m_pScript->CallMethod( "OnSetTeam", params.entityId, params.teamId);

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClTextMessage)
{
	OnTextMessage((ETextMessageType)params.type, params.msg.c_str(), 
		params.params[0].empty()?0:params.params[0].c_str(),
		params.params[1].empty()?0:params.params[1].c_str(),
		params.params[2].empty()?0:params.params[2].c_str(),
		params.params[3].empty()?0:params.params[3].c_str()
		);

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClSetGameTime)
{
	m_endTime = params.endTime;

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClSetRoundTime)
{
	m_roundEndTime = params.endTime;

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClSetPreRoundTime)
{
	m_preRoundEndTime = params.endTime;

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClSetReviveCycleTime)
{
	m_reviveCycleEndTime = params.endTime;

	return true;
}

//------------------------------------------------------------------------
IMPLEMENT_RMI(CGameRules, ClSetGameStartTimer)
{
	m_gameStartTime = params.endTime;

	return true;
}

IMPLEMENT_RMI(CGameRules, ClEnteredGame)
{
	IActor *pClientActor = m_pGameFramework->GetClientActor();

	if (pClientActor)
	{
		IEntity *pClientEntity = pClientActor->GetEntity();
		const EntityId clientEntityId = pClientEntity->GetId();

		if(!gEnv->bServer)
		{
			int status[2];
			status[0] = GetTeam(clientEntityId);
			status[1] = pClientActor->GetSpectatorMode();
			m_pGameplayRecorder->Event(pClientEntity, GameplayEvent(eGE_Connected, 0, 0, (void*)status));
		}

		if (g_pGame->GetHostMigrationState() != CGame::eHMS_NotMigrating)
		{
			eHostMigrationState hostMigrationState = g_pGame->GetGameLobby()->GetMatchMakingHostMigrationState();
			if (hostMigrationState < eHMS_ReconnectClient)
			{
				CryLog("CGameRules::ClEnteredGame() received a left over message from previous server, ignoring it");
				return true;
			}

			CryLog("CGameRules::ClEnteredGame() We have our client actor ('%s'), send migration params", pClientEntity->GetName());

			// Request various bits
			GetGameObject()->InvokeRMI(SvHostMigrationRequestSetup(), *m_pHostMigrationParams, eRMI_ToServer);
			SAFE_DELETE(m_pHostMigrationParams);

			pClientActor->GetEntity()->SetPos(m_pHostMigrationClientParams->m_position);
			pClientActor->SetViewRotation(m_pHostMigrationClientParams->m_viewQuat);

			if (m_pHostMigrationClientParams->m_hasValidVelocity)
			{
				pe_action_set_velocity actionVel;
				actionVel.v = m_pHostMigrationClientParams->m_velocity;
				actionVel.w.zero();
				IPhysicalEntity *pPhysicalEntity = pClientEntity->GetPhysics();
				if (pPhysicalEntity)
				{
					pPhysicalEntity->Action(&actionVel);
				}
			}

			/*CPlayerMovementController *pPMC = static_cast<CPlayerMovementController *>(pClientActor->GetMovementController());
			if (pPMC)
			{
				// Force an update through so that the aim direction gets set correctly
				pPMC->PostUpdate(0.f);
			}*/

			/*if (m_pHostMigrationClientParams->m_pSelectedItemClass)
			{
				CItem *pItem = pClientActor->GetItemByClass(m_pHostMigrationClientParams->m_pSelectedItemClass);
				if (pItem)
				{
					EntityId itemId = pItem->GetEntityId();
					if (pClientActor->GetCurrentItemId() != itemId)
					{
						pClientActor->SelectItem(itemId, false);
					}
				}
			}*/

			m_pHostMigrationClientParams->m_doneEnteredGame = true;
			if (m_pHostMigrationClientParams->IsDone())
			{
				SAFE_DELETE(m_pHostMigrationClientParams);
			}

			if (!gEnv->bServer)
			{
				// todo: ui
			}

			m_hostMigrationClientHasRejoined = true;
		}
	}

	return true;
}

IMPLEMENT_RMI(CGameRules, ClPlayerJoined)
{
	m_pScript->CallMethod("OnPlayerJoined", ToMonoString(params.name.c_str()), params.entityId);

	return true;
}

IMPLEMENT_RMI(CGameRules, ClPlayerLeft)
{
	m_pScript->CallMethod("OnPlayerLeft", ToMonoString(params.name.c_str()), params.entityId);

	return true;
}
