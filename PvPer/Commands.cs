using Microsoft.Xna.Framework;
using TShockAPI;

namespace PvPer
{
    public class Commands
    {
        public static void Duel(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                HelpCmd(args);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "invite":
                    InviteCmd(args);
                    return;
                case "accept":
                    AcceptCmd(args);
                    return;
                case "reject":
                case "decline":
                case "deny":
                    RejectCmd(args);
                    return;
                case "stats":
                    StatsCmd(args);
                    return;
                case "l":
                case "leaderboard":
                    LeaderboardCmd(args);
                    return;
                case "s":
                case "set":
                    SetCmd(args);
                    return;
                case "r":
                case "reset":
                    ResetCmd(args);
                    return;
                default:
                    HelpCmd(args);
                    return;
            }
        }


        private static void HelpCmd(CommandArgs args)
        {
            string msg = "List of Sub-commands:\n" +
                "[c/FFFE80:/duel invite <player name>] - [c/7EE874:Invite player to duel]\n" +
                "[c/74D3E8:/duel accept] - [c/7EE874:Accept duel]\n" +
                "[c/74D3E8:/duel reject] - [c/7EE874:Decline duel]\n" +
                "[c/74D3E8:/duel stats] - [c/7EE874:Check record]\n" +
                "[c/74D3E8:/duel leaderboard] - [c/7EE874:Ranking]";

            if (args.Player.HasPermission("pvper.admin"))
            {
                msg += "\n[c/FFFE80:/duel set <1/2/3/4>] - [c/7EE874:1/2 player positions, 3/4 arena boundaries] (Admin only)\n " +
                    "[c/74D3E8:/duel reset] - [c/7EE874:Reset player database. (Admin only)]";
            }

            args.Player.SendMessage(msg, Color.GreenYellow);
        }

        private static void ClearAllData(CommandArgs args)
        {
            // Try to delete all player data from the database
            if (PvPer.DbManager.ClearData())
            {
                args.Player.SendSuccessMessage("All players' dueling data in the database has been successfully cleared.");
                TShock.Log.ConsoleInfo("All players' dueling data in the database has been successfully cleared.");
            }
            else
            {
                args.Player.SendErrorMessage("An error occurred while clearing all players' dueling data.");
                TShock.Log.ConsoleInfo("An error occurred while clearing all players' dueling data.");
            }
        }

        private static void InviteCmd(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Please specify the target player's name.");
                return;
            }

