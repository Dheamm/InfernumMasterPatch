using CalamityMod.Systems;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernumMasterPatch
{
    public class MasterPatchDifficulty : DifficultyMode
    {
        public override Asset<Texture2D> Texture => ModContent.Request<Texture2D>("InfernumMasterPatch/Assets/Textures/IconUI");
        public override Asset<Texture2D> TextureDisabled => ModContent.Request<Texture2D>("InfernumMasterPatch/Assets/Textures/IconUI_Off");
        public override Asset<Texture2D> OutlineTexture => ModContent.Request<Texture2D>("InfernumMasterPatch/Assets/Textures/IconUI_Outline");

        public override SoundStyle ActivationSound => new SoundStyle("InfernumMasterPatch/Assets/Sounds/ModeToggleLaughDeep");

        public override float DifficultyScale => 0.25f;
        public override int BackBoneGameModeID => GameModeID.Master;
        public override Color ChatTextColor => Color.DarkRed; 

        public override LocalizedText Name => Language.GetText("Mods.InfernumMasterPatch.DifficultyUI.Name");
        public override LocalizedText ShortDescription => Language.GetText("Mods.InfernumMasterPatch.DifficultyUI.Desc");
        public override LocalizedText ExpandedDescription => Language.GetText("Mods.InfernumMasterPatch.DifficultyUI.Expanded");

        public override bool Enabled
        {
            get => CompatibilitySystem.IsActive;
            set
            {
                if (value != CompatibilitySystem.IsActive)
                    CompatibilitySystem.TogglePatch(value);
            }
        }

        public override bool IsBasedOn(DifficultyMode mode)
        {
            if (mode is DeathDifficulty || mode is MasterDifficulty) return true;
            return base.IsBasedOn(mode);
        }

        public override int[] FavoredDifficultyAtTier(int tier)
        {
            var difficultyArray = DifficultyModeSystem.DifficultyTiers[tier];
            List<int> list = new List<int>();

            for (int i = 0; i < difficultyArray.Length; i++)
            {
                if (difficultyArray[i] is DeathDifficulty || difficultyArray[i] is MasterDifficulty)
                    list.Add(i);
            }

            if (list.Count <= 0) list.Add(0);
            return list.ToArray();
        }
    }
}