/*************************************************************************
	Crytek Source File.
	Copyright (C), Crytek Studios, 2001-2004.
	-------------------------------------------------------------------------
	$Id$
	$DateTime$

	-------------------------------------------------------------------------
	History:
		- 7:2:2006   15:38 : Created by Marcio Martins

*************************************************************************/
#include "StdAfx.h"
#include "GameRules.h"
#include "Game.h"
#include "GameCVars.h"

#include <IAIObject.h>

#include "IVehicleSystem.h"
#include "IItemSystem.h"

#include "IUIDraw.h"
#include "IMovieSystem.h"

#include "ServerSynchedStorage.h"

#include "GameActions.h"
#include "Audio/GameAudio.h"
#include "SPAnalyst.h"
#include "IWorldQuery.h"

#include <StlUtils.h>
#include <StringUtils.h>

#include <IBreakableManager.h>

int CGameRules::s_invulnID = 0;
int CGameRules::s_barbWireID = 0;

//------------------------------------------------------------------------
CGameRules::CGameRules()
: m_pGameFramework(0),
	m_pGameplayRecorder(0),
	m_pSystem(0),
	m_pActorSystem(0),
	m_pEntitySystem(0),
	m_pScriptSystem(0),
	m_pMaterialManager(0),
	m_pClientNetChannel(0),
	m_pScript(nullptr),
	m_teamIdGen(0),
	m_endTime(0.0f),
	m_roundEndTime(0.0f),
	m_preRoundEndTime(0.0f),
	m_gameStartTime(0.0f),
	m_ignoreEntityNextCollision(0),
	m_timeOfDayInitialized(false),
	m_explosionScreenFX(true)
{
}

//------------------------------------------------------------------------
CGameRules::~CGameRules()
{
	if (m_pGameFramework)
	{
		if (m_pGameFramework->GetIGameRulesSystem())
			m_pGameFramework->GetIGameRulesSystem()->SetCurrentGameRules(0);
		
		if(m_pGameFramework->GetIViewSystem())
			m_pGameFramework->GetIViewSystem()->RemoveListener(this);
	}
}

//------------------------------------------------------------------------
bool CGameRules::Init( IGameObject * pGameObject )
{
	SetGameObject(pGameObject);

	if (!GetGameObject()->BindToNetwork())
		return false;

	GetGameObject()->EnablePostUpdates(this);

	m_pGameFramework = g_pGame->GetIGameFramework();
	m_pGameplayRecorder = m_pGameFramework->GetIGameplayRecorder();
	m_pSystem = m_pGameFramework->GetISystem();
	m_pActorSystem = m_pGameFramework->GetIActorSystem();
	m_pEntitySystem = gEnv->pEntitySystem;
	m_pScriptSystem = m_pSystem->GetIScriptSystem();
	m_pMaterialManager = gEnv->p3DEngine->GetMaterialManager();
	s_invulnID = m_pMaterialManager->GetSurfaceTypeManager()->GetSurfaceTypeByName("mat_invulnerable")->GetId();
	s_barbWireID = m_pMaterialManager->GetSurfaceTypeManager()->GetSurfaceTypeByName("mat_metal_barbwire")->GetId();

	//Register as ViewSystem listener (for cut-scenes, ...)
	if(m_pGameFramework->GetIViewSystem())
		m_pGameFramework->GetIViewSystem()->AddListener(this);

	m_pGameFramework->GetIGameRulesSystem()->SetCurrentGameRules(this);

	SAFE_RELEASE(m_pScript);
	m_pScript = GetMonoScriptSystem()->InstantiateScript(GetEntity()->GetClass()->GetName(), eScriptFlag_GameRules);

	// setup animation time scaling (until we have assets that cover the speeds we need timescaling).
	if (gEnv->pCharacterManager)
		gEnv->pCharacterManager->SetScalingLimits( Vec2(0.5f, 3.0f) );

	bool isMultiplayer=gEnv->bMultiplayer;

	if(gEnv->IsClient())
	{
		IActionMapManager *pActionMapMan = g_pGame->GetIGameFramework()->GetIActionMapManager();
		IActionMap *am = NULL;
		pActionMapMan->EnableActionMap("multiplayer",isMultiplayer);
		pActionMapMan->EnableActionMap("singleplayer",!isMultiplayer);
		if(isMultiplayer)
		{
			am=pActionMapMan->GetActionMap("multiplayer");
		}
		else
		{
			am=pActionMapMan->GetActionMap("singleplayer");
		}

		if(am)
		{
			am->SetActionListener(GetEntity()->GetId());
		}
	}

	g_pGame->GetSPAnalyst()->Enable(!isMultiplayer);

	return true;
}

//------------------------------------------------------------------------
void CGameRules::PostInit( IGameObject * pGameObject )
{
	pGameObject->EnableUpdateSlot(this, 0);
	pGameObject->SetUpdateSlotEnableCondition(this, 0, eUEC_WithoutAI);
	pGameObject->EnablePostUpdates(this);
	
	IConsole *pConsole=gEnv->pConsole;
	RegisterConsoleCommands(pConsole);
	RegisterConsoleVars(pConsole);
}

//------------------------------------------------------------------------
void CGameRules::InitClient(int channelId)
{
}

//------------------------------------------------------------------------
void CGameRules::PostInitClient(int channelId)
{
	// update the time
	GetGameObject()->InvokeRMI(ClSetGameTime(), SetGameTimeParams(m_endTime), eRMI_ToClientChannel, channelId);
	GetGameObject()->InvokeRMI(ClSetRoundTime(), SetGameTimeParams(m_roundEndTime), eRMI_ToClientChannel, channelId);
	GetGameObject()->InvokeRMI(ClSetPreRoundTime(), SetGameTimeParams(m_preRoundEndTime), eRMI_ToClientChannel, channelId);
	GetGameObject()->InvokeRMI(ClSetReviveCycleTime(), SetGameTimeParams(m_reviveCycleEndTime), eRMI_ToClientChannel, channelId);
	if (m_gameStartTime.GetMilliSeconds()>m_pGameFramework->GetServerTime().GetMilliSeconds())
		GetGameObject()->InvokeRMI(ClSetGameStartTimer(), SetGameTimeParams(m_gameStartTime), eRMI_ToClientChannel, channelId);

	// update team status on the client
	for (TEntityTeamIdMap::const_iterator tit=m_entityteams.begin(); tit!=m_entityteams.end(); ++tit)
		GetGameObject()->InvokeRMIWithDependentObject(ClSetTeam(), SetTeamParams(tit->first, tit->second), eRMI_ToClientChannel, tit->first, channelId);
}

//------------------------------------------------------------------------
void CGameRules::Release()
{
	UnregisterConsoleCommands(gEnv->pConsole);
	delete this;
}

//------------------------------------------------------------------------
void CGameRules::FullSerialize( TSerialize ser )
{
	SAFE_GAMEAUDIO_FUNC(Serialize(ser));

	if (g_pGame->GetSPAnalyst())
		g_pGame->GetSPAnalyst()->Serialize(ser);
}

//-----------------------------------------------------------------------------------------------------
void CGameRules::PostSerialize()
{
}

//------------------------------------------------------------------------
void CGameRules::Update( SEntityUpdateContext& ctx, int updateSlot )
{
	if (updateSlot!=0)
		return;

	//g_pGame->GetServerSynchedStorage()->SetGlobalValue(15, 1026);

	bool server=gEnv->bServer;

	if (server)
		UpdateEntitySchedules(ctx.fFrameTime);

	if (gEnv->bServer)
		GetGameObject()->ChangedNetworkState( eEA_GameServerDynamic );
}

//------------------------------------------------------------------------
void CGameRules::HandleEvent( const SGameObjectEvent& event)
{
}

