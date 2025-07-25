﻿using Microsoft.Extensions.DependencyInjection;
using NativeNotification.Interface;
using SyncClipboard.Abstract;
using SyncClipboard.Core.Commons;
using SyncClipboard.Core.Interfaces;
using SyncClipboard.Core.Models;
using SyncClipboard.Core.Models.UserConfigs;
using SyncClipboard.Core.Utilities;
using System.Text.Json;

namespace SyncClipboard.Core.Clipboard;

public abstract class Profile
{
    #region ClipboardProfileDTO Field

    public virtual string FileName { get; set; } = "";
    public virtual string Text { get; set; } = "";

    #endregion

    #region abstract

    public abstract ProfileType Type { get; }
    public abstract string ToolTip();
    public abstract string ShowcaseText();
    public abstract Task UploadProfile(IWebDav webdav, CancellationToken cancelToken);

    protected abstract IClipboardSetter<Profile> ClipboardSetter { get; }
    protected abstract bool Same(Profile rhs);
    protected abstract ClipboardMetaInfomation CreateMetaInformation();

    #endregion

    protected const string RemoteProfilePath = Env.RemoteProfilePath;
    protected static string LocalTemplateFolder => Env.TemplateFileFolder;
    protected static IServiceProvider ServiceProvider { get; } = AppCore.Current.Services;
    protected static IWebDav WebDav => ServiceProvider.GetRequiredService<IWebDav>();
    protected static ILogger Logger => ServiceProvider.GetRequiredService<ILogger>();
    protected static ConfigManager Config => ServiceProvider.GetRequiredService<ConfigManager>();

    private static INotificationManager NotificationManager => ServiceProvider.GetRequiredService<INotificationManager>();
    private static bool EnableNotify => Config.GetConfig<SyncConfig>().NotifyOnDownloaded;

    private ClipboardMetaInfomation? @metaInfomation;
    public ClipboardMetaInfomation MetaInfomation
    {
        get
        {
            @metaInfomation ??= CreateMetaInformation();
            return @metaInfomation;
        }
    }

    public virtual Task BeforeSetLocal(CancellationToken cancelToken,
        IProgress<HttpDownloadProgress>? progress = null)
    {
        return Task.CompletedTask;
    }

    public bool ContentControl { get; set; } = true;
    public virtual bool IsAvailableFromRemote() => true;
    public bool IsAvailableFromLocal() => !ContentControl || IsAvailableAfterFilter();
    public virtual bool IsAvailableAfterFilter() => true;

    public virtual Task EnsureAvailable(CancellationToken token) => Task.CompletedTask;

    protected virtual void SetNotification(INotificationManager notificationManager)
    {
        notificationManager.ShowText(I18n.Strings.ClipboardUpdated, Text);
    }

    public async Task SetLocalClipboard(bool notify, CancellationToken ctk, bool mutex = true)
    {
        if (mutex)
        {
            await LocalClipboard.Semaphore.WaitAsync(ctk);
        }

        try
        {
            var dispather = AppCore.Current.Services.GetService<IThreadDispatcher>();
            if (dispather is null)
            {
                await ClipboardSetter.SetLocalClipboard(MetaInfomation, ctk);
            }
            else
            {
                await dispather.RunOnMainThreadAsync(() => ClipboardSetter.SetLocalClipboard(MetaInfomation, ctk));
            }

            if (notify && EnableNotify)
            {
                Logger.Write("System notification has sent.");
                SetNotification(NotificationManager);
            }
        }
        finally
        {
            if (mutex)
            {
                LocalClipboard.Semaphore.Release();
            }
        }
    }

    public string ToJsonString() => JsonSerializer.Serialize(ToDto());

    public ClipboardProfileDTO ToDto() => new ClipboardProfileDTO(FileName, Text, Type);

    public static bool Same(Profile? lhs, Profile? rhs)
    {
        if (ReferenceEquals(lhs, rhs))
        {
            return true;
        }

        if (lhs is null)
        {
            return rhs is null;
        }

        if (rhs is null)
        {
            return false;
        }

        if (lhs.GetType() != rhs.GetType())
        {
            return false;
        }

        return lhs.Same(rhs);
    }

    public override string ToString()
    {
        string str = "";
        str += "FileName" + FileName;
        str += "Text:" + Text;
        return str;
    }

    protected ActionButton DefaultButton()
    {
        return new ActionButton(I18n.Strings.Copy, () => { _ = SetLocalClipboard(false, CancellationToken.None); });
    }
}
