using CalamityMod.Systems;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace InfernumMasterPatch
{
    public class InfernumMasterPatch : Mod
    {
        private static Hook _difficultyPreUpdateHook;

        public override void Load()
        {
            InstallHook();
        }

        public override void Unload()
        {
            _difficultyPreUpdateHook?.Dispose();
            _difficultyPreUpdateHook = null;
        }

        public override void PostSetupContent()
        {
            var difficulty = ModContent.GetInstance<MasterPatchDifficulty>();

            if (DifficultyModeSystem.Difficulties != null && !DifficultyModeSystem.Difficulties.Contains(difficulty))
            {
                DifficultyModeSystem.Difficulties.Add(difficulty);
                DifficultyModeSystem.CalculateDifficultyData();
            }
        }

        private void InstallHook()
        {
            if (!ModLoader.TryGetMod("InfernumMode", out Mod inf))
            {
                Logger.Warn("[InfernumMasterPatch] InfernumMode not found — hook not installed.");
                return;
            }

            var diffSystemType = inf.Code.GetType(
                "InfernumMode.Core.GlobalInstances.Systems.DifficultyManagementSystem");
            var preUpdateMethod = diffSystemType?.GetMethod(
                "PreUpdateWorld", BindingFlags.Instance | BindingFlags.Public);

            if (preUpdateMethod == null)
            {
                Logger.Warn("[InfernumMasterPatch] Could not find DifficultyManagementSystem.PreUpdateWorld — hook not installed.");
                return;
            }

            _difficultyPreUpdateHook = new Hook(preUpdateMethod, OnDifficultyPreUpdateWorld);
            Logger.Info("[InfernumMasterPatch] Hook installed on DifficultyManagementSystem.PreUpdateWorld.");
        }

        private delegate void OrigPreUpdateWorld(object self);

        private static void OnDifficultyPreUpdateWorld(OrigPreUpdateWorld orig, object self)
        {
            if (CompatibilitySystem.IsPatchActive)
                CompatibilitySystem.ForceDisableDifficultyModesOff();

            orig(self);

            if (CompatibilitySystem.IsPatchActive)
                CompatibilitySystem.EnsureInfernumActive();
        }
    }
}