//------------------------------------------------------------------------
void CGameRules::ProcessEvent( SEntityEvent& event)
{
	FUNCTION_PROFILER(gEnv->pSystem, PROFILE_GAME);

	static ICVar* pTOD = gEnv->pConsole->GetCVar("sv_timeofdayenable");

	switch(event.event)
	{
	case ENTITY_EVENT_RESET:
		m_timeOfDayInitialized = false;
    	
		m_respawns.clear();
		m_removals.clear();
		break;

	case ENTITY_EVENT_START_GAME:
		m_timeOfDayInitialized = false;

		if (gEnv->bServer && gEnv->bMultiplayer && pTOD && pTOD->GetIVal() && g_pGame->GetIGameFramework()->IsImmersiveMPEnabled())
		{
			static ICVar* pStart = gEnv->pConsole->GetCVar("sv_timeofdaystart");
			if (pStart)
				gEnv->p3DEngine->GetTimeOfDay()->SetTime(pStart->GetFVal(), true);
		}

		break;
	}
}

//------------------------------------------------------------------------
IActor *CGameRules::GetActorByChannelId(int channelId) const
{
	return static_cast<IActor *>(m_pGameFramework->GetIActorSystem()->GetActorByChannelId(channelId));
}

//------------------------------------------------------------------------
IActor *CGameRules::GetActorByEntityId(EntityId entityId) const
{
	return static_cast<IActor *>(m_pGameFramework->GetIActorSystem()->GetActor(entityId));
}

//------------------------------------------------------------------------
int CGameRules::GetChannelId(EntityId entityId) const
{
	IActor *pActor = static_cast<IActor *>(m_pGameFramework->GetIActorSystem()->GetActor(entityId));
	if (pActor)
		return pActor->GetChannelId();

	return 0;
}

//------------------------------------------------------------------------
bool CGameRules::IsDead(EntityId id) const
{
	if (IActor *pActor=GetActorByEntityId(id))
		return (pActor->GetHealth()<=0);

	return false;
}

//------------------------------------------------------------------------
bool CGameRules::ShouldKeepClient(int channelId, EDisconnectionCause cause, const char *desc) const
{
	return (!strcmp("timeout", desc) || cause==eDC_Timeout);
}

//------------------------------------------------------------------------
void CGameRules::PrecacheLevel()
{
	//CallScript(m_script, "PrecacheLevel");
	m_pScript->CallMethod("PrecacheLevel");
}

//------------------------------------------------------------------------
void CGameRules::OnConnect(struct INetChannel *pNetChannel)
{
	m_pClientNetChannel=pNetChannel;

	//CallScript(m_clientStateScript,"OnConnect");
	m_pScript->CallMethod("OnConnect");
}


//------------------------------------------------------------------------
void CGameRules::OnDisconnect(EDisconnectionCause cause, const char *desc)
{
	m_pClientNetChannel=0;
	//int icause=(int)cause;
	//CallScript(m_clientStateScript, "OnDisconnect", icause, desc);
	m_pScript->CallMethod("OnDisconnect", cause, desc);
}

//------------------------------------------------------------------------
bool CGameRules::OnClientConnect(int channelId, bool isReset)
{
	if (!isReset)
	{
		m_channelIds.push_back(channelId);
		g_pGame->GetServerSynchedStorage()->OnClientConnect(channelId);
	}

	m_pScript->CallMethod("OnClientConnect", channelId, isReset, GetPlayerName(channelId, true).c_str());

	IActor *pActor=GetActorByChannelId(channelId);
	if (pActor)
	{
		//we need to pass team somehow so it will be reported correctly
		int status[2];
		status[0] = GetTeam(pActor->GetEntityId());
		status[1] = pActor->GetSpectatorMode();
		m_pGameplayRecorder->Event(pActor->GetEntity(), GameplayEvent(eGE_Connected, 0, m_pGameFramework->IsChannelOnHold(channelId)?1.0f:0.0f, (void*)status));
		
		//notify client he has entered the game
		GetGameObject()->InvokeRMIWithDependentObject(ClEnteredGame(), NoParams(), eRMI_ToClientChannel, pActor->GetEntityId(), channelId);
		
		if (isReset)
		{
			SetTeam(GetChannelTeam(channelId), pActor->GetEntityId());
		}
	}

	return pActor != 0;
}

//------------------------------------------------------------------------
void CGameRules::OnClientDisconnect(int channelId, EDisconnectionCause cause, const char *desc, bool keepClient)
{
	IActor *pActor=GetActorByChannelId(channelId);
	//assert(pActor);

	if (!pActor || !keepClient)
		if (g_pGame->GetServerSynchedStorage())
			g_pGame->GetServerSynchedStorage()->OnClientDisconnect(channelId, false);

	if (!pActor)
		return;

	string playerName = GetPlayerName(channelId);
	RenameEntityParams params(pActor->GetEntityId(), playerName.c_str());
	GetGameObject()->InvokeRMI(ClPlayerLeft(), params, eRMI_ToAllClients);

	if (pActor)
		m_pGameplayRecorder->Event(pActor->GetEntity(), GameplayEvent(eGE_Disconnected,"",keepClient?1.0f:0.0f));

	if (keepClient)
	{
		if (g_pGame->GetServerSynchedStorage())
			g_pGame->GetServerSynchedStorage()->OnClientDisconnect(channelId, true);

		pActor->GetGameObject()->SetAspectProfile(eEA_Physics, eAP_NotPhysicalized);

		return;
	}

	if (IVehicle *pVehicle=pActor->GetLinkedVehicle())
	{
		if (IVehicleSeat *pSeat=pVehicle->GetSeatForPassenger(pActor->GetEntityId()))
			pSeat->Reset();
	}

	SetTeam(0, pActor->GetEntityId());

	std::vector<int>::iterator channelit=std::find(m_channelIds.begin(), m_channelIds.end(), channelId);
	if (channelit!=m_channelIds.end())
		m_channelIds.erase(channelit);

	//CallScript(m_serverStateScript, "OnClientDisconnect", channelId);
	m_pScript->CallMethod("OnClientDisconnect", channelId);

	return;
}

//------------------------------------------------------------------------
bool CGameRules::OnClientEnteredGame(int channelId, bool isReset)
{ 
	IActor *pActor=GetActorByChannelId(channelId);
	if (!pActor)
		return false;

	string playerName = GetPlayerName(channelId);
	RenameEntityParams params(pActor->GetEntityId(), playerName.c_str());
	GetGameObject()->InvokeRMI(ClPlayerJoined(), params, eRMI_ToAllClients);

	if (g_pGame->GetServerSynchedStorage())
		g_pGame->GetServerSynchedStorage()->OnClientEnteredGame(channelId);

	IScriptTable *pPlayer=pActor->GetEntity()->GetScriptTable();
	//int loadingSaveGame=m_pGameFramework->IsLoadingSaveGame()?1:0;
	//CallScript(m_serverStateScript, "OnClientEnteredGame", channelId, pPlayer, isReset, loadingSaveGame);

	m_pScript->CallMethod("OnClientEnteredGame", channelId, pActor->GetEntityId(), isReset, m_pGameFramework->IsLoadingSaveGame());

	// Need to update the time of day serialization chunk so that the new client can start at the right point
	// Note: Since we don't generally have a dynamic time of day, this will likely only effect clients
	// rejoining after a host migration since they won't be loading the value from the level
	CHANGED_NETWORK_STATE(this, eEA_GameServerDynamic);
	CHANGED_NETWORK_STATE(this, eEA_GameServerStatic);
	
	return true;
}

//------------------------------------------------------------------------
void CGameRules::OnEntitySpawn(IEntity *pEntity)
{
}

//------------------------------------------------------------------------
void CGameRules::OnEntityRemoved(IEntity *pEntity)
{
	if (gEnv->IsClient())
		SetTeam(0, pEntity->GetId());
}

//------------------------------------------------------------------------
void CGameRules::OnItemDropped(EntityId itemId, EntityId actorId)
{
	//ScriptHandle itemIdHandle(itemId);
	//ScriptHandle actorIdHandle(actorId);
	//CallScript(m_serverStateScript, "OnItemDropped", itemIdHandle, actorIdHandle);

	m_pScript->CallMethod("OnItemDropped", itemId, actorId);
}

