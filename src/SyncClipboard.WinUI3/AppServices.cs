﻿using Microsoft.Extensions.DependencyInjection;
using SyncClipboard.Abstract;
using SyncClipboard.Core;
using SyncClipboard.Core.Clipboard;
using SyncClipboard.Core.Interfaces;
using SyncClipboard.WinUI3.ClipboardWinUI;
using SyncClipboard.WinUI3.Views;
using SyncClipboard.WinUI3.Win32;
using SyncClipboard.WinUI3.Utilities;

namespace SyncClipboard.WinUI3;

public class AppServices
{
    public static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        AppCore.ConfigCommonService(services);
        AppCore.ConfigurateViewModels(services);
        AppCore.ConfigurateUserService(services);

        services.AddTransient<IAppConfig, AppConfig>();

        services.AddSingleton<IMainWindow, MainWindow>();
        services.AddSingleton<ClipboardListener>();
        services.AddSingleton<IClipboardChangingListener>(sp => sp.GetRequiredService<ClipboardListener>());
        services.AddSingleton<IClipboardMoniter>(sp => sp.GetRequiredService<ClipboardListener>());
        services.AddSingleton<ClipboardFactory>();
        services.AddSingleton<IClipboardFactory>(sp => sp.GetRequiredService<ClipboardFactory>());
        services.AddSingleton<IProfileDtoHelper>(sp => sp.GetRequiredService<ClipboardFactory>());
        services.AddSingleton<TrayIcon>(sp => ((MainWindow)sp.GetRequiredService<IMainWindow>()).TrayIcon);
        services.AddSingleton<ITrayIcon, TrayIconImpl>();
        services.AddSingleton<IContextMenu, TrayIconContextMenu>();
        services.AddSingleton<INativeHotkeyRegistry>(sp => new NativeHotkeyRegistry((MainWindow)sp.GetRequiredService<IMainWindow>()));

        services.AddTransient<IThreadDispatcher>(sp => new ThreadDispatcher(((MainWindow)sp.GetRequiredService<IMainWindow>()).DispatcherQueue));

        services.AddTransient<IClipboardSetter<TextProfile>, TextClipboardSetter>();
        services.AddTransient<IClipboardSetter<FileProfile>, FileClipboardSetter>();
        services.AddTransient<IClipboardSetter<ImageProfile>, ImageClipboardSetter>();
        services.AddTransient<IClipboardSetter<GroupProfile>, FileClipboardSetter>();

        return services;
    }
}
