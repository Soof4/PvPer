using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace PvPer
{
    public class Pair
    {
        public TSPlayer Player1, Player2;
        public bool PrevPvP1, PrevPvP2;
        public int PrevTeam1, PrevTeam2;
        public static Configuration Config = new Configuration();

        public Pair(TSPlayer player1, TSPlayer player2)
        {
            Player1 = player1;
            Player2 = player2;
            PrevPvP1 = player1.TPlayer.hostile;
            PrevPvP2 = player2.TPlayer.hostile;
            PrevTeam1 = player1.Team;
            PrevTeam2 = player2.Team;
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
            return Player1.Index << 16 | Player2.Index;
        }

        public void SendErrorMessage(string msg)
        {
            Player1.SendErrorMessage(msg);
            Player2.SendErrorMessage(msg);
        }

        public void SendSuccessMessage(string msg)
        {
            Player1.SendSuccessMessage(msg);
            Player2.SendSuccessMessage(msg);
        }

        public void SetBuff(int type, int time = 3600, bool bypass = false)
        {
            Player1.SetBuff(type, time, bypass);
            Player2.SetBuff(type, time, bypass);
        }

        public void SetPvP(bool mode, bool withMsg = false)
        {
            Player1.SetPvP(mode, withMsg);
            Player2.SetPvP(mode, withMsg);
        }

        public void SetTeam(int team)
        {
            Player1.SetTeam(team);
            Player2.SetTeam(team);
        }
        public void Heal(int health = 600)
        {
            Player1.Heal(health);
            Player2.Heal(health);
        }

        public void SendData(PacketTypes msgType, string text = "", int number = 0, float number2 = 0, float number3 = 0, float number4 = 0, int number5 = 0)
        {
            Player1.SendData(msgType, text, number, number2, number3, number4, number5);
            Player2.SendData(msgType, text, number, number2, number3, number4, number5);
        }

        public void SendFloatingMessage(string msg, Color color)
        {
            Utils.SendFloatingMessage(
                msg,
                new Vector2(Player1.X + 16, Player1.Y - 16),
                color,
                Player1.Index
            );

            Utils.SendFloatingMessage(
                msg,
                new Vector2(Player2.X + 16, Player2.Y - 16),
                color,
                Player2.Index
            );
        }

        public void TogglePvP(bool isOn)
        {
            Main.player[Player1.Index].hostile = isOn;
            Main.player[Player2.Index].hostile = isOn;

            SendData(PacketTypes.TogglePvp, number: Player1.Index);
            SendData(PacketTypes.TogglePvp, number: Player2.Index);
        }

        public void StartDuel()
        {
            if (Player1 != null && Player2 != null)
            {
                if (!Player1.Active || !Player2.Active)
                {
                    SendErrorMessage("Duel has been cancelled because one of the participants is offline.");
                    PvPer.Invitations.Remove(this);
                    return;
                }

                if (Player1.Dead || Player2.Dead)
                {
                    SendErrorMessage("Duel has been cancelled because one of the participants is dead.");
                    PvPer.Invitations.Remove(this);
                    return;
                }

                if (Utils.IsPlayerInADuel(Player1.Index) || Utils.IsPlayerInADuel(Player2.Index))
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

            Player1.Teleport(PvPer.Config.Player1PositionX * 16, PvPer.Config.Player1PositionY * 16);
            Player2.Teleport(PvPer.Config.Player2PositionX * 16, PvPer.Config.Player2PositionY * 16);

            SetBuff(BuffID.Webbed, 60 * 6);
            SetPvP(false);
            SetTeam(0);
            Heal();
            SendData(PacketTypes.PlayerDodge, number: Player1.Index, number2: 6);

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

        public void EndDuel(TSPlayer winner)
        {
            TSPlayer loser = winner.Index == Player1.Index ? Player2 : Player1;

            string msg = DeathMessages.GetMessage(winner.Name, loser.Name);
            TSPlayer.All.SendMessage(msg, 255, 25, 25);

            PvPer.ActiveDuels.Remove(this);

            // Rollback to previous team and pvp settings
            Player1.SetPvP(PrevPvP1);
            Player2.SetPvP(PrevPvP2);
            Player1.SetTeam(PrevTeam1);
            Player2.SetTeam(PrevTeam2);

            Task.Run(async () =>
            {
                // Save winner data and calculate win streak
                SavePlayersData(winner);
                // Reset loser's win streak to 0
                ResetLoserWinStreak(loser);
                // Update winner's win streak
                DPlayer winnerData = PvPer.DbManager.GetDPlayer(winner.Account.ID);
                winnerData.WinStreak++; // Increment winner's win streak
                PvPer.DbManager.SavePlayer(winnerData); // Save updated winner data

                int winStreak = winnerData.WinStreak; // Use updated winner's win streak directly
                TSPlayer.All.SendMessage($"{winner.Name} has won {winStreak} consecutive duels!", 255, 255, 90);

                // Launch fireworks
                int p = Projectile.NewProjectile(Projectile.GetNoneSource(), winner.TPlayer.position.X + 16,
                winner.TPlayer.position.Y - 64f, 0f, -8f, ProjectileID.RocketFireworkGreen, 0, 0);
                Main.projectile[p].Kill();
                await Task.Delay(5000);
                winner.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
            });
        }

        // Reset the loser's win streak to 0
        private void ResetLoserWinStreak(TSPlayer loser)
        {
            DPlayer playerData = PvPer.DbManager.GetDPlayer(loser.Account.ID);
            playerData.WinStreak = 0; // The WinStreak property stores the player's win streak
            PvPer.DbManager.SavePlayer(playerData); // Save the updated loser data
        }

        public void SavePlayersData(TSPlayer winner)
        {
            DPlayer plr1, plr2;
            try
            {
                plr1 = PvPer.DbManager.GetDPlayer(Player1.Account.ID);
            }
            catch (NullReferenceException)
            {
                PvPer.DbManager.InsertPlayer(Player1.Account.ID, 0, 0);
                plr1 = PvPer.DbManager.GetDPlayer(Player1.Account.ID);
            }
            try
            {
                plr2 = PvPer.DbManager.GetDPlayer(Player2.Account.ID);
            }
            catch (NullReferenceException)
            {
                PvPer.DbManager.InsertPlayer(Player2.Account.ID, 0, 0);
                plr2 = PvPer.DbManager.GetDPlayer(Player2.Account.ID);
            }

            if (winner.Index == Player1.Index)
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
