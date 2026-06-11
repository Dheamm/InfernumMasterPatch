using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernumMasterPatch
{
    public class CompatibilitySystem : ModSystem
    {
        private static PropertyInfo _infernumDisableModes;
        private static PropertyInfo _infernumModeEnabledProp;
        private static FieldInfo _calamityRevengeField;
        private static MethodInfo _sendPacketMethod;
        private static Type _infernumActivityPacketType;

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
            Initialize();
        }

        public override void Unload()
        {
            _infernumDisableModes = null;
            _infernumModeEnabledProp = null;
            _calamityRevengeField = null;
            _sendPacketMethod = null;
            _infernumActivityPacketType = null;
            _initialized = false;
            _patchActive = false;
        }

        public override void PreUpdateWorld()
        {
            if (Main.gameMenu)
            {
                _patchActive = false;
                return;
            }

            if (_patchActive)
                ForceDisableDifficultyModesOff();
        }

        public static void ForceDisableDifficultyModesOff()
        {
            try
            {
                _infernumDisableModes?.SetValue(null, false);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    $"[InfernumMasterPatch] ForceDisableDifficultyModesOff failed: {ex.Message}");
            }
        }

        public static void EnsureInfernumActive()
        {
            try
            {
                if (_infernumModeEnabledProp == null)
                    return;

                bool currentlyActive = (bool)_infernumModeEnabledProp.GetValue(null);
                if (!currentlyActive)
                {
                    ModContent.GetInstance<InfernumMasterPatch>().Logger.Info(
                        "[InfernumMasterPatch] Infernum was turned off after PreUpdateWorld — restoring.");
                    _infernumModeEnabledProp.SetValue(null, true);
                    SendInfernumSyncPacket();
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    $"[InfernumMasterPatch] EnsureInfernumActive failed: {ex.Message}");
            }
        }

        public static bool IsInfernumActive()
        {
            if (!_initialized || _infernumModeEnabledProp == null)
                return false;

            try
            {
                return (bool)_infernumModeEnabledProp.GetValue(null);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    $"[InfernumMasterPatch] IsInfernumActive failed: {ex.Message}");
                return false;
            }
        }

        public static void SetInfernumActive(bool active)
        {
            if (!_initialized || _infernumModeEnabledProp == null)
                return;

            try
            {
                _infernumModeEnabledProp.SetValue(null, active);

                if (active)
                    SendInfernumSyncPacket();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    $"[InfernumMasterPatch] SetInfernumActive({active}) failed: {ex.Message}");
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
            catch (Exception ex)
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    $"[InfernumMasterPatch] SetRevengeActive({active}) failed: {ex.Message}");
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

        private static void SendInfernumSyncPacket()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            try
            {
                if (_sendPacketMethod == null || _infernumActivityPacketType == null)
                    return;

                var genericMethod = _sendPacketMethod.MakeGenericMethod(_infernumActivityPacketType);
                var parameters = genericMethod.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    args[i] = parameters[i].ParameterType == typeof(int) ? (object)-1 : null;

                genericMethod.Invoke(null, args);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    $"[InfernumMasterPatch] SendInfernumSyncPacket failed: {ex.Message}");
            }
        }

        private static void Initialize()
        {
            if (_initialized)
                return;

            if (ModLoader.TryGetMod("InfernumMode", out Mod inf))
            {
                var diffSystemType = inf.Code.GetType(
                    "InfernumMode.Core.GlobalInstances.Systems.DifficultyManagementSystem");
                _infernumDisableModes = diffSystemType?.GetProperty(
                    "DisableDifficultyModes",
                    BindingFlags.Static | BindingFlags.Public);

                var worldSaveType = inf.Code.GetType(
                    "InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem");
                _infernumModeEnabledProp = worldSaveType?.GetProperty(
                    "InfernumModeEnabled",
                    BindingFlags.Static | BindingFlags.Public);

                var packetManagerType = inf.Code.GetType("InfernumMode.Core.Netcode.PacketManager");
                _infernumActivityPacketType = inf.Code.GetType(
                    "InfernumMode.Core.Netcode.Packets.InfernumModeActivityPacket");

                if (packetManagerType != null)
                {
                    foreach (var method in packetManagerType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        if (method.Name == "SendPacket" && method.IsGenericMethod)
                        {
                            _sendPacketMethod = method;
                            break;
                        }
                    }
                }

                if (_infernumDisableModes == null)
                    ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                        "[InfernumMasterPatch] Could not cache DisableDifficultyModes.");
                if (_infernumModeEnabledProp == null)
                    ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                        "[InfernumMasterPatch] Could not cache InfernumModeEnabled.");
            }
            else
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    "[InfernumMasterPatch] InfernumMode not found during Initialize.");
            }

            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                var calamityWorldType = cal.Code.GetType("CalamityMod.World.CalamityWorld");
                _calamityRevengeField = calamityWorldType?.GetField(
                    "revenge", BindingFlags.Static | BindingFlags.Public);

                if (_calamityRevengeField == null)
                    ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                        "[InfernumMasterPatch] Could not cache CalamityWorld.revenge.");
            }
            else
            {
                ModContent.GetInstance<InfernumMasterPatch>().Logger.Warn(
                    "[InfernumMasterPatch] CalamityMod not found during Initialize.");
            }

            _initialized = true;
        }
    }
}