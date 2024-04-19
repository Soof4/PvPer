using Microsoft.Data.Sqlite;
using Org.BouncyCastle.Utilities;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Microsoft.Xna.Framework;

namespace PvPer
{
    [ApiVersion(2, 1)]
    public class PvPer : TerrariaPlugin
    {
        public override string Name => "PvPer";
        public override Version Version => new Version(1, 1, 1);
        public override string Author => "Soofa 羽学";
        public override string Description => "PvP with commands.";
        public PvPer(Main game) : base(game) { }
        public static string ConfigPath = Path.Combine(TShock.SavePath + "/PvPerConfig.json");
        public static Configuration Config = new Configuration();

        public static List<Pair> Invitations = new List<Pair>();
        public static List<Pair> ActiveDuels = new List<Pair>();
        public static DbManager DbManager = new DbManager(new SqliteConnection("Data Source=" + Path.Combine(TShock.SavePath, "PvPer.sqlite")));
        public override void Initialize()
        {
            LoadConfig();
            GetDataHandlers.PlayerTeam += OnPlayerChangeTeam;
            GetDataHandlers.TogglePvp += OnPlayerTogglePvP;
            GetDataHandlers.Teleport += OnPlayerTeleport;
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            GetDataHandlers.KillMe += OnKill;
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            GeneralHooks.ReloadEvent += LoadConfig;
            TShockAPI.Commands.ChatCommands.Add(new Command("pvper.duel", Commands.Duel, "duel"));
        }

        private static void LoadConfig(ReloadEventArgs args = null!)
        {
            string configPath = Configuration.FilePath;

            if (File.Exists(configPath))
            {
                Config = Configuration.Read(configPath);
                Console.WriteLine($"[PvPerConfig] Reloading");
            }
            else
            {
                Config = new Configuration();
                Config.Write(configPath);
            }
        }

        #region Hooks
        public static void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            string name = plr.Name;

            if (Utils.IsPlayerInADuel(args.PlayerId) && !Utils.IsPlayerInArena(plr))
            {
                if (Config.PlayerKill)
                {
                    plr.DamagePlayer(int.MaxValue);
                    TSPlayer.All.SendMessage($"{name}[c/E84B54:Escaped] the arena! Judged as [c/13A1D1:cowardice] and punished with [c/F86565:death].", Color.Yellow);
                    return;
                }
                else
                {
                    plr.DamagePlayer(Config.PlayerSlap);
                    plr.SendMessage($"{name}[c/E84B54:Escaped] the arena! Judged as [c/13A1D1:cowardice] and punished with [c/F86565:deduction of {Config.PlayerSlap} blood].", Color.Yellow);
                }
                if (Config.PullArena)
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
                    float pullR = Math.Min(maxR, Config.PullRange);

                    // New configuration item: Distance range for pulling towards the center of the arena
                    float pullRangeInBlocks = Config.PullRange * 16;

                    // Compute the target coordinates for pulling
                    float targetX = (float)(centerX + dx * pullRangeInBlocks / Math.Sqrt(dx * dx + dy * dy));
                    float targetY = (float)(centerY + dy * pullRangeInBlocks / Math.Sqrt(dx * dx + dy * dy));

                    PullTP(plr, targetX, targetY, (int)Math.Max(pullR, 0));

                    TSPlayer.All.SendMessage($"{name}[c/E84B54:Escaped] the arena! Action: [C/4284CD:Pulled back]", Color.Yellow);
                }
            }
        }

        #region Pulling Player Back into Arena Methods

        // Method for pulling a player back
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
        #endregion

        public void OnKill(object? sender, GetDataHandlers.KillMeEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            Pair? duel = Utils.GetDuel(plr.Index);

            if (duel != null)
            {
                int winnerIndex = duel.Player1 == plr.Index ? duel.Player2 : duel.Player1;
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
        #endregion
    }
}
