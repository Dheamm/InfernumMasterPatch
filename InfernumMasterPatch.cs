using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace InfernumMasterPatch
{
    public class InfernumMasterPatch : Mod { }

    public class CompatibilitySystem : ModSystem
    {
        private static PropertyInfo _disableModes;
        private static FieldInfo _infActive, _deathActive;
        private static bool _initialized, _wasEnabled, _firstCheckDone;

        public override void OnWorldLoad() => _firstCheckDone = false;

        public override void PostUpdateWorld()
        {
            if (Main.gameMenu) { _wasEnabled = false; return; }
            if (!_initialized) Initialize();
            bool isMaster = Main.masterMode || Main.getGoodWorld || Main.zenithWorld;
            bool isDeath = _deathActive != null && (bool)_deathActive.GetValue(null);
            bool isCompatible = isMaster && isDeath;

            _disableModes?.SetValue(null, false);

            if (isCompatible)
            {
                _infActive?.SetValue(null, true);

                if (!_wasEnabled)
                {
                    Main.NewText("InfernumMasterPatch: Enabled.", Color.LimeGreen);
                    _wasEnabled = true;
                    _firstCheckDone = true;
                }
            }
            else
            {
                if (_wasEnabled || !_firstCheckDone)
                {
                    Main.NewText("InfernumMasterPatch: Enable Death Mode to activate Infernum.", Color.Orange);
                    if (_wasEnabled) Main.NewText("InfernumMasterPatch: Disabled.", Color.IndianRed);
                    
                    _wasEnabled = false;
                    _firstCheckDone = true;
                }
            }
        }

        private void Initialize()
        {
            if (ModLoader.TryGetMod("InfernumMode", out Mod inf))
            {
                _disableModes = inf.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.DifficultyManagementSystem")?.GetProperty("DisableDifficultyModes");
                _infActive = inf.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem")?.GetField("infernumModeEnabled", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
            _deathActive = ModLoader.GetMod("CalamityMod")?.Code.GetType("CalamityMod.World.CalamityWorld")?.GetField("death");
            _initialized = true;
        }
    }
}