//------------------------------------------------------------------------
void CGameRules::OnItemPickedUp(EntityId itemId, EntityId actorId)
{
	//ScriptHandle itemIdHandle(itemId);
	//ScriptHandle actorIdHandle(actorId);
	//CallScript(m_serverStateScript, "OnItemPickedUp", itemIdHandle, actorIdHandle);

	m_pScript->CallMethod("OnItemPickedUp", itemId, actorId);
}

//------------------------------------------------------------------------
void CGameRules::OnTextMessage(ETextMessageType type, const char *msg,
															 const char *p0, const char *p1, const char *p2, const char *p3)
{
	switch(type)
	{
	case eTextMessageConsole:
		CryLogAlways("%s", msg);
		break;
	case eTextMessageServer:
		{
			string completeMsg("** Server: ");
			completeMsg.append(msg);
			completeMsg.append(" **");

			CryLogAlways("[server] %s", msg);
		}
		break;
	case eTextMessageError:
			break;
	case eTextMessageInfo:
		break;
	case eTextMessageCenter:
		break;
	}
}

//------------------------------------------------------------------------
void CGameRules::OnChatMessage(EChatMessageType type, EntityId sourceId, EntityId targetId, const char *msg, bool teamChatOnly)
{
	//send chat message to hud
	int teamFaction = 0;
	if(IActor *pActor = gEnv->pGame->GetIGameFramework()->GetClientActor())
	{
		if(pActor->GetEntityId() != sourceId)
		{
			if(GetTeamCount() > 1)
			{
				if(GetTeam(pActor->GetEntityId()) == GetTeam(sourceId))
					teamFaction = 1;
				else
					teamFaction = 2;
			}
			else
				teamFaction = 2;
		}
	}	
}

//------------------------------------------------------------------------
void CGameRules::OnRevive(IActor *pActor, const Vec3 &pos, const Quat &rot, int teamId)
{
	//ScriptHandle handle(pActor->GetEntityId());
	//Vec3 rotVec = Vec3(Ang3(rot));
	//CallScript(m_clientScript, "OnRevive", handle, pos, rotVec, teamId);

	m_pScript->CallMethod("OnRevive", pActor->GetEntityId(), pos, Ang3(rot), teamId);
}

//------------------------------------------------------------------------
void CGameRules::OnKill(IActor *pActor, EntityId shooterId, const char *weaponClassName, int damage, int material, int hit_type)
{
	//ScriptHandle handleEntity(pActor->GetEntityId()), handleShooter(shooterId);
	//CallScript(m_clientStateScript, "OnKill", handleEntity, handleShooter, weaponClassName, damage, material, hit_type);

	m_pScript->CallMethod("OnKill", pActor->GetEntityId(), shooterId, weaponClassName, damage, material, hit_type);
}

//------------------------------------------------------------------------
void CGameRules::OnReviveInVehicle(IActor *pActor, EntityId vehicleId, int seatId, int teamId)
{
	SGameObjectEvent evt(eCGE_ActorRevive,eGOEF_ToAll, IGameObjectSystem::InvalidExtensionID, (void*)pActor);
	
	//ScriptHandle handle(pActor->GetEntityId());
	//ScriptHandle vhandle(pActor->GetEntityId());
	//CallScript(m_clientScript, "OnReviveInVehicle", handle, vhandle, seatId, teamId);

	m_pScript->CallMethod("OnReviveInVehicle", pActor->GetEntityId(), vehicleId, seatId, teamId);
}

//------------------------------------------------------------------------
void CGameRules::OnKillMessage(EntityId targetId, EntityId shooterId, const char *weaponClassName, float damage, int material, int hit_type)
{
	if(EntityId client_id = g_pGame->GetIGameFramework()->GetClientActor()?g_pGame->GetIGameFramework()->GetClientActor()->GetEntityId():0)
	{
		if(!gEnv->bServer && gEnv->IsClient() && client_id == shooterId && client_id != targetId)
		{
			m_pGameplayRecorder->Event(gEnv->pGame->GetIGameFramework()->GetClientActor()->GetEntity(), GameplayEvent(eGE_Kill, weaponClassName)); 
		}
	}	
}

//------------------------------------------------------------------------
void CGameRules::RevivePlayer(IActor *pActor, const Vec3 &pos, const Quat &angles, int teamId, bool clearInventory)
{
	// get out of vehicles before reviving
	if (IVehicle *pVehicle=pActor->GetLinkedVehicle())
	{
		if (IVehicleSeat *pSeat=pVehicle->GetSeatForPassenger(pActor->GetEntityId()))
			pSeat->Exit(false);
	}

	float health = 100;
	if(!gEnv->bMultiplayer && pActor->IsClient())
		health = g_pGameCVars->g_playerHealthValue;
	pActor->SetMaxHealth(health);

	if (!m_pGameFramework->IsChannelOnHold(pActor->GetChannelId()))
		pActor->GetGameObject()->SetAspectProfile(eEA_Physics, eAP_Alive);

	Matrix34 tm(pActor->GetEntity()->GetWorldTM());
	tm.SetTranslation(pos);

	pActor->GetEntity()->SetWorldTM(tm);

	if (clearInventory)
	{
		if(IInventory *pInventory=pActor->GetInventory())
		{
			pInventory->Destroy();
			pInventory->Clear();
		}
	}

	m_pGameplayRecorder->Event(pActor->GetEntity(), GameplayEvent(eGE_Revive));

	OnRevive(pActor, pos, angles, teamId);
}

//------------------------------------------------------------------------
void CGameRules::RevivePlayerInVehicle(IActor *pActor, EntityId vehicleId, int seatId, int teamId/* =0 */, bool clearInventory/* =true */)
{
	// might get here with an invalid (-ve) seat id if all seats are currently occupied. 
	// In that case we use the seat exit code to find a valid position to spawn at.
	if(seatId < 0)
	{
		IVehicle* pSpawnVehicle = g_pGame->GetIGameFramework()->GetIVehicleSystem()->GetVehicle(vehicleId);
		Vec3 pos = ZERO;
		if(pSpawnVehicle && pSpawnVehicle->GetExitPositionForActor(pActor, pos, true))
		{
			RevivePlayer(pActor, pos, pSpawnVehicle->GetEntity()->GetWorldRotation(), teamId, clearInventory);
			return;
		}
	}

	if (IVehicle *pVehicle=pActor->GetLinkedVehicle())
	{
		if (IVehicleSeat *pSeat=pVehicle->GetSeatForPassenger(pActor->GetEntityId()))
			pSeat->Exit(false); 
	}

	pActor->SetHealth(100);
	pActor->SetMaxHealth(100);

	if (!m_pGameFramework->IsChannelOnHold(pActor->GetChannelId()))
		pActor->GetGameObject()->SetAspectProfile(eEA_Physics, eAP_Alive);

	if (clearInventory)
	{
		IInventory *pInventory=pActor->GetInventory();
		pInventory->Destroy();
		pInventory->Clear();
	}

	// "soft" reset the AI
	gEnv->pAISystem->Reset(IAISystem::RESET_ENTER_GAME);

	// re-enable player
	if ( pActor->GetEntity()->GetAI() && !pActor->GetEntity()->GetAI()->IsEnabled() )
		pActor->GetEntity()->GetAI()->Event(AIEVENT_ENABLE, NULL);

	m_pGameplayRecorder->Event(pActor->GetEntity(), GameplayEvent(eGE_Revive));
}

//------------------------------------------------------------------------
void CGameRules::RenamePlayer(IActor *pActor, const char *name)
{
	string fixed=VerifyName(name, pActor->GetEntity());
	RenameEntityParams params(pActor->GetEntityId(), fixed.c_str());
	if (!stricmp(fixed.c_str(), pActor->GetEntity()->GetName()))
		return;

	if (gEnv->bServer)
	{
		if (!gEnv->IsClient())
			pActor->GetEntity()->SetName(fixed.c_str());

		GetGameObject()->InvokeRMIWithDependentObject(ClRenameEntity(), params, eRMI_ToAllClients, params.entityId);

		if (INetChannel* pNetChannel = pActor->GetGameObject()->GetNetChannel())
			pNetChannel->SetNickname(fixed.c_str());

		m_pGameplayRecorder->Event(pActor->GetEntity(), GameplayEvent(eGE_Renamed, fixed));
	}
	else if (pActor->GetEntityId() == m_pGameFramework->GetClientActor()->GetEntityId())
		GetGameObject()->InvokeRMIWithDependentObject(SvRequestRename(), params, eRMI_ToServer, params.entityId);
}

