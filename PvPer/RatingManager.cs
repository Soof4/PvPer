using Terraria;
using TShockAPI;

namespace PvPer
{
    public static class RatingManager
    {   
        // Simplified Glicko-2, no sum-operators because this plugin aims to calculate and update players' ratings after reach duel.
        // Check out http://www.glicko.net/glicko/glicko2.pdf, if you want to know more about what is going down below.

        public static double ComputeEstimatedVariance(DPlayer plr, DPlayer enemy)
        {
            return G(enemy.RatingDeviation) * E(plr.Rating, enemy.Rating, enemy.RatingDeviation) * (1 - E(plr.Rating, enemy.Rating, enemy.RatingDeviation));
        }

        public static double ComputeEstimatedImprovement(DPlayer plr, DPlayer enemy, double score)
        {
            return ComputeEstimatedVariance(plr, enemy) * G(enemy.RatingDeviation) * (score - E(plr.Rating, enemy.Rating, enemy.RatingDeviation));
        }

        private static double G(double phi)
        {
            return 1 / (1 + 3 * phi * phi / (Math.PI * Math.PI));
        }

        private static double E(double mu, double enemyMu, double enemyPhi)
        {
            return 1 / (1 + Math.Exp(-G(enemyPhi) * (mu - enemyMu)));
        }
    }

}