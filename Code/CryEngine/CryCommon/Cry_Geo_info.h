#include "Cry_Geo.h"

STRUCT_INFO_BEGIN(RectF)
	VAR_INFO(x)
	VAR_INFO(y)
	VAR_INFO(w)
	VAR_INFO(h)
STRUCT_INFO_END(RectF)

STRUCT_INFO_BEGIN(AABB)
	VAR_INFO(min)
	VAR_INFO(max)
STRUCT_INFO_END(AABB)

STRUCT_INFO_BEGIN(OBB)
	VAR_INFO(m33)
	VAR_INFO(h)
	VAR_INFO(c)
STRUCT_INFO_END(OBB)

ENUM_INFO_BEGIN(EGeomType)
	ENUM_ELEM_INFO(, GeomType_None)
	ENUM_ELEM_INFO(, GeomType_BoundingBox)
	ENUM_ELEM_INFO(, GeomType_Physics)
	ENUM_ELEM_INFO(, GeomType_Render)
ENUM_INFO_END(EGeomType)

ENUM_INFO_BEGIN(EGeomForm)
	ENUM_ELEM_INFO(, GeomForm_Vertices)
	ENUM_ELEM_INFO(, GeomForm_Edges)
	ENUM_ELEM_INFO(, GeomForm_Surface)
	ENUM_ELEM_INFO(, GeomForm_Volume)
ENUM_INFO_END(EGeomForm)

