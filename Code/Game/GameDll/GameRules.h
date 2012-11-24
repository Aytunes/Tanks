/*************************************************************************
	Crytek Source File.
	Copyright (C), Crytek Studios, 2001-2004.
	-------------------------------------------------------------------------
	$Id$
	$DateTime$
	Description: 

	-------------------------------------------------------------------------
	History:
	- 7:2:2006   15:38 : Created by Marcio Martins

*************************************************************************/
#ifndef __GAMERULES_H__
#define __GAMERULES_H__

#if _MSC_VER > 1000
# pragma once
#endif

#include "Game.h"
#include <IGameObject.h>
#include <IGameRulesSystem.h>
#include "SynchedStorage.h"
#include <queue>
#include "IViewSystem.h"

#include <MonoCommon.h>

struct IGameObject;
struct IActorSystem;

#define GAMERULES_INVOKE_ON_TEAM(team, rmi, params)	\
{ \
	TPlayerTeamIdMap::const_iterator _team=m_playerteams.find(team); \
	if (_team!=m_playerteams.end()) \
	{ \
	const TPlayers &_players=_team->second; \
	for (TPlayers::const_iterator _player=_players.begin();_player!=_players.end(); ++_player) \
	GetGameObject()->InvokeRMI(rmi, params, eRMI_ToClientChannel, GetChannelId(*_player)); \
	} \
} \

#define GAMERULES_INVOKE_ON_TEAM_NOLOCAL(team, rmi, params)	\
{ \
	TPlayerTeamIdMap::const_iterator _team=m_playerteams.find(team); \
	if (_team!=m_playerteams.end()) \
	{ \
	const TPlayers &_players=_team->second; \
	for (TPlayers::const_iterator _player=_players.begin();_player!=_players.end(); ++_player) \
	GetGameObject()->InvokeRMI(rmi, params, eRMI_ToClientChannel|eRMI_NoLocalCalls, GetChannelId(*_player)); \
	} \
} \


#define ACTOR_INVOKE_ON_TEAM(team, rmi, params)	\
{ \
	TPlayerTeamIdMap::const_iterator _team=m_playerteams.find(team); \
	if (_team!=playerteams.end()) \
	{ \
	const TPlayers &_players=_team.second; \
	for (TPlayers::const_iterator _player=_players.begin();_player!=_players.end(); ++_player) \
		{ \
		CActor *pActor=GetActorByEntityId(*_player); \
		if (pActor) \
		pActor->GetGameObject()->InvokeRMI(rmi, params, eRMI_ToClientChannel, GetChannelId(*_player)); \
		} \
	} \
} \


#define ACTOR_INVOKE_ON_TEAM_NOLOCAL(team, rmi, params)	\
{ \
	TPlayerTeamIdMap::const_iterator _team=m_playerteams.find(team); \
	if (_team!=playerteams.end()) \
	{ \
	const TPlayers &_players=_team.second; \
	for (TPlayers::const_iterator _player=_players.begin();_player!=_players.end(); ++_player) \
		{ \
		CActor *pActor=GetActorByEntityId(*_player); \
		if (pActor) \
		pActor->GetGameObject()->InvokeRMI(rmi, params, eRMI_ToClientChannel|eRMI_NoLocalCalls, GetChannelId(*_player)); \
		} \
	} \
} \

class IGameRulesClientConnectionListener
{
public:
	virtual ~IGameRulesClientConnectionListener() {}

	virtual void OnClientConnect(int channelId, bool isReset, EntityId playerId) = 0;
	virtual void OnClientDisconnect(int channelId, EntityId playerId) = 0;
	virtual void OnClientEnteredGame(int channelId, bool isReset, EntityId playerId) = 0;
	virtual void OnOwnClientEnteredGame() = 0;
};

