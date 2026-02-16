using System.Reflection;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace InfernumMasterPatch
{
    public class CompatibilitySystem : ModSystem
    {
        private static PropertyInfo _infernumDisableModes;
        private static FieldInfo _infernumActiveField;
        private static FieldInfo _calamityRevengeField;
        private static MethodInfo _sendPacketMethod;
        private static System.Type _infernumActivityPacketType;
        private static bool _initialized;
        private static bool _patchActive;

        public static bool IsPatchActive
        {
            get => _patchActive;
            set => _patchActive = value;
        }

        public override void Load()
        {
            _initialized = false;
            _patchActive = false;
        }

        public override void OnModLoad()
        {
            Initialize();
        }

        public override void PreUpdateWorld()
        {
            if (Main.gameMenu)
            {
                _patchActive = false;
                return;
            }

            if (!_initialized)
                Initialize();

            if (_patchActive && _infernumDisableModes != null)
            {
                _infernumDisableModes.SetValue(null, false);
            }
        }

        public static bool IsInfernumActive()
        {
            if (!_initialized || _infernumActiveField == null)
                return false;

            try
            {
                return (bool)_infernumActiveField.GetValue(null);
            }
            catch
            {
                return false;
            }
        }

        public static void SetInfernumActive(bool active)
        {
            if (!_initialized || _infernumActiveField == null)
                return;

            try
            {
                _infernumActiveField.SetValue(null, active);
                
                if (active)
                    SendInfernumSyncPacket();
            }
            catch
            {
            }
        }

        private static void SendInfernumSyncPacket()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            try
            {
                if (_sendPacketMethod != null && _infernumActivityPacketType != null)
                {
                    var genericMethod = _sendPacketMethod.MakeGenericMethod(_infernumActivityPacketType);
                    genericMethod.Invoke(null, new object[] { null, -1, -1 });
                }
            }
            catch
            {
            }
        }

        public static void SetRevengeActive(bool active)
        {
            if (!_initialized || _calamityRevengeField == null)
                return;

            try
            {
                _calamityRevengeField.SetValue(null, active);
            }
            catch
            {
            }
        }

        public static void Announce(string key)
        {
            string text = Language.GetTextValue(key);
            if (text == key)
                text = key.Contains("Enabled") ? "Infernum Master Patch: ENABLED" : "Infernum Master Patch: DISABLED";

            Color color = Color.White;

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color);
            else if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(text, color);
        }

        private static void Initialize()
        {
            if (_initialized)
                return;

            if (ModLoader.TryGetMod("InfernumMode", out Mod inf))
            {
                _infernumDisableModes = inf.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.DifficultyManagementSystem")?.GetProperty("DisableDifficultyModes");
                _infernumActiveField = inf.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem")?.GetField("infernumModeEnabled", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                
                var packetManagerType = inf.Code.GetType("InfernumMode.Core.Netcode.PacketManager");
                _infernumActivityPacketType = inf.Code.GetType("InfernumMode.Core.Netcode.Packets.InfernumModeActivityPacket");
                
                if (packetManagerType != null)
                {
                    var methods = packetManagerType.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    foreach (var method in methods)
                    {
                        if (method.Name == "SendPacket" && method.IsGenericMethod)
                        {
                            _sendPacketMethod = method;
                            break;
                        }
                    }
                }
            }

            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                var calamityWorldType = cal.Code.GetType("CalamityMod.World.CalamityWorld");
                _calamityRevengeField = calamityWorldType?.GetField("revenge", BindingFlags.Static | BindingFlags.Public);
            }

            _initialized = true;
        }
    }
}