using Microsoft.Data.Sqlite;
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
        public override Version Version => new Version(1, 1, 3);
        public override string Author => "Soofa 羽学";
        public override string Description => "PvP with commands.";
        public PvPer(Main game) : base(game) { }
        public static string ConfigPath = Path.Combine(TShock.SavePath + "/PvPerConfig.json");
        public static Configuration Config = new Configuration();

        public static List<Pair> Invitations = new List<Pair>();
        public static List<Pair> AwaitingDuels = new List<Pair>();
        public static List<Pair> ActiveDuels = new List<Pair>();
        public static DbManager DbManager = new DbManager(new SqliteConnection("Data Source=" + Path.Combine(TShock.SavePath, "PvPer.sqlite")));


        public override void Initialize()
        {
            Handlers.LoadConfig();
            Handlers.InitializeHandlers(this);
            TShockAPI.Commands.ChatCommands.Add(new Command("pvper.duel", Commands.Duel, "duel"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Handlers.DisposeHandlers(this);
            }

            base.Dispose(disposing);
        }
    }
}