//------------------------------------------------------------------------
string CGameRules::VerifyName(const char *name, IEntity *pEntity)
{
	string nameFormatter(name);

	// size limit is 26
	if (nameFormatter.size()>26)
		nameFormatter.resize(26);

	// no spaces at start/end
	nameFormatter.TrimLeft(' ');
	nameFormatter.TrimRight(' ');

	// no empty names
	if (nameFormatter.empty())
		nameFormatter="empty";

	// no @ signs
	nameFormatter.replace("@", "_");

	// search for duplicates
	if (IsNameTaken(nameFormatter.c_str(), pEntity))
	{
		int n=1;
		string appendix;
		do 
		{
			appendix.Format("(%d)", n++);
		} while(IsNameTaken(nameFormatter+appendix));

		nameFormatter.append(appendix);
	}

	return nameFormatter;
}

//------------------------------------------------------------------------
bool CGameRules::IsNameTaken(const char *name, IEntity *pEntity)
{
	for (std::vector<int>::const_iterator it=m_channelIds.begin(); it!=m_channelIds.end(); ++it)
	{
		IActor *pActor=GetActorByChannelId(*it);
		if (pActor && pActor->GetEntity()!=pEntity && !stricmp(name, pActor->GetEntity()->GetName()))
			return true;
	}

	return false;
}

//------------------------------------------------------------------------
void CGameRules::ChangeTeam(IActor *pActor, int teamId)
{
	if (teamId == GetTeam(pActor->GetEntityId()))
		return;

	ChangeTeamParams params(pActor->GetEntityId(), teamId);

	if (gEnv->bServer)
	{
		//ScriptHandle handle(params.entityId);
		//CallScript(m_serverStateScript, "OnChangeTeam", handle, params.teamId);

		m_pScript->CallMethod("OnChangeTeam", params.entityId, params.teamId);
	}
	else if (pActor->GetEntityId() == m_pGameFramework->GetClientActor()->GetEntityId())
		GetGameObject()->InvokeRMIWithDependentObject(SvRequestChangeTeam(), params, eRMI_ToServer, params.entityId);
}

//------------------------------------------------------------------------
void CGameRules::ChangeTeam(IActor *pActor, const char *teamName)
{
	if (!teamName)
		return;

	int teamId=GetTeamId(teamName);

	if (!teamId)
	{
		CryLogAlways("Invalid team: %s", teamName);
		return;
	}

	ChangeTeam(pActor, teamId);
}

//------------------------------------------------------------------------
int CGameRules::GetPlayerCount(bool inGame) const
{
	if (!inGame)
		return (int)m_channelIds.size();

	int count=0;
	for (std::vector<int>::const_iterator it=m_channelIds.begin(); it!=m_channelIds.end(); ++it)
	{
		if (IsChannelInGame(*it))
			++count;
	}

	return count;
}

//------------------------------------------------------------------------
EntityId CGameRules::GetPlayer(int idx)
{
	if (idx<0||idx>=m_channelIds.size())
		return 0;

	IActor *pActor=GetActorByChannelId(m_channelIds[idx]);
	return pActor?pActor->GetEntityId():0;
}

//------------------------------------------------------------------------
void CGameRules::GetPlayers(TPlayers &players)
{
	players.resize(0);
	players.reserve(m_channelIds.size());

	for (std::vector<int>::const_iterator it=m_channelIds.begin(); it!=m_channelIds.end(); ++it)
	{
		IActor *pActor=GetActorByChannelId(*it);
		if (pActor)
			players.push_back(pActor->GetEntityId());
	}
}

//------------------------------------------------------------------------
bool CGameRules::IsPlayerInGame(EntityId playerId) const
{
	const bool isLocalPlayer = (playerId == m_pGameFramework->GetClientActorId());
	INetChannel* pNetChannel = NULL;

	if(isLocalPlayer)
	{
		pNetChannel = g_pGame->GetIGameFramework()->GetClientChannel();
	}
	else
	{
		pNetChannel = g_pGame->GetIGameFramework()->GetNetChannel(GetChannelId(playerId));
	}

	if (pNetChannel && pNetChannel->GetContextViewState()>=eCVS_InGame)
		return true;

	return false;
}

//------------------------------------------------------------------------
bool CGameRules::IsPlayerActivelyPlaying(EntityId playerId) const
{
	if(!gEnv->bMultiplayer)
		return true;

	// 'actively playing' means they have selected a team / joined the game.

	if(GetTeamCount() == 1)
	{
		IActor* pActor = reinterpret_cast<IActor*>(g_pGame->GetIGameFramework()->GetIActorSystem()->GetActor(playerId));
		if(!pActor) 
			return false;

		return pActor->GetHealth() >= 0;
	}
	else
	{
		// in PS, out of the game if not yet on a team
		return (GetTeam(playerId) != 0);
	}
}

//------------------------------------------------------------------------
bool CGameRules::IsChannelInGame(int channelId) const
{
	INetChannel *pNetChannel=g_pGame->GetIGameFramework()->GetNetChannel(channelId);
	if (pNetChannel && pNetChannel->GetContextViewState()>=eCVS_InGame)
		return true;
	return false;
}

//------------------------------------------------------------------------
int CGameRules::CreateTeam(const char *name)
{
	TTeamIdMap::iterator it = m_teams.find(CONST_TEMP_STRING(name));
	if (it != m_teams.end())
		return it->second;

	m_teams.insert(TTeamIdMap::value_type(name, ++m_teamIdGen));
	m_playerteams.insert(TPlayerTeamIdMap::value_type(m_teamIdGen, TPlayers()));

	return m_teamIdGen;
}

//------------------------------------------------------------------------
void CGameRules::RemoveTeam(int teamId)
{
	TTeamIdMap::iterator it = m_teams.find(CONST_TEMP_STRING(GetTeamName(teamId)));
	if (it == m_teams.end())
		return;

	m_teams.erase(it);

	for (TEntityTeamIdMap::iterator eit=m_entityteams.begin(); eit != m_entityteams.end(); ++eit)
	{
		if (eit->second == teamId)
			eit->second = 0; // 0 is no team
	}

	m_playerteams.erase(m_playerteams.find(teamId));
}

//------------------------------------------------------------------------
const char *CGameRules::GetTeamName(int teamId) const
{
	for (TTeamIdMap::const_iterator it = m_teams.begin(); it!=m_teams.end(); ++it)
	{
		if (teamId == it->second)
			return it->first;
	}

	return 0;
}

//------------------------------------------------------------------------
int CGameRules::GetTeamId(const char *name) const
{
	TTeamIdMap::const_iterator it=m_teams.find(CONST_TEMP_STRING(name));
	if (it!=m_teams.end())
		return it->second;

	return 0;
}

//------------------------------------------------------------------------
int CGameRules::GetTeamCount() const
{
	return (int)m_teams.size();
}

//------------------------------------------------------------------------
int CGameRules::GetTeamPlayerCount(int teamId, bool inGame) const
{
	if (!inGame)
	{
		TPlayerTeamIdMap::const_iterator it=m_playerteams.find(teamId);
		if (it!=m_playerteams.end())
			return (int)it->second.size();
		return 0;
	}
	else
	{
		TPlayerTeamIdMap::const_iterator it=m_playerteams.find(teamId);
		if (it!=m_playerteams.end())
		{
			int count=0;

			const TPlayers &players=it->second;
			for (TPlayers::const_iterator pit=players.begin(); pit!=players.end(); ++pit)
				if (IsPlayerInGame(*pit))
					++count;

			return count;
		}
		return 0;
	}
}

