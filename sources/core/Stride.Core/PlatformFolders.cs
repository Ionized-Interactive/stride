// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Reflection;

namespace Stride.Core;

/// <summary>
/// Folders used for the running platform.
/// </summary>
public static class PlatformFolders
{
    // TODO: This class should not try to initialize directories...etc. Try to find another way to do this

    /// <summary>
    /// The system temporary directory.
    /// </summary>
    public static readonly string TemporaryDirectory = GetTemporaryDirectory();

    /// <summary>
    /// The Application temporary directory.
    /// </summary>
    public static readonly string ApplicationTemporaryDirectory = GetApplicationTemporaryDirectory();

    /// <summary>
    /// The application local directory, where user can write local data (included in backup).
    /// </summary>
    public static readonly string ApplicationLocalDirectory = GetApplicationLocalDirectory();

    /// <summary>
    /// The application roaming directory, where user can write roaming data (included in backup).
    /// </summary>
    public static readonly string ApplicationRoamingDirectory = GetApplicationRoamingDirectory();

    /// <summary>
    /// The application cache directory, where user can write data that won't be backup.
    /// </summary>
    public static readonly string ApplicationCacheDirectory = GetApplicationCacheDirectory();

    /// <summary>
    /// The application data directory, where data is deployed.
    /// It could be read-only on some platforms.
    /// </summary>
    public static readonly string ApplicationDataDirectory = GetApplicationDataDirectory();

    /// <summary>
    /// The (optional) application data subdirectory. If not null or empty, /data will be mounted on <see cref="ApplicationDataDirectory"/>/<see cref="ApplicationDataSubDirectory"/>
    /// </summary>
    /// <remarks>This property should not be written after the VirtualFileSystem static initialization. If so, an InvalidOperationExeception will be thrown.</remarks>
    public static string ApplicationDataSubDirectory
    {
        get { return applicationDataSubDirectory; }

        set
        {
            if (IsVirtualFileSystemInitialized)
                throw new InvalidOperationException("ApplicationDataSubDirectory cannot be modified after the VirtualFileSystem has been initialized.");

            applicationDataSubDirectory = value;
        }
    }

    /// <summary>
    /// The application directory, where assemblies are deployed.
    /// It could be read-only on some platforms.
    /// </summary>
    public static readonly string ApplicationBinaryDirectory = GetApplicationBinaryDirectory();

    /// <summary>
    /// Get the path to the application executable.
    /// </summary>
    /// <remarks>Might be null if start executable is unknown.</remarks>
    public static readonly string? ApplicationExecutablePath = GetApplicationExecutablePath();

    private static string applicationDataSubDirectory = string.Empty;

    public static bool IsVirtualFileSystemInitialized { get; internal set; }

    private static string GetApplicationLocalDirectory()
    {
#if STRIDE_PLATFORM_ANDROID
        var directory = Path.Combine(PlatformAndroid.Context.FilesDir.AbsolutePath, "local");
        Directory.CreateDirectory(directory);
        return directory;
#elif STRIDE_PLATFORM_UWP
        return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif STRIDE_PLATFORM_IOS
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", "Local");
        Directory.CreateDirectory(directory);
        return directory;
#else
        // TODO: Should we add "local" ?
        var directory = Path.Combine(GetApplicationBinaryDirectory(), "local");
        Directory.CreateDirectory(directory);
        return directory;
#endif
    }

    private static string GetApplicationRoamingDirectory()
    {
#if STRIDE_PLATFORM_ANDROID
        var directory = Path.Combine(PlatformAndroid.Context.FilesDir.AbsolutePath, "roaming");
        Directory.CreateDirectory(directory);
        return directory;
#elif STRIDE_PLATFORM_UWP
        return Windows.Storage.ApplicationData.Current.RoamingFolder.Path;
#elif STRIDE_PLATFORM_IOS
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", "Roaming");
        Directory.CreateDirectory(directory);
        return directory;
#else
        // TODO: Should we add "local" ?
        var directory = Path.Combine(GetApplicationBinaryDirectory(), "roaming");
        Directory.CreateDirectory(directory);
        return directory;
#endif
    }

    private static string GetApplicationCacheDirectory()
    {
#if STRIDE_PLATFORM_ANDROID
        var directory = Path.Combine(PlatformAndroid.Context.FilesDir.AbsolutePath, "cache");
#elif STRIDE_PLATFORM_UWP
        var directory = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "cache");
#elif STRIDE_PLATFORM_IOS
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", "Caches");
#else
        // TODO: Should we add "local" ?
        var directory = Path.Combine(GetApplicationBinaryDirectory(), "cache");
#endif
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string? GetApplicationExecutablePath()
    {
#if STRIDE_PLATFORM_ANDROID
        return PlatformAndroid.Context.PackageCodePath;
#elif STRIDE_PLATFORM_DESKTOP || STRIDE_PLATFORM_MONO_MOBILE
        var appPath = Assembly.GetEntryAssembly()?.Location;
        if (string.IsNullOrEmpty(appPath))
        {
            appPath = Environment.ProcessPath;
        }
        return appPath;
#else
        return null;
#endif
    }

    private static string GetTemporaryDirectory()
    {
        return GetApplicationTemporaryDirectory();
    }

    private static string GetApplicationTemporaryDirectory()
    {
#if STRIDE_PLATFORM_ANDROID
        return PlatformAndroid.Context.CacheDir.AbsolutePath;
#elif STRIDE_PLATFORM_UWP
        return Windows.Storage.ApplicationData.Current.TemporaryFolder.Path;
#elif STRIDE_PLATFORM_IOS
        return Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "tmp");
#else
        return Path.GetTempPath();
#endif
    }

    private static string GetApplicationBinaryDirectory()
    {
#if STRIDE_PLATFORM_ANDROID
        return GetApplicationExecutableDirectory();
#elif STRIDE_PLATFORM_DESKTOP
        return Path.GetDirectoryName(AppContext.BaseDirectory)!;
#else
        return Path.GetDirectoryName(typeof(PlatformFolders).Assembly.Location);
#endif
    }

    private static string GetApplicationExecutableDirectory()
    {
#if STRIDE_PLATFORM_DESKTOP || STRIDE_PLATFORM_MONO_MOBILE
        var executableName = GetApplicationExecutablePath();
        if (!string.IsNullOrEmpty(executableName))
        {
            return Path.GetDirectoryName(executableName)!;
        }
#if STRIDE_RUNTIME_CORECLR
        return AppContext.BaseDirectory;
#else
        return AppDomain.CurrentDomain.BaseDirectory;
#endif
#elif STRIDE_PLATFORM_UWP
        return Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#else
        throw new NotImplementedException();
#endif
    }

    private static string GetApplicationDataDirectory()
    {
#if STRIDE_PLATFORM_ANDROID
        return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Android/data/" + PlatformAndroid.Context.PackageName + "/data";
#elif STRIDE_PLATFORM_IOS
        return Foundation.NSBundle.MainBundle.BundlePath + "/data";
#elif STRIDE_PLATFORM_UWP
        return Windows.ApplicationModel.Package.Current.InstalledLocation.Path + @"\data";
#else
        return Path.Combine(GetApplicationBinaryDirectory(), "data");
#endif
    }
}
