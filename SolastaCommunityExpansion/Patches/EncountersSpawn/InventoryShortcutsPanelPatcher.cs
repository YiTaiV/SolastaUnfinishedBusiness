﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.PlayerController
{
    // this patch allows the away party to fully utilize the shortcuts during their turn
    [HarmonyPatch(typeof(InventoryShortcutsPanel), "Refresh")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class InventoryShortcutsPanel_Refresh
    {
        private static bool Flip { get; set; }

        internal static void Prefix(InventoryShortcutsPanel __instance)
        {
            if (Main.Settings.EnableEnemiesControlledByPlayer &&
                __instance.GuiCharacter.RulesetCharacter is RulesetCharacterHero &&
                __instance.GuiCharacter.RulesetCharacterHero.Side == RuleDefinitions.Side.Enemy)
            {
                Flip = true;
                __instance.GuiCharacter.RulesetCharacterHero.ChangeSide(RuleDefinitions.Side.Ally);
            }
        }

        internal static void Postfix(InventoryShortcutsPanel __instance)
        {
            if (Flip)
            {
                Flip = false;
                __instance.GuiCharacter.RulesetCharacterHero.ChangeSide(RuleDefinitions.Side.Enemy);
            }
        }
    }
}
