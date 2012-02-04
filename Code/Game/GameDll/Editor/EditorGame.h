/*************************************************************************
  Crytek Source File.
  Copyright (C), Crytek Studios, 2001-2004.
 -------------------------------------------------------------------------
  $Id$
  $DateTime$
  Description: Implements the Editor->Game communication interface.
  
 -------------------------------------------------------------------------
  History:
  - 30:8:2004   11:17 : Created by M�rcio Martins

*************************************************************************/
#ifndef __EDITORGAME_H__
#define __EDITORGAME_H__

#if _MSC_VER > 1000
# pragma once
#endif


#include <IGameRef.h>
#include <IEditorGame.h>

struct IGameStartup;

class CEditorGame :
	public IEditorGame
{
public:
	CEditorGame();
	virtual ~CEditorGame();

	VIRTUAL bool Init(ISystem *pSystem,IGameToEditorInterface *pGameToEditorInterface);
	VIRTUAL int Update(bool haveFocus, unsigned int updateFlags);
	VIRTUAL void Shutdown();
	VIRTUAL bool SetGameMode(bool bGameMode);
	VIRTUAL IEntity * GetPlayer();
	VIRTUAL void SetPlayerPosAng(Vec3 pos,Vec3 viewDir);
	VIRTUAL void HidePlayer(bool bHide);
	VIRTUAL void OnBeforeLevelLoad();
	VIRTUAL void OnAfterLevelLoad(const char *levelName, const char *levelFolder);
	VIRTUAL void OnCloseLevel() {}
	VIRTUAL bool BuildEntitySerializationList(XmlNodeRef output) { return true; }
	VIRTUAL bool GetAdditionalMinimapData(XmlNodeRef output) { return true; }

	VIRTUAL IFlowSystem * GetIFlowSystem() { return gEnv->pGameFramework ? gEnv->pGameFramework->GetIFlowSystem() : NULL; }
	VIRTUAL IGameTokenSystem* GetIGameTokenSystem() { return gEnv->pGameFramework ? gEnv->pGameFramework->GetIGameTokenSystem() : NULL; }
	VIRTUAL IEquipmentSystemInterface* GetIEquipmentSystemInterface() { return NULL; }

	VIRTUAL bool SupportsMultiplayerGameRules() { return true; }
	VIRTUAL void ToggleMultiplayerGameRules() {}
private:
	void InitUIEnums(IGameToEditorInterface* pGTE);
	void InitGlobalFileEnums(IGameToEditorInterface* pGTE);
	void InitActionEnums(IGameToEditorInterface* pGTE);
	bool ConfigureNetContext( bool on );
	void EnablePlayer(bool bPlayer);

	IGameRef			m_pGame;

	bool          m_bEnabled;
	bool          m_bGameMode;
	bool          m_bPlayer;

	IGameStartup *m_pGameStartup;

	static ICVar  *s_pEditorGameMode;
	static CEditorGame *s_pEditorGame;
};


#endif //__EDITORGAME_H__
