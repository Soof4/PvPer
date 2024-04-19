using Newtonsoft.Json;
using System.Text;
using TShockAPI;

namespace PvPer
{
    public class Configuration
    {
        public string README = "/duel set 3 4 Set the coordinates of the player's teleportation higher or lower than the player's coordinates by 3 blocks";
        public string README2 = "Pull Range: Will retract players from their direction of exiting the arena back to a specified " +
            "opposite position relative to the arena center (a positive value indicates a position in the same direction). " +
            "This feature is enabled by default when the option to kill players is disabled.";
        public string PermName = "pvper.duel / pvper.admin";
        public bool PullArena = true;
        public int PullRange = -20;
        public bool PlayerKill = false;
        public int PlayerSlap = 100;
        public int Player1PositionX = 0;
        public int Player1PositionY = 0;
        public int Player2PositionX = 0;
        public int Player2PositionY = 0;
        public int ArenaPosX1 = 0;
        public int ArenaPosY1 = 0;
        public int ArenaPosX2 = 0;
        public int ArenaPosY2 = 0;

        public static readonly string FilePath = Path.Combine(TShock.SavePath, "PvPerConfig.json");

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                var str = JsonConvert.SerializeObject(this, Formatting.Indented);
                sw.Write(str);
            }
        }

        public static Configuration Read(string path)
        {
            if (!File.Exists(path))
            {
                var defaultConfig = new Configuration();
                defaultConfig.Write(path);
                return defaultConfig;
            }
            else
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sr = new StreamReader(fs))
                {
                    var json = sr.ReadToEnd();
                    var cf = JsonConvert.DeserializeObject<Configuration>(json);
                    return cf!;
                }
            }
        }
    }
}
