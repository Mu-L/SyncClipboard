using SyncClipboard.Abstract.Notification;
using SyncClipboard.Core.Interfaces;
using SyncClipboard.Core.Models.UserConfigs;
using System.Diagnostics;
#if LINUX
using SyncClipboard.Core.Utilities;
#endif

namespace SyncClipboard.Core.Commons;

public class ConfigManager : ConfigBase
{
    public ConfigManager(StaticConfig staticConfig, INotification notification)
    {
        Notification = notification;
        bool portableUserConfig = staticConfig.GetConfig<EnvConfig>().PortableUserConfig;
        SetPath(portableUserConfig);
        Load();
        staticConfig.ListenConfig<EnvConfig>(EnvConfigChanged);
    }

    private void EnvConfigChanged(EnvConfig envConfig)
    {
        SetPath(envConfig.PortableUserConfig);
        Save();
    }

    private void SetPath(bool portableUserConfig)
    {
        Path = portableUserConfig ? Env.PortableUserConfigFile : Env.UserConfigFile;
    }

    public MenuItem[] Menu => new[]
    {
#if MACOS
        new MenuItem(I18n.Strings.OpenConfigFile, () => Process.Start("open", $"-a TextEdit \"{Path}\"")),
        new MenuItem(I18n.Strings.ReloadConfigFile, Load),
        new MenuItem(I18n.Strings.OpenConfigFileFolder, () => Process.Start("open", $"\"{Env.AppDataDirectory}\""))
#endif
#if WINDOWS
        new MenuItem(I18n.Strings.OpenConfigFile, () => Process.Start("notepad", $"\"{Path}\"")),
        new MenuItem(I18n.Strings.ReloadConfigFile, Load),
        new MenuItem(I18n.Strings.OpenConfigFileFolder, () => Process.Start("explorer", $"\"{Env.AppDataDirectory}\"")),
#endif
#if LINUX
        new MenuItem(I18n.Strings.OpenConfigFile, () => Sys.OpenWithDefaultApp(Path)),
        new MenuItem(I18n.Strings.ReloadConfigFile, Load),
        new MenuItem(I18n.Strings.OpenDataFolderInNautilus, new Action(() => Process.Start("nautilus", $"\"{Env.AppDataDirectory}\"")).NoExcept())
#endif
    };
}