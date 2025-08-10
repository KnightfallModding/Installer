namespace MelonLoader.Installer;

internal static class Config
{
    public static Uri MelonWiki { get; private set; } = new("https://melonwiki.xyz");
    public static Uri Discord { get; private set; } = new("https://discord.gg/2Wn3N2P");
    public static Uri Github { get; private set; } = new("https://github.com/LavaGang");
    public static Uri Twitter { get; private set; } = new("https://x.com/lava_gang");
    public static string MelonLoaderReleasesApi { get; private set; } = "https://api.github.com/repos/KnightfallModding/ModLoader/releases";
    public static string MelonLoaderBuildWorkflowApi { get; private set; } = "https://api.github.com/repos/KnightfallModding/ModLoader/actions/workflows/180368520/runs?branch=master&event=push&status=success&per_page=5";
    public static string InstallerLatestReleaseApi { get; private set; } = "https://api.github.com/repos/KnightfallModding/Installer/releases/latest";
    public static string CacheDir { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MelonLoader Installer");
    public static string LocalZipCache { get; private set; } = Path.Combine(CacheDir, "Local Build");
    public static string GameListPath { get; private set; } = Path.Combine(CacheDir, "games.txt");

    public static string ProcessPath { get; private set; } =
#if OSX
        Path.GetDirectoryName(Environment.ProcessPath)!;
#else
        Environment.ProcessPath!;
#endif
    public static string ProcessDirectory { get; private set; } = Path.GetDirectoryName(ProcessPath)!;

    public static string[] LoadGameList()
    {
        try
        {
            return !File.Exists(GameListPath) ? [] : File.ReadAllLines(GameListPath);
        }
        catch
        {
            return [];
        }
    }

    public static void SaveGameList(IEnumerable<string> gamePaths)
    {
        try
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
            File.WriteAllLines(GameListPath, gamePaths);
        }
        catch { }
    }
}
