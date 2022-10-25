using System.Reflection;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace BetterCV {
	[StaticConstructorOnStartup]
	public static class ModWallPatch {
        static ModWallPatch() {
            var harmony = new Harmony("BetterCV");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
		
    }

    public static class BetterCVUtil {
		public static bool isWall(BuildableDef bDef) {
			ThingDef def = bDef as ThingDef;
			if (def == null) return false;

			return (def.graphicData != null && ((def.graphicData.linkFlags & (LinkFlags.Wall)) != 0 || def.IsSmoothed));
        }
		public static bool isOverWall(BuildableDef bDef) {
			ThingDef def = bDef as ThingDef;
			if (def == null) return false;
			if (def.building == null) return false;
			if (bDef.PlaceWorkers == null) return false;
			
			bool isOnWall = false;
			foreach (PlaceWorker p in bDef.PlaceWorkers) {
				Log.Message(p.ToString());
				if (p is PlaceWorker_OnWall) {
					isOnWall = true;
				}
			}
			return isOnWall && def.building.canPlaceOverWall;
		}
    }

	//Patches to:
	//Rimworld.GenConstruct.BlocksConstruction
	//Rimworld.GenConstruct.CanPlaceBlueprintOver
	//Verse.GenSpawn.SpawningWipes
	//in an effort to make modded walls work.

	
	[HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
	public static class BlocksConstruction {
		[HarmonyPostfix]
		public static void _Postfix(Thing constructible, Thing t,ref bool __result) {

			//if our check wouldn't make any difference, don't bother.
			if (!__result) return;

			//convert constructible (incase it's a frame or blueprint, through entityDefToBuild
			BuildableDef cDef = constructible.def.entityDefToBuild;
			if (cDef == null) {
				cDef = constructible.def;
            }
			BuildableDef tDef = t.def.entityDefToBuild;
			if (tDef == null) {
				tDef = t.def;
            }

			//check both ways for over/under
			if ((BetterCVUtil.isOverWall(cDef) && BetterCVUtil.isWall(tDef)) || (BetterCVUtil.isOverWall(tDef) && BetterCVUtil.isWall(cDef))) {
				__result = false;
            }
		}
	}

	[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintOver")]
	public static class CanPlaceBlueprintOver {
		[HarmonyPostfix]
		public static void _Postfix(BuildableDef newDef, ThingDef oldDef,ref bool __result) {
			if (__result) return;

			//check both ways for over/under
			if ((BetterCVUtil.isOverWall(newDef) && BetterCVUtil.isWall(GenConstruct.BuiltDefOf(oldDef))) || (BetterCVUtil.isOverWall(GenConstruct.BuiltDefOf(oldDef)) && BetterCVUtil.isWall(newDef))) {
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
	public static class SpawningWipes {
		[HarmonyPostfix]
		public static void _Postfix(BuildableDef newEntDef, BuildableDef oldEntDef,ref bool __result) {
			if (!__result) return;
			
			BuildableDef thingDef = GenConstruct.BuiltDefOf(newEntDef as ThingDef);
			BuildableDef thingDef2 = GenConstruct.BuiltDefOf(oldEntDef as ThingDef);
			if (thingDef == null || thingDef2 == null) {
				return;
			}
			
			if ((BetterCVUtil.isOverWall(thingDef) && BetterCVUtil.isWall(thingDef2)) || (BetterCVUtil.isOverWall(thingDef2) && BetterCVUtil.isWall(thingDef))) {
				__result = false;
			}
		}
	}
}