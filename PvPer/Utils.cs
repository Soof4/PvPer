using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace PvPer
{
    public class Utils
    {
        public static bool IsPlayerInADuel(int playerIndex)
        {
            foreach (Pair p in PvPer.ActiveDuels)
            {
                if (p.Player1 == playerIndex || p.Player2 == playerIndex)
                {
                    return true;
                }
            }

            foreach (Pair p in PvPer.AwaitingDuels)
            {
                if (p.Player1 == playerIndex || p.Player2 == playerIndex)
                {
                    return true;
                }
            }

            return false;
        }

        public static Pair? GetInvitationFromSenderIndex(int playerIndex)
        {
            foreach (Pair p in PvPer.Invitations)
            {
                if (p.Player1 == playerIndex)
                {
                    return p;
                }
            }

            return null;
        }

        public static Pair? GetInvitationFromReceiverIndex(int playerIndex)
        {
            foreach (Pair p in PvPer.Invitations)
            {
                if (p.Player2 == playerIndex)
                {
                    return p;
                }
            }

            return null;
        }

        public static Pair? GetDuel(int playerIndex)
        {
            foreach (Pair p in PvPer.ActiveDuels)
            {
                if (p.Player1 == playerIndex || p.Player2 == playerIndex)
                {
                    return p;
                }
            }

            return null;
        }

        public static bool IsPlayerInArena(TSPlayer player)
        {
            return player.TileX >= PvPer.Config.ArenaPosX1 &&
                   player.TileY >= PvPer.Config.ArenaPosY1 &&
                   player.TileX + 1 <= PvPer.Config.ArenaPosX2 &&
                   player.TileY + 2 <= PvPer.Config.ArenaPosY2;
        }

        public static bool IsLocationInArena(int x, int y)
        {
            return x >= PvPer.Config.ArenaPosX1 &&
                   y >= PvPer.Config.ArenaPosY1 &&
                   x <= PvPer.Config.ArenaPosX2 &&
                   y <= PvPer.Config.ArenaPosY2;
        }

        public static void ProcessDeath(GetDataHandlers.KillMeEventArgs args)
        {
            args.Player.Dead = true;
            args.Player.RespawnTimer = TShock.Config.Settings.RespawnSeconds;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && (npc.boss || npc.type == 13 || npc.type == 14 || npc.type == 15) &&
                    Math.Abs(args.Player.TPlayer.Center.X - npc.Center.X) + Math.Abs(args.Player.TPlayer.Center.Y - npc.Center.Y) < 4000f)
                {
                    args.Player.RespawnTimer = TShock.Config.Settings.RespawnBossSeconds;
                    break;
                }
            }

            // Handle kicks/bans on mediumcore/hardcore deaths.
            if (args.Player.TPlayer.difficulty == 1 || args.Player.TPlayer.difficulty == 2) // Player is not softcore
            {
                bool mediumcore = args.Player.TPlayer.difficulty == 1;
                bool shouldBan = mediumcore ? TShock.Config.Settings.BanOnMediumcoreDeath : TShock.Config.Settings.BanOnHardcoreDeath;
                bool shouldKick = mediumcore ? TShock.Config.Settings.KickOnMediumcoreDeath : TShock.Config.Settings.KickOnHardcoreDeath;
                string banReason = mediumcore ? TShock.Config.Settings.MediumcoreBanReason : TShock.Config.Settings.HardcoreBanReason;
                string kickReason = mediumcore ? TShock.Config.Settings.MediumcoreKickReason : TShock.Config.Settings.HardcoreKickReason;

                if (shouldBan)
                {
                    if (!args.Player.Ban(banReason, "TShock"))
                    {
                        TShock.Log.ConsoleDebug("GetDataHandlers / HandlePlayerKillMeV2 kicked with difficulty {0} {1}", args.Player.Name, args.Player.TPlayer.difficulty);
                        args.Player.Kick("You died! Normally, you'd be banned.", true, true);
                    }
                }
                else if (shouldKick)
                {
                    TShock.Log.ConsoleDebug("GetDataHandlers / HandlePlayerKillMeV2 kicked with difficulty {0} {1}", args.Player.Name, args.Player.TPlayer.difficulty);
                    args.Player.Kick(kickReason, true, true, null, false);
                }
            }

            if (args.Player.TPlayer.difficulty == 2 && Main.ServerSideCharacter && args.Player.IsLoggedIn)
            {
                if (TShock.CharacterDB.RemovePlayer(args.Player.Account.ID))
                {
                    TShock.Log.ConsoleDebug("GetDataHandlers / HandlePlayerKillMeV2 ssc delete {0} {1}", args.Player.Name, args.Player.TPlayer.difficulty);
                    args.Player.SendErrorMessage("You have fallen in hardcore mode, and your items have been lost forever.");
                    TShock.CharacterDB.SeedInitialData(args.Player.Account);
                }
            }

            args.Handled = true;
        }

        public static void PullTP(TSPlayer plr, float x, float y, int r)
        {
            if (r <= 0)
            {
                plr.Teleport(x, y, 1);
                return;
            }
            float x2 = ((Entity)plr.TPlayer).Center.X;
            float y2 = ((Entity)plr.TPlayer).Center.Y;
            x2 -= x;
            y2 -= y;
            if (x2 != 0f || y2 != 0f)
            {
                double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;
                x2 = (float)((double)r * Math.Cos(num * Math.PI / 180.0));
                y2 = (float)((double)r * Math.Sin(num * Math.PI / 180.0));
                x2 += x;
                y2 += y;
                plr.Teleport(x2, y2, 1);
            }
        }

        public static void SendFloatingMessage(string message, Vector2 position, Color color, int remoteClient = -1, int ignoreClient = -1)
        {
            NetMessage.SendData(
                (int)PacketTypes.CreateCombatTextExtended,
                remoteClient,
                ignoreClient,
                Terraria.Localization.NetworkText.FromLiteral(message),
                (int)color.PackedValue,
                position.X,
                position.Y
            );
        }
    }
}