//------------------------------------------------------------------------
int CGameRules::GetTeamChannelCount(int teamId, bool inGame) const
{
	int count=0;
	for (TChannelTeamIdMap::const_iterator it=m_channelteams.begin(); it!=m_channelteams.end(); ++it)
	{
		if (teamId==it->second)
		{
			if (!inGame || IsChannelInGame(it->first))
				++count;
		}
	}

	return count;
}

//------------------------------------------------------------------------
EntityId CGameRules::GetTeamPlayer(int teamId, int idx)
{
	TPlayerTeamIdMap::const_iterator it=m_playerteams.find(teamId);
	if (it!=m_playerteams.end())
	{
		if (idx>=0 && idx<it->second.size())
			return it->second[idx];
	}

	return 0;
}

//------------------------------------------------------------------------
void CGameRules::GetTeamPlayers(int teamId, TPlayers &players)
{
	players.resize(0);
	TPlayerTeamIdMap::const_iterator it=m_playerteams.find(teamId);
	if (it!=m_playerteams.end())
		players=it->second;
}

//------------------------------------------------------------------------
void CGameRules::SetTeam(int teamId, EntityId id)
{
	if (!gEnv->bServer )
	{
		assert(0);
		return;
	}

	int oldTeam = GetTeam(id);
	if (oldTeam==teamId)
		return;

	TEntityTeamIdMap::iterator it=m_entityteams.find(id);
	if (it!=m_entityteams.end())
		m_entityteams.erase(it);

	IActor *pActor=m_pActorSystem->GetActor(id);
	bool isplayer=pActor!=0;
	if (isplayer && oldTeam)
	{	
		TPlayerTeamIdMap::iterator pit=m_playerteams.find(oldTeam);
		assert(pit!=m_playerteams.end());
		stl::find_and_erase(pit->second, id);
	}

	if (teamId)
	{
		m_entityteams.insert(TEntityTeamIdMap::value_type(id, teamId));

		if (isplayer)
		{
			TPlayerTeamIdMap::iterator pit=m_playerteams.find(teamId);
			assert(pit!=m_playerteams.end());
			pit->second.push_back(id);
		}
	}

	if(isplayer)
	{
		int channelId=GetChannelId(id);

		TChannelTeamIdMap::iterator itChannels=m_channelteams.find(channelId);
		if (itChannels!=m_channelteams.end())
		{
			if (!teamId)
				m_channelteams.erase(itChannels);
			else
				itChannels->second=teamId;
		}
		else if(teamId)
			m_channelteams.insert(TChannelTeamIdMap::value_type(channelId, teamId));
	}

	{
		//ScriptHandle handle(id);
		//CallScript(m_serverStateScript, "OnSetTeam", handle, teamId);

		m_pScript->CallMethod("OnSetTeam", id, teamId);
	}

	//if (gEnv->IsClient())
	{
		//ScriptHandle handle(id);
		//CallScript(m_clientStateScript, "OnSetTeam", handle, teamId);
	}
	
	GetGameObject()->InvokeRMIWithDependentObject(ClSetTeam(), SetTeamParams(id, teamId), eRMI_ToRemoteClients, id);

	if (IEntity *pEntity=m_pEntitySystem->GetEntity(id))
		m_pGameplayRecorder->Event(pEntity, GameplayEvent(eGE_ChangedTeam, 0, (float)teamId));
}

//------------------------------------------------------------------------
int CGameRules::GetTeam(EntityId entityId) const
{
	TEntityTeamIdMap::const_iterator it = m_entityteams.find(entityId);
	if (it != m_entityteams.end())
		return it->second;

	return 0;
}

//------------------------------------------------------------------------
int CGameRules::GetChannelTeam(int channelId) const
{
	TChannelTeamIdMap::const_iterator it = m_channelteams.find(channelId);
	if (it != m_channelteams.end())
		return it->second;

	return 0;
}

//------------------------------------------------------------------------
void CGameRules::AddGameRulesListener(SGameRulesListener* pRulesListener)
{
	stl::push_back_unique(m_rulesListeners, pRulesListener);
}

//------------------------------------------------------------------------
void CGameRules::RemoveGameRulesListener(SGameRulesListener* pRulesListener)
{
	stl::find_and_erase(m_rulesListeners, pRulesListener);
}

//------------------------------------------------------------------------
void CGameRules::SendTextMessage(ETextMessageType type, const char *msg, unsigned int to, int channelId,
																 const char *p0, const char *p1, const char *p2, const char *p3)
{
	GetGameObject()->InvokeRMI(ClTextMessage(), TextMessageParams(type, msg, p0, p1, p2, p3), to, channelId);
}

//------------------------------------------------------------------------
bool CGameRules::CanReceiveChatMessage(EChatMessageType type, EntityId sourceId, EntityId targetId) const
{
	if(sourceId == targetId)
		return true;

	bool sspec=!IsPlayerActivelyPlaying(sourceId);
	bool sdead=IsDead(sourceId);

	bool tspec=!IsPlayerActivelyPlaying(targetId);
	bool tdead=IsDead(targetId);

	if(sdead != tdead)
	{
		//CryLog("Disallowing msg (dead): source %d, target %d, sspec %d, sdead %d, tspec %d, tdead %d", sourceId, targetId, sspec, sdead, tspec, tdead);
		return false;
	}

	if(!(tspec || (sspec==tspec)))
	{
		//CryLog("Disallowing msg (spec): source %d, target %d, sspec %d, sdead %d, tspec %d, tdead %d", sourceId, targetId, sspec, sdead, tspec, tdead);
		return false;
	}

	//CryLog("Allowing msg: source %d, target %d, sspec %d, sdead %d, tspec %d, tdead %d", sourceId, targetId, sspec, sdead, tspec, tdead);
	return true;
}

//------------------------------------------------------------------------
void CGameRules::ChatLog(EChatMessageType type, EntityId sourceId, EntityId targetId, const char *msg)
{
	IEntity * pSource = gEnv->pEntitySystem->GetEntity(sourceId);
	IEntity * pTarget = gEnv->pEntitySystem->GetEntity(targetId);
	const char * sourceName = pSource? pSource->GetName() : "<unknown>";
	const char * targetName = pTarget? pTarget->GetName() : "<unknown>";
	int teamId = GetTeam(sourceId);

	char tempBuffer[64];

	switch (type)
	{
	case eChatToTeam:
		if (teamId)
		{
			targetName = tempBuffer;
			sprintf(tempBuffer, "Team %s", GetTeamName(teamId));
		}
		else
		{
	case eChatToAll:
			targetName = "ALL";
		}
		break;
	}

	CryLogAlways("CHAT %s to %s:", sourceName, targetName);
	CryLogAlways("   %s", msg);
}

//------------------------------------------------------------------------
void CGameRules::SendChatMessage(EChatMessageType type, EntityId sourceId, EntityId targetId, const char *msg)
{
	ChatMessageParams params(type, sourceId, targetId, msg, (type == eChatToTeam)?true:false);

	bool sdead=IsDead(sourceId);

	ChatLog(type, sourceId, targetId, msg);

	if (gEnv->bServer)
	{
		switch(type)
		{
		case eChatToTarget:
			{
				if (CanReceiveChatMessage(type, sourceId, targetId))
					GetGameObject()->InvokeRMIWithDependentObject(ClChatMessage(), params, eRMI_ToClientChannel, targetId, GetChannelId(targetId));
			}
			break;
		case eChatToAll:
			{
				std::vector<int>::const_iterator begin=m_channelIds.begin();
				std::vector<int>::const_iterator end=m_channelIds.end();

				for (std::vector<int>::const_iterator it=begin; it!=end; ++it)
				{
					if (IActor *pActor=GetActorByChannelId(*it))
					{
						if (CanReceiveChatMessage(type, sourceId, pActor->GetEntityId()) && IsPlayerInGame(pActor->GetEntityId()))
							GetGameObject()->InvokeRMIWithDependentObject(ClChatMessage(), params, eRMI_ToClientChannel, pActor->GetEntityId(), *it);
					}
				}
			}
			break;
		case eChatToTeam:
			{
				int teamId = GetTeam(sourceId);
				if (teamId)
				{
					TPlayerTeamIdMap::const_iterator tit=m_playerteams.find(teamId);
					if (tit!=m_playerteams.end())
					{
						TPlayers::const_iterator begin=tit->second.begin();
						TPlayers::const_iterator end=tit->second.end();

						for (TPlayers::const_iterator it=begin; it!=end; ++it)
						{
							if (CanReceiveChatMessage(type, sourceId, *it))
								GetGameObject()->InvokeRMIWithDependentObject(ClChatMessage(), params, eRMI_ToClientChannel, *it, GetChannelId(*it));
						}
					}
				}
			}
			break;
		}
	}
	else
		GetGameObject()->InvokeRMI(SvRequestChatMessage(), params, eRMI_ToServer);
}

