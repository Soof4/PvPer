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
        public override Version Version => new Version(1, 1, 0);

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
            TShockAPI.Commands.ChatCommands.Add(new Command("pvper.use", Commands.Duel, "pvp"));
        }

        private static void LoadConfig(ReloadEventArgs args = null!)
        {
            string configPath = Configuration.FilePath;

            if (File.Exists(configPath))
            {
                Config = Configuration.Read(configPath);
                Console.WriteLine($"[PvPerConfig]Reloading");
            }
            else
            {
                Config = new Configuration();
                Config.Write(configPath);
            }
        }

        #region Hooks
        public static async void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.PlayerId];
            string playerName = plr.Name;

            if (Utils.IsPlayerInADuel(args.PlayerId) && !Utils.IsPlayerInArena(plr))
            {
                if (Config.PlayerKill)
                {
                    plr.KillPlayer();
                    plr.SendMessage($"{playerName}[c/E84B54:Escaped] the arena! Judged as [c/13A1D1:cowardice] and punished with [c/F86565:death]", Color.Yellow);
                    return;
                }
                else
                {
                    plr.DamagePlayer(Config.PlayerSlap);
                    plr.SendMessage($"{playerName}[c/E84B54:Escaped] the arena! Judged as [c/13A1D1:cowardice] and punished with [c/F86565:deduction of {Config.PlayerSlap} blood]", Color.Yellow);
                    return;
                }
            }
        }

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
