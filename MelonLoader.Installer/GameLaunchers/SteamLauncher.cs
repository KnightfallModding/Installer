﻿using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace MelonLoader.Installer.GameLaunchers;

#pragma warning disable CA1416

public class SteamLauncher : GameLauncher
{
    private static readonly string? steamPath;

    static SteamLauncher()
    {
#if WINDOWS
        var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
        key ??= Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
        steamPath = (string?)key?.GetValue("InstallPath");
#elif LINUX
        steamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam");
#elif OSX
        steamPath = Path.Combine(Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal)), "Library", "Application Support", "Steam");
        if ((steamPath != null)
            && !Directory.Exists(steamPath))
            steamPath = "/Applications/Steam.app";
#endif
        if ((steamPath != null)
            && !Directory.Exists(steamPath))
            steamPath = null;
    }

    internal SteamLauncher() : base("/Assets/steam.png") { }

    public override void AddGames()
    {
        if (steamPath == null)
            return;

        var libPath = Path.Combine(steamPath, "config", "libraryfolders.vdf");
        if (!File.Exists(libPath))
            return;

        var libraryFolders = VdfConvert.Deserialize(File.ReadAllText(libPath));
        var libDirs = libraryFolders.Value.Select(x => ((VProperty)((VProperty)x).Value.First(y => ((VProperty)y).Key == "path")).Value.ToString()); // Lord forgive me for this one-liner

        foreach (var library in libDirs)
        {
            var steamapps = Path.Combine(library, "steamapps");
            if (!Directory.Exists(steamapps))
                continue;

            var acfs = Directory.EnumerateFiles(steamapps, "*.acf");
            foreach (var acfPath in acfs)
            {
                VToken acf;
                try
                {
                    acf = VdfConvert.Deserialize(File.ReadAllText(acfPath)).Value;
                }
                catch
                {
                    continue;
                }

                var id = ((VProperty?)acf.FirstOrDefault(x => ((VProperty)x).Key == "appid"))?.Value?.ToString();
                var name = ((VProperty?)acf.FirstOrDefault(x => ((VProperty)x).Key == "name"))?.Value?.ToString();
                var dirName = ((VProperty?)acf.FirstOrDefault(x => ((VProperty)x).Key == "installdir"))?.Value?.ToString();

                if (id == null || name == null || dirName == null)
                    continue;

                var appDir = Path.Combine(steamapps, "common", dirName);
                if (!Directory.Exists(appDir))
                    continue;

                var iconPath = Path.Combine(steamPath, "appcache", "librarycache", id);
                iconPath = Directory.Exists(iconPath)
                    ? Directory.EnumerateFiles(iconPath, "*.jpg").FirstOrDefault(x =>
                    {
                        var fileName = Path.GetFileName(x);
                        return !fileName.StartsWith("library") && !fileName.StartsWith("header") &&
                               !fileName.StartsWith("logo");
                    })
                    : null;

                GameManager.TryAddGame(appDir, name, this, iconPath, out _);
            }
        }
    }
}
