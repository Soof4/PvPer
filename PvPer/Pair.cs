using Microsoft.Xna.Framework;
using System.Configuration;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace PvPer
{
    public class Pair
    {
        public int Player1, Player2;
        public static Configuration Config = new Configuration();

        public Pair(int player1, int player2)
        {
            Player1 = player1;
            Player2 = player2;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            Pair other = (Pair)obj;

            return Player1 == other.Player1 && Player2 == other.Player2 || Player1 == other.Player2 && Player2 == other.Player1;
        }

        public override int GetHashCode()
        {
            return Player1 << 16 | Player2;
        }

        public void StartDuel()
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            if (plr1 != null && plr2 != null)
            {
                if (!plr1.Active || !plr2.Active)
                {
                    plr1.SendErrorMessage("Duel has been cancelled because one of the participants is offline.");
                    plr2.SendErrorMessage("Duel has been cancelled because one of the participants is offline.");
                    PvPer.Invitations.Remove(this);
                    return;
                }

                if (plr1.Dead || plr2.Dead)
                {
                    plr1.SendErrorMessage("Duel has been cancelled because one of the participants is dead.");
                    plr2.SendErrorMessage("Duel has been cancelled because one of the participants is dead.");
                    PvPer.Invitations.Remove(this);
                    return;
                }

                if (Utils.IsPlayerInADuel(plr1.Index) || Utils.IsPlayerInADuel(plr2.Index))
                {
                    plr1.SendErrorMessage("Duel has been cancelled because one of the participants is already in another duel.");
                    plr2.SendErrorMessage("Duel has been cancelled because one of the participants is already in another duel.");
                    PvPer.Invitations.Remove(this);
                    return;
                }
            }
            else
            {
                PvPer.Invitations.Remove(this);
                return;
            }

            plr1.SendSuccessMessage($"Duel is starting!");
            plr2.SendSuccessMessage($"Duel is starting!");

            plr1.Teleport(PvPer.Config.Player1PositionX * 16, PvPer.Config.Player1PositionY * 16);
            plr2.Teleport(PvPer.Config.Player2PositionX * 16, PvPer.Config.Player2PositionY * 16);

            plr1.SetBuff(BuffID.Webbed, 60 * 6);
            plr2.SetBuff(BuffID.Webbed, 60 * 6);

            plr1.SetPvP(false);
            plr2.SetPvP(false);
            plr1.SetTeam(0);
            plr2.SetTeam(0);
            plr1.Heal();
            plr2.Heal();
            plr1.SendData(PacketTypes.PlayerDodge, number: plr1.Index, number2: 6);
            plr2.SendData(PacketTypes.PlayerDodge, number: plr1.Index, number2: 6);

            PvPer.Invitations.Remove(this);
            PvPer.AwaitingDuels.Add(this);

            Task.Run(async () =>
            {
                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, Player1, -1,
                    Terraria.Localization.NetworkText.FromLiteral("Duel starting in..."), (int)new Color(0, 255, 0).PackedValue,
                    plr1.X + 16, plr1.Y - 16);

                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, Player2, -1,
                    Terraria.Localization.NetworkText.FromLiteral("Duel starting in..."), (int)new Color(0, 255, 0).PackedValue,
                    plr2.X + 16, plr2.Y - 16);

                for (int i = 5; i > 0; i--)
                {
                    await Task.Delay(1000);
                    NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, Player1, -1,
                        Terraria.Localization.NetworkText.FromLiteral(i.ToString()), (int)new Color(255 - i * 50, i * 50, 0).PackedValue,
                        plr1.X + 16, plr1.Y - 16);

                    NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, Player2, -1,
                        Terraria.Localization.NetworkText.FromLiteral(i.ToString()), (int)new Color(255 - i * 50, i * 50, 0).PackedValue,
                        plr2.X + 16, plr2.Y - 16);
                }

                await Task.Delay(1000);

                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, Player1, -1,
                    Terraria.Localization.NetworkText.FromLiteral("GO!!"), (int)new Color(255, 0, 0).PackedValue,
                    plr1.X + 16, plr1.Y - 16);

                NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, Player2, -1,
                    Terraria.Localization.NetworkText.FromLiteral("GO!!"), (int)new Color(255, 0, 0).PackedValue,
                    plr2.X + 16, plr2.Y - 16);

                PvPer.AwaitingDuels.Remove(this);
                PvPer.ActiveDuels.Add(this);

                plr1.TPlayer.hostile = true;
                plr2.TPlayer.hostile = true;
                NetMessage.SendData((int)PacketTypes.TogglePvp, Player1, -1, Terraria.Localization.NetworkText.Empty, Player1);
                NetMessage.SendData((int)PacketTypes.TogglePvp, Player1, -1, Terraria.Localization.NetworkText.Empty, Player2);
                NetMessage.SendData((int)PacketTypes.TogglePvp, Player2, -1, Terraria.Localization.NetworkText.Empty, Player1);
                NetMessage.SendData((int)PacketTypes.TogglePvp, Player2, -1, Terraria.Localization.NetworkText.Empty, Player2);
            });
        }

        public void EndDuel(int winner)
        {
            int loser = winner == Player1 ? Player2 : Player1;
            string msg = DeathMessages.GetMessage(TShock.Players[winner].Name, TShock.Players[loser].Name);
            TSPlayer.All.SendMessage(msg, 255, 204, 255);

            PvPer.ActiveDuels.Remove(this);
            TShock.Players[winner].SetPvP(false);
            TShock.Players[loser].SetPvP(false);

            Task.Run(async () =>
            {
                // Save winner data and calculate win streak
                SavePlayersData(winner);
                // Reset loser's win streak to 0
                ResetLoserWinStreak(loser);
                // Update winner's win streak
                DPlayer winnerData = PvPer.DbManager.GetDPlayer(TShock.Players[winner].Account.ID);
                winnerData.WinStreak++; // Increment winner's win streak
                PvPer.DbManager.SavePlayer(winnerData); // Save updated winner data

                int winStreak = winnerData.WinStreak; // Use updated winner's win streak directly
                TSPlayer.All.SendMessage($"{TShock.Players[winner].Name} has won {winStreak} consecutive duels!", 255, 255, 90);

                // Launch fireworks
                int p = Projectile.NewProjectile(Projectile.GetNoneSource(), TShock.Players[winner].TPlayer.position.X + 16,
                TShock.Players[winner].TPlayer.position.Y - 64f, 0f, -8f, ProjectileID.RocketFireworkGreen, 0, 0);
                Main.projectile[p].Kill();
                await Task.Delay(5000);
                TShock.Players[winner].Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
            });
        }

        // Reset the loser's win streak to 0
        private void ResetLoserWinStreak(int loser)
        {
            DPlayer playerData = PvPer.DbManager.GetDPlayer(TShock.Players[loser].Account.ID);
            playerData.WinStreak = 0; // The WinStreak property stores the player's win streak
            PvPer.DbManager.SavePlayer(playerData); // Save the updated loser data
        }

        public void SavePlayersData(int winnerIndex)
        {
            DPlayer plr1, plr2;
            try
            {
                plr1 = PvPer.DbManager.GetDPlayer(TShock.Players[Player1].Account.ID);
            }
            catch (NullReferenceException)
            {
                PvPer.DbManager.InsertPlayer(TShock.Players[Player1].Account.ID, 0, 0);
                plr1 = PvPer.DbManager.GetDPlayer(TShock.Players[Player1].Account.ID);
            }
            try
            {
                plr2 = PvPer.DbManager.GetDPlayer(TShock.Players[Player2].Account.ID);
            }
            catch (NullReferenceException)
            {
                PvPer.DbManager.InsertPlayer(TShock.Players[Player2].Account.ID, 0, 0);
                plr2 = PvPer.DbManager.GetDPlayer(TShock.Players[Player2].Account.ID);
            }

            if (winnerIndex == Player1)
            {
                plr1.Kills++;
                plr2.Deaths++;
                PvPer.DbManager.SavePlayer(plr1);
                PvPer.DbManager.SavePlayer(plr2);
            }
            else
            {
                plr2.Kills++;
                plr1.Deaths++;
                PvPer.DbManager.SavePlayer(plr1);
                PvPer.DbManager.SavePlayer(plr2);
            }
        }
    }
}
