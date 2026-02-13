using CalamityMod.Systems;
using Terraria.ModLoader;

namespace InfernumMasterPatch
{
    public class InfernumMasterPatch : Mod 
    {
        public override void PostSetupContent()
        {
            var difficulty = ModContent.GetInstance<MasterPatchDifficulty>();

            if (DifficultyModeSystem.Difficulties != null && !DifficultyModeSystem.Difficulties.Contains(difficulty))
            {
                DifficultyModeSystem.Difficulties.Add(difficulty);
                DifficultyModeSystem.CalculateDifficultyData();
            }
        }
    }
}