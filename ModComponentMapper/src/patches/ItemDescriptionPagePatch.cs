﻿using Harmony;
using ModComponentAPI;

namespace ModComponentMapper.patches
{
    [HarmonyPatch(typeof(ItemDescriptionPage), "GetEquipButtonLocalizationId")]//positive caller count
    class ItemDescriptionPageGetEquipButtonLocalizationIdPatch
    {
        public static void Postfix(GearItem gi, ref string __result)
        {
            if (__result != string.Empty)
            {
                return;
            }

            ModComponent modComponent = ModUtils.GetModComponent(gi);
            if (modComponent != null)
            {
                __result = modComponent.InventoryActionLocalizationId;
            }
        }
    }

    [HarmonyPatch(typeof(ItemDescriptionPage), "CanExamine")]//positive caller count
    class ItemDescriptionPageCanExaminePatch
    {
        public static void Postfix(GearItem gi, ref bool __result)
        {
            // guns can always be examined
            __result |= ModUtils.GetComponent<GunItem>(gi) != null;
        }
    }
}
