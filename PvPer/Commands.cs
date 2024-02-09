using TShockAPI;

namespace PvPer
{
    public class Commands
    {
        public static void Duel(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("You need to specify a sub-command. (Available sub-commands are: invite, accept, reject)");
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
                default:
                    args.Player.SendErrorMessage("[Default] You need to specify a sub-command. (Available sub-commands are: invite, accept, reject)");
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
            if (Utils.IsPlayerInADuel(targetPlr.Index))
            {
                args.Player.SendErrorMessage($"{targetPlr.Name} is currently in a duel.");
            }

            PvPer.Invitations.Add(new Pair(args.Player.Index, targetPlr.Index));
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
    }
}