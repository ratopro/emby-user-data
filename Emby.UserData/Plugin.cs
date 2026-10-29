using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Emby.UserData
{
    public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
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
                DisplayName = "Datos de usuarios",
                EmbeddedResourcePath = GetType().Namespace + ".Web.UserDataConfiguration.html",
                IsMainConfigPage = true
            };
            yield return new PluginPageInfo {
                Name = "UserDataConfigurationjs",
                EmbeddedResourcePath = GetType().Namespace + ".Web.UserDataConfiguration.js"
            };
        }
    }
}
