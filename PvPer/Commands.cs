using Steamworks;
using TShockAPI;

namespace PvPer
{
    public class Commands
    {
        public static void Duel(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("You need to specify a sub-command. (Available sub-commands are: invite, accept, reject, stats, leaderboard)");
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "invite":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("Please specify target player's name.");
                    }
                    else
                    {
                        InviteCmd(args);
                    }
                    return;
                case "accept":
                    AcceptCmd(args);
                    return;
                case "reject":
                    RejectCommand(args);
                    return;
                case "stats":
                    StatsCommand(args);
                    return;
                case "leaderboard":
                    LeaderboardCommand(args);
                    return;
                default:
                    args.Player.SendErrorMessage("[Default] You need to specify a sub-command. (Available sub-commands are: invite, accept, reject, stats, leaderboard)");
                    return;
            }
        }

        private static void InviteCmd(CommandArgs args)
        {
            List<TSPlayer> plrList = TSPlayer.FindByNameOrID(string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1)));


            if (plrList.Count == 0)
            {
                args.Player.SendErrorMessage("Player not found.");
                return;
            }

            if (Utils.IsPlayerInADuel(args.Player.Index))
            {
                args.Player.SendErrorMessage("You're already in a duel right now.");
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

            PvPer.Invitations.Add(new Pair(args.Player.Index, targetPlr.Index));
            args.Player.SendSuccessMessage($"Successfully invited {targetPlr.Name} for a duel.");
            targetPlr.SendMessage($"{args.Player.Name} has sent you a duel invitation. Do [c/CCFFCC:/duel accept] to accept, [c/FFE6CC:/duel reject] to reject.", 255, 204, 255);
        }

        private static void AcceptCmd(CommandArgs args)
        {
            Pair? invitation = Utils.GetInvitationFromReceiverIndex(args.Player.Index);

            if (invitation == null)
            {
                args.Player.SendErrorMessage("There is no active invitation for you.");
                return;
            }

            invitation.StartDuel();
        }

        private static void RejectCommand(CommandArgs args)
        {
            Pair? invitation = Utils.GetInvitationFromReceiverIndex(args.Player.Index);

            if (invitation == null)
            {
                args.Player.SendErrorMessage("There is no active invitation for you.");
                return;
            }

            TShock.Players[invitation.Player1].SendErrorMessage("The other player has rejected your duel invitation.");
            PvPer.Invitations.Remove(invitation);
        }

        private static void StatsCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                try
                {
                    DPlayer plr = PvPer.DbManager.GetDPlayer(args.Player.Account.ID);
                    args.Player.SendInfoMessage("Your stats:\n" +
                                                $"Kills: {plr.Kills}\n" +
                                                $"Deaths: {plr.Deaths}\n" +
                                                $"Kill/Death Ratio: {plr.GetKillDeathRatio()}");
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
                    args.Player.SendInfoMessage($"{matchedAccounts[0].Name} stats:\n" +
                                                $"Kills: {plr.Kills}\n" +
                                                $"Deaths: {plr.Deaths}\n" +
                                                $"Kill/Death Ratio: {plr.GetKillDeathRatio()}");
                }
                catch (NullReferenceException)
                {
                    args.Player.SendErrorMessage("Player not found!");
                }
            }
        }

        private static void LeaderboardCommand(CommandArgs args)
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
    }
}