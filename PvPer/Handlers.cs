using System.IO.Streams;
using Terraria;
using TShockAPI;
using TShockAPI.Hooks;
using TerrariaApi.Server;

namespace PvPer
{
    public static class Handlers
    {
        public static void InitializeHandlers(TerrariaPlugin registrator)
        {
            GetDataHandlers.PlayerTeam += OnPlayerChangeTeam;
            GetDataHandlers.TogglePvp += OnPlayerTogglePvP;
            GetDataHandlers.Teleport += OnPlayerTeleport;
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            GetDataHandlers.KillMe += OnKill;
            ServerApi.Hooks.ServerLeave.Register(registrator, OnServerLeave);
            GeneralHooks.ReloadEvent += LoadConfig;
        }

        public static void DisposeHandlers(TerrariaPlugin registrator)
        {
            GetDataHandlers.PlayerTeam -= OnPlayerChangeTeam;
            GetDataHandlers.TogglePvp -= OnPlayerTogglePvP;
            GetDataHandlers.Teleport -= OnPlayerTeleport;
            GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;
            GetDataHandlers.KillMe -= OnKill;
            ServerApi.Hooks.ServerLeave.Deregister(registrator, OnServerLeave);
            GeneralHooks.ReloadEvent -= LoadConfig;
        }

        public static void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            string name = plr.Name;

            if (Utils.IsPlayerInADuel(args.PlayerId) && !Utils.IsPlayerInArena(plr))
            {
                if (PvPer.Config.PlayerKill)
                {
                    plr.KillPlayer();
                    // TSPlayer.All.SendMessage($"{name}[c/E84B54:Escaped] the arena! Judged as [c/13A1D1:cowardice] and punished with [c/F86565:death].", Color.Yellow);
                    return;
                }
                else
                {
                    plr.DamagePlayer(PvPer.Config.PlayerSlap);
                    // plr.SendMessage($"{name}[c/E84B54:Escaped] the arena! Judged as [c/13A1D1:cowardice] and punished with [c/F86565:deduction of {Config.PlayerSlap} blood].", Color.Yellow);
                }
                if (PvPer.Config.PullArena)
                {
                    float playerX = ((Entity)plr.TPlayer).Center.X;
                    float playerY = ((Entity)plr.TPlayer).Center.Y;

                    // Calculate the vector from the player to the center of the arena (dx, dy)
                    float centerX = (PvPer.Config.ArenaPosX1 * 16 + PvPer.Config.ArenaPosX2 * 16) / 2f;
                    float centerY = (PvPer.Config.ArenaPosY1 * 16 + PvPer.Config.ArenaPosY2 * 16) / 2f;
                    float dx = centerX - playerX;
                    float dy = centerY - playerY;

                    // Ensure the pulling radius is within the bounds of the arena
                    float maxR = Math.Max(Math.Abs(PvPer.Config.ArenaPosX1 * 16 - centerX), Math.Abs(PvPer.Config.ArenaPosY1 * 16 - centerY)) / 2f;
                    float pullR = Math.Min(maxR, PvPer.Config.PullRange);

                    // New configuration item: Distance range for pulling towards the center of the arena
                    float pullRangeInBlocks = PvPer.Config.PullRange * 16;

                    // Compute the target coordinates for pulling
                    float targetX = (float)(centerX + dx * pullRangeInBlocks / Math.Sqrt(dx * dx + dy * dy));
                    float targetY = (float)(centerY + dy * pullRangeInBlocks / Math.Sqrt(dx * dx + dy * dy));

                    Utils.PullTP(plr, targetX, targetY, (int)Math.Max(pullR, 0));

                    // TSPlayer.All.SendMessage($"{name}[c/E84B54:Escaped] the arena! Action: [C/4284CD:Pulled back]", Color.Yellow);
                }
            }
        }

        public static void OnKill(object? sender, GetDataHandlers.KillMeEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            Pair? duel = Utils.GetDuel(plr.Index);

            if (duel != null)
            {
                int winnerIndex = duel.Player1 == plr.Index ? duel.Player2 : duel.Player1;
                Utils.ProcessDeath(args);
                duel.EndDuel(winnerIndex);
            }
        }

        public static void OnServerLeave(LeaveEventArgs args)
        {
            Pair? duel = Utils.GetDuel(args.Who);
            if (duel != null)
            {
                int winnerIndex = duel.Player1 == args.Who ? duel.Player2 : duel.Player1;
                duel.EndDuel(winnerIndex);
            }
        }

        public static void OnPlayerTogglePvP(object? sender, GetDataHandlers.TogglePvpEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            Pair? duel = Utils.GetDuel(args.PlayerId);

            if (duel != null)
            {
                args.Handled = true;
                plr.TPlayer.hostile = true;
                plr.SendData(PacketTypes.TogglePvp, number: plr.Index);
            }
        }
        public static void OnPlayerTeleport(object? sender, GetDataHandlers.TeleportEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.ID];
            Pair? duel = Utils.GetDuel(args.ID);

            if (duel != null && !Utils.IsLocationInArena((int)(args.X / 16), (int)(args.Y / 16)))
            {
                args.Player.KillPlayer();
            }
        }

        public static void OnPlayerChangeTeam(object? sender, GetDataHandlers.PlayerTeamEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            Pair? duel = Utils.GetDuel(args.PlayerId);

            if (duel != null)
            {
                args.Handled = true;
                plr.TPlayer.team = 0;
                plr.SendData(PacketTypes.PlayerTeam, number: plr.Index);
            }
        }

        public static void LoadConfig(ReloadEventArgs args = null!)
        {
            string configPath = Configuration.FilePath;

            if (File.Exists(configPath))
            {
                PvPer.Config = Configuration.Read(configPath);
                Console.WriteLine($"[PvPerConfig] Reloading");
            }
            else
            {
                PvPer.Config = new Configuration();
                PvPer.Config.Write(configPath);
            }
        }
    }
}