class CGameRules 
	:	public CGameObjectExtensionHelper<CGameRules, IGameRules, 64>
	,   public IViewSystemListener
	,	public IHostMigrationEventListener
{
public:

	typedef std::vector<EntityId>								TPlayers;
	typedef std::vector<EntityId>								TEntityIdVec;
	typedef std::set<CryUserID>									TCryUserIdSet;

	struct SGameRulesListener
	{
		virtual ~SGameRulesListener() {}
		virtual void GameOver(int localWinner) = 0;
		virtual void EnteredGame() = 0;
		virtual void EndGameNear(EntityId id) = 0;
		virtual void ClientEnteredGame( EntityId clientId ) {}
		virtual void ClientDisconnect( EntityId clientId ) {}
	};
	typedef std::vector<SGameRulesListener*> TGameRulesListenerVec;

	typedef std::map<IEntity *, float> TExplosionAffectedEntities;

	// This structure contains the necessary information to create a new player
	// actor from a migrating one (a new player actor is created for each
	// reconnecting client and needs to be identical to the original actor on
	// the original server, or at least as close as possible)
	struct SMigratingPlayerInfo 
	{
		CryFixedStringT<HOST_MIGRATION_MAX_PLAYER_NAME_SIZE>	m_originalName;
		Vec3					m_pos;
		Ang3					m_ori;
		EntityId			m_originalEntityId;
		int						m_team;
		float					m_health;
		TNetChannelID	m_channelID;
		bool					m_inUse;

		SMigratingPlayerInfo() : m_inUse(false), m_channelID(0) {}

		void SetChannelID(uint16 id) { assert(id > 0); m_channelID = id; }

		void SetData(const char* inOriginalName, EntityId inOriginalEntityId, int inTeam, const Vec3& inPos, const Ang3& inOri, float inHealth)
		{
			m_originalName = inOriginalName;
			m_originalEntityId = inOriginalEntityId;
			m_team = inTeam;
			m_pos = inPos;
			m_ori = inOri;
			m_health = inHealth;

			m_inUse = true;
		}

		void Reset() { m_inUse = false; m_channelID = 0; }

		bool InUse() { return m_inUse; }
	};

	struct SHostMigrationItemInfo
	{
		SHostMigrationItemInfo()
		{
			Reset();
		}

		void Reset()
		{
			m_inUse = false;
		}

		void Set(EntityId itemId, EntityId ownerId, bool isUsed, bool isSelected)
		{
			m_itemId = itemId;
			m_ownerId = ownerId;
			m_isUsed = isUsed;
			m_isSelected = isSelected;

			m_inUse = true;
		}

		EntityId m_itemId;
		EntityId m_ownerId;
		bool m_isUsed;
		bool m_isSelected;

		bool m_inUse;
	};

	struct SHostMigrationClientRequestParams
	{
		SHostMigrationClientRequestParams()
		{
			m_hasSentLoadout = false;
			m_timeToAutoRevive = 0.f;
		}

		void SerializeWith(TSerialize ser)
		{
			//m_loadoutParams.SerializeWith(ser);

			ser.Value("hasSentLoadout", m_hasSentLoadout, 'bool');
			ser.Value("timeToAutoRevive", m_timeToAutoRevive, 'fsec');
		}

		//CGameRules::EquipmentLoadoutParams m_loadoutParams;
		float m_timeToAutoRevive;
		bool m_hasSentLoadout;
	};

	struct SHostMigrationClientControlledParams
	{
		SHostMigrationClientControlledParams()
		{
			m_pAmmoParams = NULL;
			m_doneEnteredGame = false;
			m_doneSetAmmo = false;
			m_pHolsteredItemClass = NULL;
			m_pSelectedItemClass = NULL;
			m_hasValidVelocity = false;
			m_bInVisorMode = false;
		}

		~SHostMigrationClientControlledParams()
		{
			SAFE_DELETE_ARRAY(m_pAmmoParams);
		}

		bool IsDone()
		{
			return (m_doneEnteredGame && m_doneSetAmmo);
		}

		struct SAmmoParams
		{
			IEntityClass *m_pAmmoClass;
			int m_count;
		};

		Quat m_viewQuat;
		Vec3 m_position;		// Save this since the new server may not have it stored correctly (lag dependent)
		Vec3 m_velocity;
		Vec3 m_aimDirection;

		SAmmoParams *m_pAmmoParams;
		IEntityClass *m_pHolsteredItemClass;
		IEntityClass *m_pSelectedItemClass;

		int m_numAmmoParams;
		int m_numExpectedItems;

		bool m_hasValidVelocity;
		bool m_bInVisorMode;

		bool m_doneEnteredGame;
		bool m_doneSetAmmo;
	};

	struct SMidMigrationJoinParams
	{
		SMidMigrationJoinParams() : m_state(0), m_timeSinceStateChanged(0.f) {}
		SMidMigrationJoinParams(int state, float timeSinceStateChanged) : m_state(state), m_timeSinceStateChanged(timeSinceStateChanged) {}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("state", m_state, 'ui2');
			ser.Value("timeSinceStateChanged", m_timeSinceStateChanged, 'fsec');
		}

		int m_state;
		float m_timeSinceStateChanged;
	};

	CGameRules();
	virtual ~CGameRules();
	//IGameObjectExtension
	virtual bool Init( IGameObject * pGameObject );
	virtual void PostInit( IGameObject * pGameObject );
	virtual void InitClient(int channelId);
	virtual void PostInitClient(int channelId);
	virtual bool ReloadExtension( IGameObject * pGameObject, const SEntitySpawnParams &params ) { return false; }
	virtual void PostReloadExtension( IGameObject * pGameObject, const SEntitySpawnParams &params ) {}
	virtual bool GetEntityPoolSignature( TSerialize signature ) { return false; }
	virtual void Release();
	virtual void FullSerialize( TSerialize ser );
	virtual bool NetSerialize( TSerialize ser, EEntityAspects aspect, uint8 profile, int flags );
	virtual void PostSerialize();
	virtual void SerializeSpawnInfo( TSerialize ser ) {}
	virtual ISerializableInfoPtr GetSpawnInfo() {return 0;}
	virtual void Update( SEntityUpdateContext& ctx, int updateSlot );
	virtual void HandleEvent( const SGameObjectEvent& );
	virtual void ProcessEvent( SEntityEvent& );
	virtual void SetChannelId(uint16 id) {};
	virtual void SetAuthority( bool auth ) {}
	virtual void PostUpdate( float frameTime ) {}
	virtual void PostRemoteSpawn() {};
	virtual void GetMemoryUsage(ICrySizer * s) const;
	//~IGameObjectExtension

	// IViewSystemListener
	virtual bool OnBeginCutScene(IAnimSequence* pSeq, bool bResetFX);
	virtual bool OnEndCutScene(IAnimSequence* pSeq);
	virtual void OnPlayCutSceneSound(IAnimSequence* pSeq, ISound* pSound) {};
	virtual bool OnCameraChange(const SCameraParams& cameraParams){ return true; };
	// ~IViewSystemListener

	// IHostMigrationEventListener
	virtual bool OnInitiate(SHostMigrationInfo& hostMigrationInfo, uint32& state);
	virtual bool OnDisconnectClient(SHostMigrationInfo& hostMigrationInfo, uint32& state) { return true; }
	virtual bool OnDemoteToClient(SHostMigrationInfo& hostMigrationInfo, uint32& state);
	virtual bool OnPromoteToServer(SHostMigrationInfo& hostMigrationInfo, uint32& state);
	virtual bool OnReconnectClient(SHostMigrationInfo& hostMigrationInfo, uint32& state);
	virtual bool OnFinalise(SHostMigrationInfo& hostMigrationInfo, uint32& state);
	virtual bool OnTerminate(SHostMigrationInfo& hostMigrationInfo, uint32& state) { return true; }
	virtual bool OnReset(SHostMigrationInfo& hostMigrationInfo, uint32& state) { return true; }
	// ~IHostMigrationEventListener

	//IGameRules
	virtual bool ShouldKeepClient(int channelId, EDisconnectionCause cause, const char *desc) const;
	virtual void PrecacheLevel();
	virtual void PrecacheLevelResource(const char* resourceName, EGameResourceType resourceType) {};

	virtual XmlNodeRef FindPrecachedXmlFile(const char *sFilename) { return 0; }
	virtual void OnConnect(struct INetChannel *pNetChannel);
	virtual void OnDisconnect(EDisconnectionCause cause, const char *desc); // notification to the client that he has been disconnected

	virtual bool OnClientConnect(int channelId, bool isReset);
	virtual void OnClientDisconnect(int channelId, EDisconnectionCause cause, const char *desc, bool keepClient);
	virtual bool OnClientEnteredGame(int channelId, bool isReset);

	virtual void OnEntitySpawn(IEntity *pEntity);
	virtual void OnEntityRemoved(IEntity *pEntity);
	
	virtual void OnItemDropped(EntityId itemId, EntityId actorId) {}
	virtual void OnItemPickedUp(EntityId itemId, EntityId actorId) {}

	virtual void SendTextMessage(ETextMessageType type, const char *msg, uint32 to=eRMI_ToAllClients, int channelId=-1,
		const char *p0=0, const char *p1=0, const char *p2=0, const char *p3=0);
	virtual void SendChatMessage(EChatMessageType type, EntityId sourceId, EntityId targetId, const char *msg);
	virtual bool CanReceiveChatMessage(EChatMessageType type, EntityId sourceId, EntityId targetId) const;

	virtual void ClientSimpleHit(const SimpleHitInfo &simpleHitInfo) {}
	virtual void ServerSimpleHit(const SimpleHitInfo &simpleHitInfo) {}
	virtual void ClientHit(const HitInfo &hitInfo) {}
	virtual void ServerHit(const HitInfo &hitInfo) {}

	virtual int GetHitTypeId(const char *type) const { return 0; }
	virtual const char *GetHitType(int id) const { return ""; }

	virtual void OnVehicleDestroyed(EntityId id) {}
	virtual void OnVehicleSubmerged(EntityId id, float ratio) {}
	virtual void OnVehicleFlipped(EntityId id) {}

	virtual void AddHitListener(IHitListener* pHitListener) {}
	virtual void RemoveHitListener(IHitListener* pHitListener) {}

	virtual bool IsFrozen(EntityId id) const { return false; }

	virtual void ForbiddenAreaWarning(bool active, int timer, EntityId targetId);

	virtual void ResetGameTime();
	virtual float GetRemainingGameTime() const;
	virtual void SetRemainingGameTime(float seconds);
	virtual void ClearAllMigratingPlayers(void);
	virtual EntityId SetChannelForMigratingPlayer(const char* name, uint16 channelID);
	virtual void StoreMigratingPlayer(IActor* pActor);

	// Summary
	// Determines if a projectile spawned by the client is hitting a friendly AI
	virtual bool IsClientFriendlyProjectile(const EntityId projectileId, const EntityId targetEntityId) ;
	virtual bool IsTimeLimited() const;

	virtual void ResetRoundTime();
	virtual float GetRemainingRoundTime() const;
	virtual bool IsRoundTimeLimited() const;

	virtual void ResetPreRoundTime();
	virtual float GetRemainingPreRoundTime() const;

	virtual void ResetReviveCycleTime();
	virtual float GetRemainingReviveCycleTime() const;

	virtual void ResetGameStartTimer(float time=-1);
	virtual float GetRemainingStartTimer() const;

	virtual bool OnCollision(const SGameCollision& event);
	virtual void OnCollision_NotifyAI( const EventPhys * pEvent ) {}
	virtual void OnEntityReused(IEntity *pEntity, SEntitySpawnParams &params, EntityId prevId) {};
	//~IGameRules

	virtual void RegisterConsoleCommands(IConsole *pConsole);
	virtual void UnregisterConsoleCommands(IConsole *pConsole);
	virtual void RegisterConsoleVars(IConsole *pConsole);

	virtual void OnRevive(IActor *pActor, const Vec3 &pos, const Quat &rot, int teamId);
	virtual void OnReviveInVehicle(IActor *pActor, EntityId vehicleId, int seatId, int teamId);
	virtual void OnKill(IActor *pActor, EntityId shooterId, const char *weaponClassName, int damage, int material, int hit_type);
	virtual void OnTextMessage(ETextMessageType type, const char *msg,
		const char *p0=0, const char *p1=0, const char *p2=0, const char *p3=0);
	virtual void OnChatMessage(EChatMessageType type, EntityId sourceId, EntityId targetId, const char *msg, bool teamChatOnly);
	virtual void OnKillMessage(EntityId targetId, EntityId shooterId, const char *weaponClassName, float damage, int material, int hit_type);

	IActor *GetActorByChannelId(int channelId) const;
	bool IsRealActor(EntityId actorId) const;
	IActor *GetActorByEntityId(EntityId entityId) const;
	ILINE const char *GetActorNameByEntityId(EntityId entityId) const
	{
		IActor *pActor=GetActorByEntityId(entityId);
		if (pActor)
			return pActor->GetEntity()->GetName();
		return 0;
	}
	ILINE const char *GetActorName(IActor *pActor) const { return pActor->GetEntity()->GetName(); };
	int GetChannelId(EntityId entityId) const;
	bool IsDead(EntityId entityId) const;
	void ShowScores(bool show);
	void KnockActorDown( EntityId actorEntityId );

	//------------------------------------------------------------------------
	// player
	virtual void RevivePlayer(IActor *pActor, const Vec3 &pos, const Quat &angles, int teamId=0, bool clearInventory=true);
	virtual void RevivePlayerInVehicle(IActor *pActor, EntityId vehicleId, int seatId, int teamId=0, bool clearInventory=true);
	virtual void RenamePlayer(IActor *pActor, const char *name);
	virtual string VerifyName(const char *name, IEntity *pEntity=0);
	virtual bool IsNameTaken(const char *name, IEntity *pEntity=0);
	virtual void ChangeTeam(IActor *pActor, int teamId);
	virtual void ChangeTeam(IActor *pActor, const char *teamName);
	//tagging time serialization limited to 0-60sec
	virtual int GetPlayerCount(bool inGame=false) const;
	virtual EntityId GetPlayer(int idx);
	virtual void GetPlayers(TPlayers &players);
	virtual bool IsPlayerInGame(EntityId playerId) const;
	virtual bool IsPlayerActivelyPlaying(EntityId playerId) const;	// [playing / dead / waiting to respawn (inc spectating while dead): true] [not yet joined game / selected Spectate: false]
	virtual bool IsChannelInGame(int channelId) const;

	//------------------------------------------------------------------------
	// teams
	virtual int CreateTeam(const char *name);
	virtual void RemoveTeam(int teamId);
	virtual const char *GetTeamName(int teamId) const;
	virtual int GetTeamId(const char *name) const;
	virtual int GetTeamCount() const;
	virtual int GetTeamPlayerCount(int teamId, bool inGame=false) const;
	virtual int GetTeamChannelCount(int teamId, bool inGame=false) const;
	virtual EntityId GetTeamPlayer(int teamId, int idx);

	virtual void GetTeamPlayers(int teamId, TPlayers &players);
	
	virtual void SetTeam(int teamId, EntityId entityId);
	virtual int GetTeam(EntityId entityId) const;
	virtual int GetChannelTeam(int channelId) const;

	//------------------------------------------------------------------------
	// game	
	virtual void Restart();
	virtual void NextLevel();
	virtual void ResetEntities();
	virtual void OnEndGame();
	virtual void EnteredGame();
	virtual void GameOver(int localWinner);
	virtual void EndGameNear(EntityId id);
	void ClientDisconnect_NotifyListeners( EntityId clientId );
	void ClientEnteredGame_NotifyListeners( EntityId clientId );

	virtual void CreateEntityRespawnData(EntityId entityId);
	virtual bool HasEntityRespawnData(EntityId entityId) const;
	virtual void ScheduleEntityRespawn(EntityId entityId, bool unique, float timer);
	virtual void AbortEntityRespawn(EntityId entityId, bool destroyData);

	virtual void ScheduleEntityRemoval(EntityId entityId, float timer, bool visibility);
	virtual void AbortEntityRemoval(EntityId entityId);

	virtual void UpdateEntitySchedules(float frameTime);
	
	virtual void ForceScoreboard(bool force);
	virtual void FreezeInput(bool freeze);

	virtual void ShowStatus();

	//misc 
	// Next time CGameRules::OnCollision is called, it will skip this entity and return false
	// This will prevent squad mates to be hit by the player
	void SetEntityToIgnore(EntityId id) { m_ignoreEntityNextCollision = id;}

	template<typename T>
	void SetSynchedGlobalValue(TSynchedKey key, const T &value)
	{
		assert(gEnv->bServer);
		g_pGame->GetSynchedStorage()->SetGlobalValue(key, value);
	};

	template<typename T>
	bool GetSynchedGlobalValue(TSynchedKey key, T &value)
	{
		if (!g_pGame->GetSynchedStorage())
			return false;
		return g_pGame->GetSynchedStorage()->GetGlobalValue(key, value);
	}

	int GetSynchedGlobalValueType(TSynchedKey key) const
	{
		if (!g_pGame->GetSynchedStorage())
			return eSVT_None;
		return g_pGame->GetSynchedStorage()->GetGlobalValueType(key);
	}

	template<typename T>
	void SetSynchedEntityValue(EntityId id, TSynchedKey key, const T &value)
	{
		assert(gEnv->bServer);
		g_pGame->GetSynchedStorage()->SetEntityValue(id, key, value);
	}
	template<typename T>
	bool GetSynchedEntityValue(EntityId id, TSynchedKey key, T &value)
	{
		return g_pGame->GetSynchedStorage()->GetEntityValue(id, key, value);
	}
	
	int GetSynchedEntityValueType(EntityId id, TSynchedKey key) const
	{
		return g_pGame->GetSynchedStorage()->GetEntityValueType(id, key);
	}

	void ResetSynchedStorage()
	{
		g_pGame->GetSynchedStorage()->Reset();
	}

	void ForceSynchedStorageSynch(int channel);


	void PlayerPosForRespawn(IActor *pPlayer, bool save);
	void SPNotifyPlayerKill(EntityId targetId, EntityId weaponId, bool bHeadShot);

	string GetPlayerName(int channelId, bool bVerifyName = false);

	struct ChatMessageParams
	{
		uint8 type;
		EntityId sourceId;
		EntityId targetId;
		string msg;
		bool onlyTeam;

		ChatMessageParams() {};
		ChatMessageParams(EChatMessageType _type, EntityId src, EntityId trg, const char *_msg, bool _onlyTeam)
		: type(_type),
			sourceId(src),
			targetId(trg),
			msg(_msg),
			onlyTeam(_onlyTeam)
		{
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("type", type, 'ui3');
			ser.Value("source", sourceId, 'eid');
			if (type == eChatToTarget)
				ser.Value("target", targetId, 'eid');
			ser.Value("message", msg);
			ser.Value("onlyTeam", onlyTeam, 'bool');
		}
	};

	struct ForbiddenAreaWarningParams
	{
		int timer;
		bool active;
		ForbiddenAreaWarningParams() {};
		ForbiddenAreaWarningParams(bool act, int time) : active(act), timer(time)
		{}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("active", active, 'bool');
			ser.Value("timer", timer, 'ui5');
		}
	};

	struct BoolParam
	{
		bool success;
		void SerializeWith(TSerialize ser)
		{
			ser.Value("success", success, 'bool');
		}
	};

	struct TextMessageParams
	{
		uint8	type;
		string msg;

		uint8 nparams;
		string params[4];

		TextMessageParams() {};
		TextMessageParams(ETextMessageType _type, const char *_msg)
		: type(_type),
			msg(_msg),
			nparams(0)
		{
		};
		TextMessageParams(ETextMessageType _type, const char *_msg, 
			const char *p0=0, const char *p1=0, const char *p2=0, const char *p3=0)
		: type(_type),
			msg(_msg),
			nparams(0)
		{
			if (!AddParam(p0)) return;
			if (!AddParam(p1)) return;
			if (!AddParam(p2)) return;
			if (!AddParam(p3)) return;
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("type", type, 'ui3');
			ser.Value("message", msg);
			ser.Value("nparams", nparams, 'ui3');

			for (int i=0;i<nparams; ++i)
				ser.Value("param", params[i]);
		}

		bool AddParam(const char *param)
		{
			if (!param || nparams>3)
				return false;
			params[nparams++]=param;
			return true;
		}
	};

	struct SetTeamParams
	{
		int				teamId;
		EntityId	entityId;

		SetTeamParams() {};
		SetTeamParams(EntityId _entityId, int _teamId)
		: entityId(_entityId),
			teamId(_teamId)
		{
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("entityId", entityId, 'eid');
			ser.Value("teamId", teamId, 'team');
		}
	};

	struct ChangeTeamParams
	{
		EntityId	entityId;
		int				teamId;

		ChangeTeamParams() {};
		ChangeTeamParams(EntityId _entityId, int _teamId)
			: entityId(_entityId),
				teamId(_teamId)
		{
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("entityId", entityId, 'eid');
			ser.Value("teamId", teamId, 'team');
		}
	};

	struct RenameEntityParams
	{
		EntityId	entityId;
		string		name;

		RenameEntityParams() {};
		RenameEntityParams(EntityId _entityId, const char *name)
			: entityId(_entityId),
				name(name)
		{
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("entityId", entityId, 'eid');
			ser.Value("name", name);
		}
	};

	struct SetGameTimeParams
	{
		CTimeValue endTime;

		SetGameTimeParams() {};
		SetGameTimeParams(CTimeValue _endTime)
		: endTime(_endTime)
		{
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("endTime", endTime);
		}
	};

	struct EntityParams
	{
		EntityId entityId;
		EntityParams() {};
		EntityParams(EntityId entId)
		: entityId(entId)
		{
		}

		void SerializeWith(TSerialize ser)
		{
			ser.Value("entityId", entityId, 'eid');
		}
	};

	struct NoParams
	{
		NoParams() {};
		void SerializeWith(TSerialize ser) {};
	};

	DECLARE_SERVER_RMI_NOATTACH(SvRequestChatMessage, ChatMessageParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClChatMessage, ChatMessageParams, eNRT_ReliableUnordered);

	DECLARE_SERVER_RMI_NOATTACH(SvRequestRename, RenameEntityParams, eNRT_ReliableOrdered);
	DECLARE_CLIENT_RMI_NOATTACH(ClRenameEntity, RenameEntityParams, eNRT_ReliableOrdered);

	DECLARE_SERVER_RMI_NOATTACH(SvRequestChangeTeam, ChangeTeamParams, eNRT_ReliableOrdered);
	DECLARE_CLIENT_RMI_NOATTACH(ClSetTeam, SetTeamParams, eNRT_ReliableOrdered);
	DECLARE_CLIENT_RMI_NOATTACH(ClTextMessage, TextMessageParams, eNRT_ReliableUnordered);

	DECLARE_CLIENT_RMI_NOATTACH(ClForbiddenAreaWarning, ForbiddenAreaWarningParams, eNRT_ReliableOrdered); // needs to be ordered to respect enter->leave->enter transitions

	DECLARE_CLIENT_RMI_NOATTACH(ClSetGameTime, SetGameTimeParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClSetRoundTime, SetGameTimeParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClSetPreRoundTime, SetGameTimeParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClSetReviveCycleTime, SetGameTimeParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClSetGameStartTimer, SetGameTimeParams, eNRT_ReliableUnordered);

	DECLARE_CLIENT_RMI_NOATTACH(ClEnteredGame, NoParams, eNRT_ReliableUnordered);

	DECLARE_CLIENT_RMI_NOATTACH(ClPlayerJoined, RenameEntityParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClPlayerLeft, RenameEntityParams, eNRT_ReliableUnordered);

	DECLARE_SERVER_RMI_NOATTACH(SvHostMigrationRequestSetup, SHostMigrationClientRequestParams, eNRT_ReliableUnordered);
	DECLARE_CLIENT_RMI_NOATTACH(ClHostMigrationFinished, NoParams, eNRT_ReliableOrdered);
	DECLARE_CLIENT_RMI_NOATTACH(ClMidMigrationJoin, SMidMigrationJoinParams, eNRT_ReliableOrdered);
	DECLARE_CLIENT_RMI_NOATTACH(ClHostMigrationPlayerJoined, EntityParams, eNRT_ReliableOrdered);

	virtual void AddGameRulesListener(SGameRulesListener* pRulesListener);
	virtual void RemoveGameRulesListener(SGameRulesListener* pRulesListener);

	void OnHostMigrationGotLocalPlayer(IActor *pPlayer);
	void OnHostMigrationStateChanged();
	int GetMigratingPlayerIndex(TNetChannelID channelID);
	void FinishMigrationForPlayer(int migratingIndex);
	void FakeDisconnectPlayer(EntityId playerId);

	void HostMigrationFindDynamicEntities(TEntityIdVec &results);
	void HostMigrationRemoveDuplicateDynamicEntities();

	ILINE void	HostMigrationStopAddingPlayers()		{ m_bBlockPlayerAddition = true;	}
	void	HostMigrationResumeAddingPlayers() { m_bBlockPlayerAddition  = false; }

	void OnUserLeftLobby( int channelId );

	typedef std::map<int, EntityId>				TTeamIdEntityIdMap;
	typedef std::map<EntityId, int>				TEntityTeamIdMap;
	typedef std::map<int, TPlayers>				TPlayerTeamIdMap;
	typedef std::map<int, EntityId>				TChannelTeamIdMap;
	typedef std::map<string, int>					TTeamIdMap;

	typedef std::map<int, int>						THitMaterialMap;
	typedef std::map<int, string>					THitTypeMap;

	typedef struct SEntityRespawnData
	{
		SmartScriptTable	properties;
		Vec3							position;
		Quat							rotation;
		Vec3							scale;
		int								flags;
		IEntityClass			*pClass;

		EntityId					m_currentEntityId;
		bool							m_bHasRespawned;

#ifdef _DEBUG
		string						name;
#endif
	}SEntityRespawnData;

	typedef struct SEntityRespawn
	{
		bool							unique;
		float							timer;
	}SEntityRespawn;

	typedef struct SEntityRemovalData
	{
		float							timer;
		float							time;
		bool							visibility;
	}SEntityRemovalData;

	typedef std::map<EntityId, SEntityRespawnData>	TEntityRespawnDataMap;
	typedef std::map<EntityId, SEntityRespawn>			TEntityRespawnMap;
	typedef std::map<EntityId, SEntityRemovalData>	TEntityRemovalMap;

