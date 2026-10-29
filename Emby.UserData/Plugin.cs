using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Emby.UserData
{
    public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
    {
        public static Plugin Instance { get; private set; }
        public override string Name => "User Data";
        public override string Description => "Custom, administrator-managed fields for Emby users.";
        public override Guid Id => new Guid("b3f9e68e-e9ba-4fa8-a2bf-7f98d9c05f3e");
        public Plugin(IApplicationPaths paths, IXmlSerializer xmlSerializer) : base(paths, xmlSerializer) { Instance = this; }
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo {
                Name = "UserDataConfiguration",
                DisplayName = "Gestión Usuarios",
                EmbeddedResourcePath = GetType().Namespace + ".Web.UserDataConfiguration.html",
                EnableInMainMenu = true,
                MenuSection = "Server",
                MenuIcon = "people",
                IsMainConfigPage = true
            };
            yield return new PluginPageInfo {
                Name = "UserDataValuesConfiguration",
                DisplayName = "Gestión Datos Usuario",
                EmbeddedResourcePath = GetType().Namespace + ".Web.UserDataValuesConfiguration.html",
                EnableInMainMenu = true,
                MenuIcon = "person"
            };
            yield return new PluginPageInfo {
                Name = "UserDataBackupConfiguration",
                DisplayName = "Copia de seguridad",
                EmbeddedResourcePath = GetType().Namespace + ".Web.UserDataBackupConfiguration.html",
                EnableInMainMenu = true,
                MenuIcon = "backup"
            };
            yield return new PluginPageInfo {
                Name = "UserDataConfigurationjs",
                EmbeddedResourcePath = GetType().Namespace + ".Web.UserDataConfiguration.js"
            };
        }
        public Stream GetThumbImage() => Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType().Namespace + ".Images.icon.png");
        public ImageFormat ThumbImageFormat => ImageFormat.Png;
    }
}