//------------------------------------------------------------------------
void CGameRules::ForbiddenAreaWarning(bool active, int timer, EntityId targetId)
{
	GetGameObject()->InvokeRMI(ClForbiddenAreaWarning(), ForbiddenAreaWarningParams(active, timer), eRMI_ToClientChannel, GetChannelId(targetId));
}

//------------------------------------------------------------------------

void CGameRules::ResetGameTime()
{
	m_endTime.SetSeconds(0.0f);

	float timeLimit=g_pGameCVars->g_timelimit;
	if (timeLimit>0.0f)
		m_endTime.SetSeconds(m_pGameFramework->GetServerTime().GetSeconds()+timeLimit*60.0f);

	GetGameObject()->InvokeRMI(ClSetGameTime(), SetGameTimeParams(m_endTime), eRMI_ToRemoteClients);
}

//------------------------------------------------------------------------
float CGameRules::GetRemainingGameTime() const
{
	return MAX(0, (m_endTime-m_pGameFramework->GetServerTime()).GetSeconds());
}

//------------------------------------------------------------------------
void CGameRules::SetRemainingGameTime(float seconds)
{
}
//------------------------------------------------------------------------
void CGameRules::ClearAllMigratingPlayers(void)
{
}
//------------------------------------------------------------------------
EntityId CGameRules::SetChannelForMigratingPlayer(const char* name, uint16 channelID)
{
	return 0;
}
//------------------------------------------------------------------------
void CGameRules::StoreMigratingPlayer(IActor* pActor)
{
}

//------------------------------------------------------------------------
bool CGameRules::IsClientFriendlyProjectile(const EntityId projectileId, const EntityId targetEntityId)
{
	return false;
}

//------------------------------------------------------------------------
bool CGameRules::IsTimeLimited() const
{
	return m_endTime.GetSeconds()>0.0f;
}

//------------------------------------------------------------------------
void CGameRules::ResetRoundTime()
{
	m_roundEndTime.SetSeconds(0.0f);

	float roundTime=g_pGameCVars->g_roundtime;
	if (roundTime>0.0f)
		m_roundEndTime.SetSeconds(m_pGameFramework->GetServerTime().GetSeconds()+roundTime*60.0f);

	GetGameObject()->InvokeRMI(ClSetRoundTime(), SetGameTimeParams(m_roundEndTime), eRMI_ToRemoteClients);
}

//------------------------------------------------------------------------
float CGameRules::GetRemainingRoundTime() const
{
	return MAX(0, (m_roundEndTime-m_pGameFramework->GetServerTime()).GetSeconds());
}

//------------------------------------------------------------------------
bool CGameRules::IsRoundTimeLimited() const
{
	return m_roundEndTime.GetSeconds()>0.0f;
}

//------------------------------------------------------------------------
void CGameRules::ResetPreRoundTime()
{
	m_preRoundEndTime.SetSeconds(0.0f);

	int preRoundTime=g_pGameCVars->g_preroundtime;
	if (preRoundTime>0)
		m_preRoundEndTime.SetSeconds(m_pGameFramework->GetServerTime().GetSeconds()+preRoundTime);

	GetGameObject()->InvokeRMI(ClSetPreRoundTime(), SetGameTimeParams(m_preRoundEndTime), eRMI_ToRemoteClients);
}

//------------------------------------------------------------------------
float CGameRules::GetRemainingPreRoundTime() const
{
	return MAX(0, (m_preRoundEndTime-m_pGameFramework->GetServerTime()).GetSeconds());
}

//------------------------------------------------------------------------
void CGameRules::ResetReviveCycleTime()
{
	if (!gEnv->bServer)
	{
		GameWarning("CGameRules::ResetReviveCycleTime() called on client");
		return;
	}

	m_reviveCycleEndTime.SetSeconds(0.0f);

	if (g_pGameCVars->g_revivetime<5)
		gEnv->pConsole->GetCVar("g_revivetime")->Set(5);

	m_reviveCycleEndTime = m_pGameFramework->GetServerTime() + float(g_pGameCVars->g_revivetime);

	GetGameObject()->InvokeRMI(ClSetReviveCycleTime(), SetGameTimeParams(m_reviveCycleEndTime), eRMI_ToRemoteClients);
}

//------------------------------------------------------------------------
float CGameRules::GetRemainingReviveCycleTime() const
{
	return MAX(0, (m_reviveCycleEndTime-m_pGameFramework->GetServerTime()).GetSeconds());
}


//------------------------------------------------------------------------
void CGameRules::ResetGameStartTimer(float time)
{
	if (!gEnv->bServer)
	{
		GameWarning("CGameRules::ResetGameStartTimer() called on client");
		return;
	}

	m_gameStartTime = m_pGameFramework->GetServerTime() + time;

	GetGameObject()->InvokeRMI(ClSetGameStartTimer(), SetGameTimeParams(m_gameStartTime), eRMI_ToRemoteClients);
}

//------------------------------------------------------------------------
float CGameRules::GetRemainingStartTimer() const
{
	return (m_gameStartTime-m_pGameFramework->GetServerTime()).GetSeconds();
}

//------------------------------------------------------------------------
bool CGameRules::OnCollision(const SGameCollision& event)
{
	FUNCTION_PROFILER(GetISystem(), PROFILE_GAME);
	// currently this function only calls server functions
	// prevent unnecessary script callbacks on the client
	if (!gEnv->bServer || IsDemoPlayback())
		return true; 

	// filter out self-collisions
	if (event.pSrcEntity == event.pTrgEntity)
		return true;

	// collisions involving partId<-1 are to be ignored by game's damage calculations
	// usually created articially to make stuff break. See CMelee::Impulse
	if (event.pCollision->partid[0]<-1||event.pCollision->partid[1]<-1)
		return true;

	m_pScript->CallMethod("OnCollision", event.pSrcEntity ? event.pSrcEntity->GetId() : 0, event.pTrgEntity ? event.pTrgEntity->GetId() : 0, event.pCollision->pt, event.pCollision->vloc[0].GetNormalizedSafe(), event.pCollision->idmat[0], event.pCollision->n);

	return true;
}

//------------------------------------------------------------------------
void CGameRules::RegisterConsoleCommands(IConsole *pConsole)
{
	// todo: move to power struggle implementation when there is one
	REGISTER_COMMAND("buy",			"if (g_gameRules and g_gameRules.Buy) then g_gameRules:Buy(%1); end",VF_NULL,"");
	REGISTER_COMMAND("buyammo", "if (g_gameRules and g_gameRules.BuyAmmo) then g_gameRules:BuyAmmo(%%); end",VF_NULL,"");
	REGISTER_COMMAND("g_debug_teams", CmdDebugTeams,VF_NULL,"");
}

//------------------------------------------------------------------------
void CGameRules::UnregisterConsoleCommands(IConsole *pConsole)
{
	pConsole->RemoveCommand("buy");
	pConsole->RemoveCommand("buyammo");
	pConsole->RemoveCommand("g_debug_spawns");
	pConsole->RemoveCommand("g_debug_minimap");
	pConsole->RemoveCommand("g_debug_teams");
	pConsole->RemoveCommand("g_debug_objectives");
}

//------------------------------------------------------------------------
void CGameRules::RegisterConsoleVars(IConsole *pConsole)
{
}

