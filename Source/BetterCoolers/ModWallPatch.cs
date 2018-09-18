using System.Reflection;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;

namespace BetterCV {
	[StaticConstructorOnStartup]
	public static class ModWallPatch {
		static ModWallPatch() {
			var harmony = HarmonyInstance.Create("BetterCV");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
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
			//if (!__result) return;

			ThingDef thingDef;
			if (constructible is Blueprint) {
				thingDef = constructible.def;
			}
			else if (constructible is Frame) {
				thingDef = constructible.def.entityDefToBuild.blueprintDef;
			}
			else {
				thingDef = constructible.def.blueprintDef;
			}
			ThingDef thingDef2 = thingDef.entityDefToBuild as ThingDef;
			if (thingDef2 != null && t.def.graphicData != null) {

				//modified wall check code to use the same logic as PlaceWorker_OnWall
				if ((t.def.graphicData.linkFlags & (LinkFlags.Wall)) != 0 && thingDef2.building != null && thingDef2.building.canPlaceOverWall) {
					__result = false;
				}
			}
			Log.Message("BlocksConstruction " + __result);
		}
	}

	[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintOver")]
	public static class CanPlaceBlueprintOver {
		[HarmonyPostfix]
		public static void _Postfix(BuildableDef newDef, ThingDef oldDef,ref bool __result) {
			//if (__result) return;

			ThingDef thingDef = newDef as ThingDef;
			BuildableDef buildableDef = GenConstruct.BuiltDefOf(oldDef);
			ThingDef thingDef2 = buildableDef as ThingDef;

			if (thingDef != null) {
				if (thingDef2 != null && thingDef2.graphicData != null && ((thingDef2.graphicData.linkFlags & (LinkFlags.Wall)) != 0 || thingDef2.IsSmoothed) && thingDef.building != null && thingDef.building.canPlaceOverWall) {
					__result = true;
				}
			}
			Log.Message("CanPlaceBlueprintOver " + __result);
		}
	}

	[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
	public static class SpawningWipes {
		[HarmonyPostfix]
		public static void _Postfix(BuildableDef newEntDef, BuildableDef oldEntDef,ref bool __result) {
			//if (__result) return;

			ThingDef thingDef = newEntDef as ThingDef;
			ThingDef thingDef2 = oldEntDef as ThingDef;
			if (thingDef == null || thingDef2 == null) {
				return;
			}
			if (thingDef2.graphicData != null && ((thingDef2.graphicData.linkFlags & (LinkFlags.Wall)) != 0 || thingDef2.IsSmoothed) && thingDef.building != null && thingDef.building.canPlaceOverWall) {
				__result = false;
			}
			Log.Message("SpawningWipes " + __result);
		}
	}
}