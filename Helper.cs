using System;
using System.IO;

namespace SerialPortNET
{
    public enum Platform
    {
        Windows,
        Linux,
        Mac,
        Android
    }

    internal static class Helper
    {
        #region Public Constructors

        static Helper()
        {
            runningPlatform = getPlatform();
        }

        #endregion Public Constructors

        #region Private Methods

        private static Platform getPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists("/Applications")
                        & Directory.Exists("/System")
                        & Directory.Exists("/Users")
                        & Directory.Exists("/Volumes"))
                        return Platform.Mac;
                    else if (File.Exists("/system/build.prop"))
                        return Platform.Android;

                    return Platform.Linux;

                case PlatformID.MacOSX:
                    return Platform.Mac;

                default:
                    return Platform.Windows;
            }
        }

        #endregion Private Methods

        #region Public Properties

        public static Platform RunningPlatform { get { return runningPlatform; } }

        #endregion Public Properties

        #region Private Fields

        private static Platform runningPlatform;

        #endregion Private Fields
    }
}