//------------------------------------------------------------------------
void CGameRules::CmdDebugTeams(IConsoleCmdArgs *pArgs)
{
	CGameRules *pGameRules=g_pGame->GetGameRules();
	if (!pGameRules->m_entityteams.empty())
	{
		CryLogAlways("// Teams //");
		for (TTeamIdMap::const_iterator tit=pGameRules->m_teams.begin(); tit!=pGameRules->m_teams.end(); ++tit)
		{
			CryLogAlways("Team: %s  (id: %d)", tit->first.c_str(), tit->second);
			for (TEntityTeamIdMap::const_iterator eit=pGameRules->m_entityteams.begin(); eit!=pGameRules->m_entityteams.end(); ++eit)
			{
				if (eit->second==tit->second)
				{
					IEntity *pEntity=gEnv->pEntitySystem->GetEntity(eit->first);
					CryLogAlways("    -> Entity: %s  class: %s  (eid: %d %08x)", pEntity?pEntity->GetName():"<null>", pEntity?pEntity->GetClass()->GetName():"<null>", eit->first, eit->first);
				}
			}
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::ShowScores(bool show)
{
	//CallScript(m_script, "ShowScores", show);
	m_pScript->CallMethod("ShowScores", show);
}

//------------------------------------------------------------------------
void CGameRules::UpdateAffectedEntitiesSet(TExplosionAffectedEntities &affectedEnts, const pe_explosion *pExplosion)
{
	if (pExplosion)
	{
		for (int i=0; i<pExplosion->nAffectedEnts; ++i)
		{ 
			if (IEntity *pEntity = gEnv->pEntitySystem->GetEntityFromPhysics(pExplosion->pAffectedEnts[i]))
			{ 
				if (IScriptTable *pEntityTable = pEntity->GetScriptTable())
				{
					IPhysicalEntity* pEnt = pEntity->GetPhysics();
					if (pEnt)
					{
						float affected=gEnv->pPhysicalWorld->IsAffectedByExplosion(pEnt);

						AddOrUpdateAffectedEntity(affectedEnts, pEntity, affected);
					}
				}
			}
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::AddOrUpdateAffectedEntity(TExplosionAffectedEntities &affectedEnts, IEntity* pEntity, float affected)
{
	TExplosionAffectedEntities::iterator it=affectedEnts.find(pEntity);
	if (it!=affectedEnts.end())
	{
		if (it->second<affected)
			it->second=affected;
	}
	else
		affectedEnts.insert(TExplosionAffectedEntities::value_type(pEntity, affected));
}

//------------------------------------------------------------------------
void CGameRules::CommitAffectedEntitiesSet(SmartScriptTable &scriptExplosionInfo, TExplosionAffectedEntities &affectedEnts)
{
	CScriptSetGetChain explosion(scriptExplosionInfo);

	SmartScriptTable affected;
	SmartScriptTable affectedObstruction;

	if (scriptExplosionInfo->GetValue("AffectedEntities", affected) && 
		scriptExplosionInfo->GetValue("AffectedEntitiesObstruction", affectedObstruction))
	{
		if (affectedEnts.empty())
		{
			affected->Clear();
			affectedObstruction->Clear();
		}
		else
		{
			int k=0;      
			for (TExplosionAffectedEntities::const_iterator it=affectedEnts.begin(),end=affectedEnts.end(); it!=end; ++it)
			{
				float obstruction = 1.0f-it->second;
				affected->SetAt(++k, it->first->GetScriptTable());
				affectedObstruction->SetAt(k, obstruction);
			}
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::Restart()
{
	if (gEnv->bServer)
		//CallScript(m_script, "RestartGame", true);
		m_pScript->CallMethod("RestartGame", true);
}

//------------------------------------------------------------------------
void CGameRules::NextLevel()
{
  if (!gEnv->bServer)
    return;

	ILevelRotation *pLevelRotation=m_pGameFramework->GetILevelSystem()->GetLevelRotation();
	if (!pLevelRotation->GetLength())
		Restart();
	else
		pLevelRotation->ChangeLevel();
}

//------------------------------------------------------------------------
void CGameRules::ResetEntities()
{
	m_respawns.clear();
	m_entityteams.clear();
	m_teamdefaultspawns.clear();

	for (TPlayerTeamIdMap::iterator tit=m_playerteams.begin(); tit!=m_playerteams.end(); tit++)
		tit->second.resize(0);

	g_pGame->GetIGameFramework()->Reset(gEnv->bServer);

//	SEntityEvent event(ENTITY_EVENT_START_GAME);
//	gEnv->pEntitySystem->SendEventToAll(event);
}

//------------------------------------------------------------------------
void CGameRules::OnEndGame()
{
	bool isMultiplayer=gEnv->bMultiplayer ;

#ifndef OLD_VOICE_SYSTEM_DEPRECATED
	if (isMultiplayer && gEnv->bServer)
		m_teamVoiceGroups.clear();
#endif

	if(gEnv->IsClient())
	{
		IActionMapManager *pActionMapMan = g_pGame->GetIGameFramework()->GetIActionMapManager();
		pActionMapMan->EnableActionMap("multiplayer", !isMultiplayer);
		pActionMapMan->EnableActionMap("singleplayer", isMultiplayer);

		IActionMap *am = NULL;
		if(isMultiplayer)
		{
			am = pActionMapMan->GetActionMap("multiplayer");
		}
		else
		{
			am = pActionMapMan->GetActionMap("singleplayer");
		}
		if(am)
		{
			am->SetActionListener(0);
		}
	}

}

//------------------------------------------------------------------------
void CGameRules::GameOver(int localWinner)
{
	if(m_rulesListeners.empty() == false)
	{
		TGameRulesListenerVec::iterator iter = m_rulesListeners.begin();
		while (iter != m_rulesListeners.end())
		{
			(*iter)->GameOver(localWinner);
			++iter;
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::EnteredGame()
{
	if(m_rulesListeners.empty() == false)
	{
		TGameRulesListenerVec::iterator iter = m_rulesListeners.begin();
		while (iter != m_rulesListeners.end())
		{
			(*iter)->EnteredGame();
			++iter;
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::EndGameNear(EntityId id)
{
	if(m_rulesListeners.empty() == false)
	{
		TGameRulesListenerVec::iterator iter = m_rulesListeners.begin();
		while(iter != m_rulesListeners.end())
		{
			(*iter)->EndGameNear(id);
			++iter;
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::CreateEntityRespawnData(EntityId entityId)
{
	if (!gEnv->bServer || m_pGameFramework->IsEditing())
		return;

	IEntity *pEntity=m_pEntitySystem->GetEntity(entityId);
	if (!pEntity)
		return;

	SEntityRespawnData respawn;
	respawn.position = pEntity->GetWorldPos();
	respawn.rotation = pEntity->GetWorldRotation();
	respawn.scale = pEntity->GetScale();
	respawn.flags = pEntity->GetFlags() & ~ENTITY_FLAG_UNREMOVABLE;
	respawn.pClass = pEntity->GetClass();
#ifdef _DEBUG
	respawn.name = pEntity->GetName();
#endif
	
	IScriptTable *pScriptTable = pEntity->GetScriptTable();

	if (pScriptTable)
		pScriptTable->GetValue("Properties", respawn.properties);

	m_respawndata[entityId] = respawn;
}

//------------------------------------------------------------------------
bool CGameRules::HasEntityRespawnData(EntityId entityId) const
{
	return m_respawndata.find(entityId)!=m_respawndata.end();
}

//------------------------------------------------------------------------
void CGameRules::ScheduleEntityRespawn(EntityId entityId, bool unique, float timer)
{
	if (!gEnv->bServer || m_pGameFramework->IsEditing())
		return;

	IEntity *pEntity=m_pEntitySystem->GetEntity(entityId);
	if (!pEntity)
		return;

	SEntityRespawn respawn;
	respawn.timer = timer;
	respawn.unique = unique;

	m_respawns[entityId] = respawn;
}

//------------------------------------------------------------------------
void CGameRules::UpdateEntitySchedules(float frameTime)
{
	if (!gEnv->bServer || m_pGameFramework->IsEditing())
		return;

	TEntityRespawnMap::iterator next;
	for (TEntityRespawnMap::iterator it=m_respawns.begin(); it!=m_respawns.end(); it=next)
	{
		next=it; ++next;
		EntityId id=it->first;
		SEntityRespawn &respawn=it->second;

		if (respawn.unique)
		{
			IEntity *pEntity=m_pEntitySystem->GetEntity(id);
			if (pEntity && !pEntity->IsGarbage())
				continue;
		}

		respawn.timer -= frameTime;
		if (respawn.timer<=0.0f)
		{
			TEntityRespawnDataMap::iterator dit=m_respawndata.find(id);
			
			if (dit==m_respawndata.end())
      {
        m_respawns.erase(it);
				continue;
      }

			SEntityRespawnData &data=dit->second;

			SEntitySpawnParams params;
			params.pClass=data.pClass;
			params.qRotation=data.rotation;
			params.vPosition=data.position;
			params.vScale=data.scale;
			params.nFlags=data.flags;

			string name;
#ifdef _DEBUG
			name=data.name;
			name.append("_repop");
#else
			name=data.pClass->GetName();
#endif
			params.sName = name.c_str();

			IEntity *pEntity=m_pEntitySystem->SpawnEntity(params, false);
			if (pEntity && data.properties.GetPtr())
			{
				SmartScriptTable properties;
				IScriptTable *pScriptTable=pEntity->GetScriptTable();
				if (pScriptTable && pScriptTable->GetValue("Properties", properties))
				{
					if (properties.GetPtr())
						properties->Clone(data.properties, true);
				}
			}

			m_pEntitySystem->InitEntity(pEntity, params);
			m_respawns.erase(it);
			m_respawndata.erase(dit);
		}
	}

	TEntityRemovalMap::iterator rnext;
	for (TEntityRemovalMap::iterator it=m_removals.begin(); it!=m_removals.end(); it=rnext)
	{
		rnext=it; ++rnext;
		EntityId id=it->first;
		SEntityRemovalData &removal=it->second;

		IEntity *pEntity=m_pEntitySystem->GetEntity(id);
		if (!pEntity)
		{
			m_removals.erase(it);
			continue;
		}

		if (removal.visibility)
		{
			AABB aabb;
			pEntity->GetWorldBounds(aabb);

			CCamera &camera=m_pSystem->GetViewCamera();
			if (camera.IsAABBVisible_F(aabb))
			{
				removal.timer=removal.time;
				continue;
			}
		}

		removal.timer-=frameTime;
		if (removal.timer<=0.0f)
		{
			m_pEntitySystem->RemoveEntity(id);
			m_removals.erase(it);
		}
	}
}

//------------------------------------------------------------------------
void CGameRules::ForceScoreboard(bool force)
{
}

//------------------------------------------------------------------------
void CGameRules::FreezeInput(bool freeze)
{
#if !defined(CRY_USE_GCM_HUD)
	if (gEnv->pInput) gEnv->pInput->ClearKeyState();
#endif

	g_pGameActions->FilterFreezeTime()->Enable(freeze);
/*
	if (IActor *pClientIActor=g_pGame->GetIGameFramework()->GetClientActor())
	{
		IActor *pClientActor=static_cast<IActor *>(pClientIActor);
		if (CWeapon *pWeapon=pClientActor->GetWeapon(pClientActor->GetCurrentItemId()))
			pWeapon->StopFire(pClientActor->GetEntityId());
	}
	*/
}

//------------------------------------------------------------------------
void CGameRules::AbortEntityRespawn(EntityId entityId, bool destroyData)
{
	TEntityRespawnMap::iterator it=m_respawns.find(entityId);
	if (it!=m_respawns.end())
		m_respawns.erase(it);

	if (destroyData)
	{
		TEntityRespawnDataMap::iterator dit=m_respawndata.find(entityId);
		if (dit!=m_respawndata.end())
			m_respawndata.erase(dit);
	}
}

//------------------------------------------------------------------------
void CGameRules::ScheduleEntityRemoval(EntityId entityId, float timer, bool visibility)
{
	if (!gEnv->bServer || m_pGameFramework->IsEditing())
		return;

	IEntity *pEntity=m_pEntitySystem->GetEntity(entityId);
	if (!pEntity)
		return;

	SEntityRemovalData removal;
	removal.time = timer;
	removal.timer = timer;
	removal.visibility = visibility;

	m_removals.insert(TEntityRemovalMap::value_type(entityId, removal));
}

//------------------------------------------------------------------------
void CGameRules::AbortEntityRemoval(EntityId entityId)
{
	TEntityRemovalMap::iterator it=m_removals.find(entityId);
	if (it!=m_removals.end())
		m_removals.erase(it);
}

void CGameRules::ShowStatus()
{
	float timeRemaining = GetRemainingGameTime();
	int mins = (int)(timeRemaining / 60.0f);
	int secs = (int)(timeRemaining - mins*60);
	CryLogAlways("time remaining: %d:%02d", mins, secs);
}

void CGameRules::ForceSynchedStorageSynch(int channel)
{
	if (!gEnv->bServer)
		return;

	g_pGame->GetServerSynchedStorage()->FullSynch(channel, true);
}

void CGameRules::SPNotifyPlayerKill(EntityId targetId, EntityId weaponId, bool bHeadShot)
{
	IActor *pActor = gEnv->pGame->GetIGameFramework()->GetClientActor();
	EntityId wepId[1] = {weaponId};
	if (pActor)
		m_pGameplayRecorder->Event(pActor->GetEntity(), GameplayEvent(eGE_Kill,0,0,wepId)); 
}

string CGameRules::GetPlayerName(int channelId, bool bVerifyName)
{
	string playerName;
	if (INetChannel *pNetChannel=m_pGameFramework->GetNetChannel(channelId))
	{
		playerName=pNetChannel->GetNickname();
		if (!playerName.empty() && bVerifyName)
			playerName=VerifyName(playerName);
	}
	return playerName;
}


void CGameRules::GetMemoryUsage(ICrySizer * s) const
{
	s->Add(*this);
	s->AddContainer(m_channelIds);
	s->AddContainer(m_teams);
	s->AddContainer(m_entityteams);
	s->AddContainer(m_channelteams);
	s->AddContainer(m_teamdefaultspawns);
	s->AddContainer(m_playerteams);
	s->AddContainer(m_respawndata);
	s->AddContainer(m_respawns);
	s->AddContainer(m_removals);
#ifndef OLD_VOICE_SYSTEM_DEPRECATED
	s->AddContainer(m_teamVoiceGroups);
#endif
	s->AddContainer(m_rulesListeners);

	for (TTeamIdMap::const_iterator iter = m_teams.begin(); iter != m_teams.end(); ++iter)
		s->Add(iter->first);
	for (TPlayerTeamIdMap::const_iterator iter = m_playerteams.begin(); iter != m_playerteams.end(); ++iter)
		s->AddContainer(iter->second);
}

bool CGameRules::NetSerialize( TSerialize ser, EEntityAspects aspect, uint8 profile, int flags )
{
		switch (aspect)
		{
		case eEA_GameServerDynamic:
				{	
						uint32 todFlags = 0;
						if (ser.IsReading())
						{
								todFlags |= ITimeOfDay::NETSER_COMPENSATELAG;
								if (!m_timeOfDayInitialized)
								{
										todFlags |= ITimeOfDay::NETSER_FORCESET;
										m_timeOfDayInitialized = true;
								}
						}
						gEnv->p3DEngine->GetTimeOfDay()->NetSerialize( ser, 0.0f, todFlags );
				}
				break;
		case eEA_GameServerStatic:
				{
						gEnv->p3DEngine->GetTimeOfDay()->NetSerialize( ser, 0.0f, ITimeOfDay::NETSER_STATICPROPS );
				}
				break;
		}

		return true;
}

bool CGameRules::OnBeginCutScene(IAnimSequence* pSeq, bool bResetFX)
{
	if(!pSeq)
		return false;

	m_explosionScreenFX = false;

	return true;
}

bool CGameRules::OnEndCutScene(IAnimSequence* pSeq)
{
	if(!pSeq)
		return false;

	m_explosionScreenFX = true;

	return true;
}
