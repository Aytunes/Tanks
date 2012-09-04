/*************************************************************************
  Crytek Source File.
  Copyright (C), Crytek Studios, 2001-2005.
  -------------------------------------------------------------------------
  $Id$
  $DateTime$
  Description:  Register the factory templates used to create classes from names
                e.g. REGISTER_FACTORY(pFramework, "Player", CPlayer, false);

                Since overriding this function creates template based linker errors,
                it's been replaced by a standalone function in its own cpp file.

  -------------------------------------------------------------------------
  History:
  - 17:8:2005   Created by Nick Hesketh - Refactor'd from Game.cpp/h

*************************************************************************/

#include "StdAfx.h"
#include "Game.h"
#include "Player.h"
//
#include "Item.h"
#include "Weapon.h"
#include "VehicleWeapon.h"
#include "AmmoPickup.h"
#include "Binocular.h"
#include "C4.h"
#include "C4Detonator.h"
#include "DebugGun.h"
#include "PlayerFeature.h"
#include "ReferenceWeapon.h"
#include "OffHand.h"
#include "Fists.h"
#include "Lam.h"
#include "GunTurret.h"
#include "ThrowableWeapon.h"
#include "RocketLauncher.h"
#include "AIGrenade.h"
#include "Accessory.h"

#include "ScriptControlledPhysics.h"

#include "GameRules.h"

#include <IItemSystem.h>
#include <IVehicleSystem.h>
#include <IGameRulesSystem.h>

#define HIDE_FROM_EDITOR(className)																																				\
  { IEntityClass *pItemClass = gEnv->pEntitySystem->GetClassRegistry()->FindClass(className);\
  pItemClass->SetFlags(pItemClass->GetFlags() | ECLF_INVISIBLE); }																				\

#define REGISTER_GAME_OBJECT(framework, name, script)\
	{\
		IEntityClassRegistry::SEntityClassDesc clsDesc;\
		clsDesc.sName = #name;\
		clsDesc.sScriptFile = script;\
		struct C##name##Creator : public IGameObjectExtensionCreatorBase\
		{\
			C##name *Create()\
			{\
				return new C##name();\
			}\
			void GetGameObjectExtensionRMIData( void ** ppRMI, size_t * nCount )\
			{\
			C##name::GetGameObjectExtensionRMIData( ppRMI, nCount );\
			}\
		};\
		static C##name##Creator _creator;\
		framework->GetIGameObjectSystem()->RegisterExtension(#name, &_creator, &clsDesc);\
	}

#define REGISTER_GAME_OBJECT_EXTENSION(framework, name)\
	{\
		struct C##name##Creator : public IGameObjectExtensionCreatorBase\
		{\
		C##name *Create()\
			{\
			return new C##name();\
			}\
			void GetGameObjectExtensionRMIData( void ** ppRMI, size_t * nCount )\
			{\
			C##name::GetGameObjectExtensionRMIData( ppRMI, nCount );\
			}\
		};\
		static C##name##Creator _creator;\
		framework->GetIGameObjectSystem()->RegisterExtension(#name, &_creator, NULL);\
	}

// Register the factory templates used to create classes from names. Called via CGame::Init()
void InitGameFactory(IGameFramework *pFramework)
{
	assert(pFramework);

	REGISTER_FACTORY(pFramework, "NullAI", CPlayer, true);
	HIDE_FROM_EDITOR("NullAI");

	REGISTER_FACTORY(pFramework, "Player", CPlayer, false);
	REGISTER_FACTORY(pFramework, "Grunt", CPlayer, true);

	REGISTER_FACTORY(pFramework, "Civilian", CPlayer, true);
	HIDE_FROM_EDITOR("Civilian");

	// Items
	REGISTER_FACTORY(pFramework, "Item", CItem, false);
	REGISTER_FACTORY(pFramework, "PlayerFeature", CPlayerFeature, false);
	REGISTER_FACTORY(pFramework, "LAM", CLam, false);
	REGISTER_FACTORY(pFramework, "Accessory", CAccessory, false);

	// Weapons
	REGISTER_FACTORY(pFramework, "Weapon", CWeapon, false);
	REGISTER_FACTORY(pFramework, "VehicleWeapon", CVehicleWeapon, false);
	REGISTER_FACTORY(pFramework, "AmmoPickup", CAmmoPickup, false);
	REGISTER_FACTORY(pFramework, "AVMine", CThrowableWeapon, false);
	REGISTER_FACTORY(pFramework, "Claymore", CThrowableWeapon, false);
	REGISTER_FACTORY(pFramework, "Binocular", CBinocular, false);
	REGISTER_FACTORY(pFramework, "C4", CC4, false);
	REGISTER_FACTORY(pFramework, "C4Detonator", CC4Detonator, false);
	REGISTER_FACTORY(pFramework, "DebugGun", CDebugGun, false);
	REGISTER_FACTORY(pFramework, "ReferenceWeapon", CReferenceWeapon, false);
	REGISTER_FACTORY(pFramework, "OffHand", COffHand, false);
	REGISTER_FACTORY(pFramework, "Fists", CFists, false);
	REGISTER_FACTORY(pFramework, "GunTurret", CGunTurret, false);
	REGISTER_FACTORY(pFramework, "RocketLauncher", CRocketLauncher, false);
	REGISTER_FACTORY(pFramework, "AIGrenade", CAIGrenade, false);
		
	// vehicle objects
	IVehicleSystem* pVehicleSystem = pFramework->GetIVehicleSystem();

#define REGISTER_VEHICLEOBJECT(name, obj) \
	REGISTER_FACTORY((IVehicleSystem*)pVehicleSystem, name, obj, false); \
	obj::m_objectId = pVehicleSystem->AssignVehicleObjectId();

	//GameRules
	REGISTER_FACTORY(pFramework, "GameRules", CGameRules, false);

	REGISTER_GAME_OBJECT_EXTENSION(pFramework, ScriptControlledPhysics);
}
