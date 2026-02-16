using CalamityMod.Systems;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernumMasterPatch
{
    public class MasterPatchDifficulty : DifficultyMode
    {
        private bool _isActive;

        public override Asset<Texture2D> Texture => ModContent.Request<Texture2D>("InfernumMasterPatch/Assets/Textures/IconUI");
        public override Asset<Texture2D> TextureDisabled => ModContent.Request<Texture2D>("InfernumMasterPatch/Assets/Textures/IconUI_Off");
        public override Asset<Texture2D> OutlineTexture => ModContent.Request<Texture2D>("InfernumMasterPatch/Assets/Textures/IconUI_Outline");

        public override SoundStyle ActivationSound => new SoundStyle("InfernumMasterPatch/Assets/Sounds/ModeToggleLaughDeep");

        public override float DifficultyScale => 1.0f;
        public override int BackBoneGameModeID => GameModeID.Master;
        public override Color ChatTextColor => Color.DarkRed;

        public override LocalizedText Name => Language.GetText("Mods.InfernumMasterPatch.DifficultyUI.Name");
        public override LocalizedText ShortDescription => Language.GetText("Mods.InfernumMasterPatch.DifficultyUI.Desc");
        public override LocalizedText ExpandedDescription => Language.GetText("Mods.InfernumMasterPatch.DifficultyUI.Expanded");

        public override FTWDisplayMode GetForTheWorthyDisplay => FTWDisplayMode.Always;

        public override bool Enabled
        {
            get => _isActive;
            set
            {
                if (value == _isActive)
                    return;

                _isActive = value;
                CompatibilitySystem.IsPatchActive = value;

                if (value)
                {
                    CompatibilitySystem.Announce("Mods.InfernumMasterPatch.Messages.Enabled");

                    if (!Main.GameModeInfo.IsJourneyMode)
                        Main.GameMode = GameModeID.Master;
                    else
                        DifficultyModeSystem.AlignJourneyDifficultySlider();

                    CompatibilitySystem.SetRevengeActive(true);
                    CompatibilitySystem.SetInfernumActive(true);
                }
                else
                {
                    CompatibilitySystem.Announce("Mods.InfernumMasterPatch.Messages.Disabled");
                }
            }
        }

        public override bool RequiresDifficulty(DifficultyMode mode)
        {
            if (mode is DeathDifficulty || mode is MasterDifficulty || mode is RevengeanceDifficulty)
                return true;

            if (Main.getGoodWorld)
            {
                if (mode is LegendaryDifficulty || mode is MaliceDifficulty)
                    return true;
            }

            if (ModLoader.TryGetMod("InfernumMode", out _))
            {
                if (mode.GetType().Name.Contains("Infernum") && !mode.GetType().Name.Contains("Master"))
                    return true;
            }

            return false;
        }

        public override bool IsBasedOn(DifficultyMode mode)
        {
            if (mode is DeathDifficulty || mode is MasterDifficulty || mode is RevengeanceDifficulty)
                return true;

            if (Main.getGoodWorld)
            {
                if (mode is LegendaryDifficulty || mode is MaliceDifficulty)
                    return true;
            }

            if (ModLoader.TryGetMod("InfernumMode", out _))
            {
                if (mode.GetType().Name.Contains("Infernum") && !mode.GetType().Name.Contains("Master"))
                    return true;
            }

            return base.IsBasedOn(mode);
        }

        public override int[] FavoredDifficultyAtTier(int tier)
        {
            var difficultyArray = DifficultyModeSystem.DifficultyTiers[tier];
            List<int> list = new List<int>();

            for (int i = 0; i < difficultyArray.Length; i++)
            {
                DifficultyMode diff = difficultyArray[i];

                if (diff is DeathDifficulty || diff is MasterDifficulty || diff is RevengeanceDifficulty)
                    list.Add(i);

                if (Main.getGoodWorld && (diff is LegendaryDifficulty || diff is MaliceDifficulty))
                    list.Add(i);
            }

            if (list.Count <= 0)
                list.Add(0);

            return list.ToArray();
        }
    }
}