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

        public void SendErrorMessage(string msg)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.SendErrorMessage(msg);
            plr2.SendErrorMessage(msg);
        }

        public void SendSuccessMessage(string msg)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.SendSuccessMessage(msg);
            plr2.SendSuccessMessage(msg);
        }

        public void SetBuff(int type, int time = 3600, bool bypass = false)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.SetBuff(type, time, bypass);
            plr2.SetBuff(type, time, bypass);
        }

        public void SetPvP(bool mode, bool withMsg = false)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.SetPvP(mode, withMsg);
            plr2.SetPvP(mode, withMsg);
        }

        public void SetTeam(int team)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.SetTeam(team);
            plr2.SetTeam(team);
        }
        public void Heal(int health = 600)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.Heal(health);
            plr2.Heal(health);
        }

        public void SendData(PacketTypes msgType, string text = "", int number = 0, float number2 = 0, float number3 = 0, float number4 = 0, int number5 = 0)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            plr1.SendData(msgType, text, number, number2, number3, number4, number5);
            plr2.SendData(msgType, text, number, number2, number3, number4, number5);
        }

        public void SendFloatingMessage(string msg, Color color)
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            Utils.SendFloatingMessage(
                msg,
                new Vector2(plr1.X + 16, plr1.Y - 16),
                color,
                plr1.Index
            );

            Utils.SendFloatingMessage(
                msg,
                new Vector2(plr2.X + 16, plr2.Y - 16),
                color,
                plr2.Index
            );
        }

        public void TogglePvP(bool isOn)
        {
            Main.player[Player1].hostile = isOn;
            Main.player[Player2].hostile = isOn;

            SendData(PacketTypes.TogglePvp, number: Player1);
            SendData(PacketTypes.TogglePvp, number: Player2);
        }

        public void StartDuel()
        {
            TSPlayer plr1 = TShock.Players[Player1];
            TSPlayer plr2 = TShock.Players[Player2];

            if (plr1 != null && plr2 != null)
            {
                if (!plr1.Active || !plr2.Active)
                {
                    SendErrorMessage("Duel has been cancelled because one of the participants is offline.");
                    PvPer.Invitations.Remove(this);
                    return;
                }

                if (plr1.Dead || plr2.Dead)
                {
                    SendErrorMessage("Duel has been cancelled because one of the participants is dead.");
                    PvPer.Invitations.Remove(this);
                    return;
                }

                if (Utils.IsPlayerInADuel(plr1.Index) || Utils.IsPlayerInADuel(plr2.Index))
                {
                    SendErrorMessage("Duel has been cancelled because one of the participants is already in another duel.");
                    PvPer.Invitations.Remove(this);
                    return;
                }
            }
            else
            {
                PvPer.Invitations.Remove(this);
                return;
            }

            SendSuccessMessage($"Duel is starting!");

            plr1.Teleport(PvPer.Config.Player1PositionX * 16, PvPer.Config.Player1PositionY * 16);
            plr2.Teleport(PvPer.Config.Player2PositionX * 16, PvPer.Config.Player2PositionY * 16);

            SetBuff(BuffID.Webbed, 60 * 6);
            SetPvP(false);
            SetTeam(0);
            Heal();
            SendData(PacketTypes.PlayerDodge, number: plr1.Index, number2: 6);

            PvPer.Invitations.Remove(this);
            PvPer.AwaitingDuels.Add(this);

            Task.Run(async () =>
            {
                SendFloatingMessage("Duel starting in...", new Color(0, 255, 0));

                for (int i = 5; i > 0; i--)
                {
                    SendFloatingMessage(i.ToString(), new Color(255 - i * 50, i * 50, 0));
                    await Task.Delay(1000);
                }

                await Task.Delay(1000);

                SendFloatingMessage("GO!!!", new Color(255, 0, 0));

                PvPer.AwaitingDuels.Remove(this);
                PvPer.ActiveDuels.Add(this);

                TogglePvP(true);
            });
        }

        public void EndDuel(int winner)
        {
            int loser = winner == Player1 ? Player2 : Player1;

            string msg = DeathMessages.GetMessage(TShock.Players[winner].Name, TShock.Players[loser].Name);
            TSPlayer.All.SendMessage(msg, 255, 25, 25);

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