protected:
	static void CmdDebugTeams(IConsoleCmdArgs *pArgs);

	void CreateScriptExplosionInfo(SmartScriptTable &scriptExplosionInfo, const ExplosionInfo &explosionInfo);
	void UpdateAffectedEntitiesSet(TExplosionAffectedEntities &affectedEnts, const pe_explosion *pExplosion);
	void AddOrUpdateAffectedEntity(TExplosionAffectedEntities &affectedEnts, IEntity* pEntity, float affected);
	void CommitAffectedEntitiesSet(SmartScriptTable &scriptExplosionInfo, TExplosionAffectedEntities &affectedEnts);
	void ChatLog(EChatMessageType type, EntityId sourceId, EntityId targetId, const char *msg);

	IGameFramework			*m_pGameFramework;
	IGameplayRecorder		*m_pGameplayRecorder;
	ISystem							*m_pSystem;
	IActorSystem				*m_pActorSystem;
	IEntitySystem				*m_pEntitySystem;
	IScriptSystem				*m_pScriptSystem;
	IMaterialManager		*m_pMaterialManager;

	INetChannel					*m_pClientNetChannel;

	std::vector<int>		m_channelIds;
	
	TTeamIdMap					m_teams;
	TEntityTeamIdMap		m_entityteams;
	TTeamIdEntityIdMap	m_teamdefaultspawns;
	TPlayerTeamIdMap		m_playerteams;
	TChannelTeamIdMap		m_channelteams;
	int									m_teamIdGen;

	TEntityRespawnDataMap	m_respawndata;
	TEntityRespawnMap			m_respawns;
	TEntityRemovalMap			m_removals;

	bool	m_bBlockPlayerAddition;

	CTimeValue					m_endTime;	// time the game will end. 0 for unlimited
	CTimeValue					m_roundEndTime;	// time the round will end. 0 for unlimited
	CTimeValue					m_preRoundEndTime;	// time the pre round will end. 0 for no preround
	CTimeValue					m_reviveCycleEndTime; // time for reinforcements.
	CTimeValue					m_gameStartTime; // time for game start, <= 0 means game started already
	CTimeValue					m_gameStartedTime;	// time the game started at.
	CTimeValue					m_cachedServerTime; // server time as of the last call to CGameRules::Update(...)
	CTimeValue					m_hostMigrationTimeSinceGameStarted;
	float						m_timeLimit;

	TGameRulesListenerVec	m_rulesListeners;
	static int					s_invulnID;
	static int          s_barbWireID;

	EntityId					  m_ignoreEntityNextCollision;

	bool                m_timeOfDayInitialized;

	bool                m_explosionScreenFX;

	IMonoObject *m_pScript;
	typedef std::vector<IGameRulesClientConnectionListener*> TClientConnectionListenersVec;
	TClientConnectionListenersVec m_clientConnectionListeners;

	// Used to store the pertinent details of migrating player entities so they
	// can be reconstructed as close as possible to their state prior to migration
	SMigratingPlayerInfo* m_pMigratingPlayerInfo;
	uint32 m_migratingPlayerMaxCount;

	static const int MAX_PLAYERS = MAX_PLAYER_LIMIT;
	TNetChannelID m_migratedPlayerChannels[MAX_PLAYERS];

	SHostMigrationClientRequestParams* m_pHostMigrationParams;
	SHostMigrationClientControlledParams* m_pHostMigrationClientParams;

	SHostMigrationItemInfo *m_pHostMigrationItemInfo;
	uint32 m_hostMigrationItemMaxCount;

	bool m_hostMigrationClientHasRejoined;

	TEntityIdVec m_hostMigrationCachedEntities;
	TEntityIdVec m_entityEventDoneListeners;

	TCryUserIdSet m_participatingUsers;
};

#endif //__GAMERULES_H__
