using Steamworks;
using Terraria;
using TShockAPI;

namespace PvPer
{
    public class DPlayer
    {
        public int AccountID, Kills, Deaths, Rating, RatingDeviation, RatingVolatility;

        public DPlayer(int accountID, int kills, int deaths)
        {
            AccountID = accountID;
            Kills = kills;
            Deaths = deaths;
        }

        public double GetKillDeathRatio()
        {
            return Deaths == 0 ? Kills : (double)Kills / Deaths;
        }
    }
}