            List<TSPlayer> plrList = TSPlayer.FindByNameOrID(string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1)));

            if (plrList.Count == 0)
            {
                args.Player.SendErrorMessage("The specified player was not found.");
                return;
            }

            if (Utils.IsPlayerInADuel(args.Player.Index))
            {
                args.Player.SendErrorMessage("You are already in a duel.");
                return;
            }

            TSPlayer targetPlr = plrList[0];

            if (targetPlr.Index == args.Player.Index)
            {
                args.Player.SendErrorMessage("You cannot duel yourself!");
                return;
            }

            if (Utils.IsPlayerInADuel(targetPlr.Index))
            {
                args.Player.SendErrorMessage($"{targetPlr.Name} is currently in a duel.");
                return;
            }

            PvPer.Invitations.Add(new Pair(args.Player, targetPlr));
            args.Player.SendSuccessMessage($"Successfully invited {targetPlr.Name} to a duel.");
            targetPlr.SendMessage($"{args.Player.Name} [c/FE7F81:has sent you a duel invitation] \nEnter [c/CCFFCC:/duel accept] to accept or [c/FFE6CC:/duel reject] to reject.", 255, 204, 255);
        }

        private static void AcceptCmd(CommandArgs args)
        {
            Pair? invitation = Utils.GetInvitationFromReceiverIndex(args.Player.Index);

            if (invitation == null)
            {
                args.Player.SendErrorMessage("You have no active invitations.");
                return;
            }

            invitation.StartDuel();
        }

        private static void RejectCmd(CommandArgs args)
        {
            Pair? invitation = Utils.GetInvitationFromReceiverIndex(args.Player.Index);

            if (invitation == null)
            {
                args.Player.SendErrorMessage("You have no active invitations.");
                return;
            }

            invitation.Player1.SendErrorMessage("The other player has declined your duel invitation");
            PvPer.Invitations.Remove(invitation);
        }

        private static void StatsCmd(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                try
                {
                    DPlayer plr = PvPer.DbManager.GetDPlayer(args.Player.Account.ID);
                    args.Player.SendInfoMessage("[c/FFCB80:Your record:]\n" +
                                                $"[c/63DC5A:Kills:] {plr.Kills}\n" +
                                                $"[c/F56469:Deaths:] {plr.Deaths}\n" +
                                                $"[c/F56469:Win Streak:] {plr.WinStreak}\n" +
                                                $"[c/5993DB:Kill-Death Ratio:] {plr.GetKillDeathRatio()}");
                }
                catch (NullReferenceException)
                {
                    args.Player.SendErrorMessage("Player not found!");
                }
            }
            else
            {
                try
                {
                    string name = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                    List<TShockAPI.DB.UserAccount> matchedAccounts = TShock.UserAccounts.GetUserAccountsByName(name);

                    if (matchedAccounts.Count == 0)
                    {
                        args.Player.SendErrorMessage("Player not found!");
                        return;
                    }

                    DPlayer plr = PvPer.DbManager.GetDPlayer(matchedAccounts[0].ID);
                    args.Player.SendInfoMessage($"[c/FFCB80:{matchedAccounts[0].Name} record:]\n" +
                                                $"[c/63DC5A:Kills:] {plr.Kills}\n" +
                                                $"[c/F56469:Deaths:] {plr.Deaths}\n" +
                                                $"[c/F56469:Win Streak:] {plr.WinStreak}\n" +
                                                $"[c/5993DB:Kill/Death Ratio: ]{plr.GetKillDeathRatio()}");
                }
                catch (NullReferenceException)
                {
                    args.Player.SendErrorMessage("Player not found!");
                }
            }
        }

        private static void LeaderboardCmd(CommandArgs args)
        {
            Task.Run(() =>
            {
                string message = "";
                List<DPlayer> list = PvPer.DbManager.GetAllDPlayers();

                list.Sort((p1, p2) =>
                {
                    if (p1.GetKillDeathRatio() >= p2.GetKillDeathRatio())
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });

                DPlayer p;

                if (list.TryGetValue(0, out p))
                {
                    message += $"{1}. {TShock.UserAccounts.GetUserAccountByID(p.AccountID).Name} : {p.GetKillDeathRatio():F2}";
                }

                for (int i = 1; i < 5; i++)
                {
                    if (list.TryGetValue(i, out p))
                    {
                        message += $"\n{i + 1}. {TShock.UserAccounts.GetUserAccountByID(p.AccountID).Name} : {p.GetKillDeathRatio():F2}";
                    }
                }
                args.Player.SendInfoMessage(message);
            });
        }

        private static void SetCmd(CommandArgs args)
        {
            if (!args.Player.HasPermission("pvper.admin"))
            {
                args.Player.SendErrorMessage("You do not have permission to use this command!");
                return;
            }

            int result;
            if (args.Parameters.Count == 2 && int.TryParse(args.Parameters[1], out result) && result >= 1 && result <= 4)
            {
                int x = args.Player.TileX;
                int y = args.Player.TileY;

                switch (result)
                {
                    case 1:
                        PvPer.Config.Player1PositionX = x;
                        PvPer.Config.Player1PositionY = y;
                        args.Player.SendMessage($"Your current position has been set as the [c/F75454:Inviter]'s teleport point, with coordinates ({x}, {y})", Color.CadetBlue);
                        Console.WriteLine($"[Duel System] Inviter's teleport point set, coordinates ({x}, {y})", Color.BurlyWood);
                        break;
                    case 2:
                        PvPer.Config.Player2PositionX = x;
                        PvPer.Config.Player2PositionY = y;
                        args.Player.SendMessage($"Your current position has been set as the [c/49B3D6:Invitee]'s teleport point, with coordinates ({x}, {y})", Color.CadetBlue);
                        Console.WriteLine($"[Duel System] Invitee's teleport point set, coordinates ({x}, {y})", Color.BurlyWood);
                        break;

                    case 3:
                        PvPer.Config.ArenaPosX1 = x;
                        PvPer.Config.ArenaPosY1 = y;
                        args.Player.SendMessage($"Your current position has been set as the [c/9487D6:Arena]'s top-left corner, with coordinates ({x}, {y})", Color.Yellow);
                        Console.WriteLine($"[Duel System] Arena's top-left corner set, coordinates ({x}, {y})", Color.Yellow);
                        break;
                    case 4:
                        PvPer.Config.ArenaPosX2 = x;
                        PvPer.Config.ArenaPosY2 = y;
                        args.Player.SendMessage($"Your current position has been set as the [c/9487D6:Arena]'s bottom-right corner, with coordinates ({x}, {y})", Color.Yellow);
                        Console.WriteLine($"[Duel System] Arena's bottom-right corner set, coordinates ({x}, {y})", Color.Yellow);
                        break;

                    default:
                        args.Player.SendErrorMessage("[i:4080]Command error! [c/CCEB60:Correct command: /pvp set[1/2/3/4]]");
                        return;
                }

                PvPer.Config.Write(Configuration.FilePath);
            }
            else
            {
                args.Player.SendErrorMessage("[i:4080]commandsystem! \nCorrect command: /pvp set [1/2/3/4] - [c/7EE874:1/2 player positions, 3/4 arena boundaries]");
            }
        }

        private static void ResetCmd(CommandArgs args)
        {
            if (!args.Player.HasPermission("pvper.admin"))
            {
                args.Player.SendErrorMessage("You do not have permisssion to use this command!");
                return;
            }

            ClearAllData(args);
        }
    }
}
