﻿using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Updates;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.Services;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Api
{
    /// <summary>
    /// Class Plugins
    /// </summary>
    [Route("/Plugins", "GET", Summary = "Gets a list of currently installed plugins")]
    [Authenticated]
    public class GetPlugins : IReturn<PluginInfo[]>
    {
        public bool? IsAppStoreEnabled { get; set; }
    }

    /// <summary>
    /// Class UninstallPlugin
    /// </summary>
    [Route("/Plugins/{Id}", "DELETE", Summary = "Uninstalls a plugin")]
    [Authenticated(Roles = "Admin")]
    public class UninstallPlugin : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Plugin Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "DELETE")]
        public string Id { get; set; }
    }

    /// <summary>
    /// Class GetPluginConfiguration
    /// </summary>
    [Route("/Plugins/{Id}/Configuration", "GET", Summary = "Gets a plugin's configuration")]
    [Authenticated]
    public class GetPluginConfiguration
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Plugin Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }

    /// <summary>
    /// Class UpdatePluginConfiguration
    /// </summary>
    [Route("/Plugins/{Id}/Configuration", "POST", Summary = "Updates a plugin's configuration")]
    [Authenticated]
    public class UpdatePluginConfiguration : IRequiresRequestStream, IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Plugin Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Id { get; set; }

        /// <summary>
        /// The raw Http Request Input Stream
        /// </summary>
        /// <value>The request stream.</value>
        public Stream RequestStream { get; set; }
    }


    /// <summary>
    /// Class PluginsService
    /// </summary>
    public class PluginService : BaseApiService
    {
        /// <summary>
        /// The _json serializer
        /// </summary>
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// The _app host
        /// </summary>
        private readonly IApplicationHost _appHost;
        private readonly IInstallationManager _installationManager;
        private readonly INetworkManager _network;
        private readonly IDeviceManager _deviceManager;

        public PluginService(IJsonSerializer jsonSerializer, IApplicationHost appHost, IInstallationManager installationManager, INetworkManager network, IDeviceManager deviceManager)
            : base()
        {
            if (jsonSerializer == null)
            {
                throw new ArgumentNullException(nameof(jsonSerializer));
            }

            _appHost = appHost;
            _installationManager = installationManager;
            _network = network;
            _deviceManager = deviceManager;
            _jsonSerializer = jsonSerializer;
        }


        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public async Task<object> Get(GetPlugins request)
        {
            // TODO cvium
              var result = _appHost.Plugins.OrderBy(p => p.Name).Select(p => p.GetPluginInfo()).ToArray();
//            var requireAppStoreEnabled = request.IsAppStoreEnabled.HasValue && request.IsAppStoreEnabled.Value;
//
//            // Don't fail just on account of image url's
//            try
//            {
//                var packages = (await _installationManager.GetAvailablePackagesWithoutRegistrationInfo(CancellationToken.None));
//
//                foreach (var plugin in result)
//                {
//                    var pkg = packages.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.guid) && string.Equals(i.guid.Replace("-", string.Empty), plugin.Id.Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase));
//
//                    if (pkg != null)
//                    {
//                        plugin.ImageUrl = pkg.thumbImage;
//                    }
//                }
//
//                if (requireAppStoreEnabled)
//                {
//                    result = result
//                        .Where(plugin =>
//                        {
//                            var pkg = packages.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.guid) && new Guid(plugin.Id).Equals(new Guid(i.guid)));
//                            return pkg != null && pkg.enableInAppStore;
//
//                        })
//                        .ToArray();
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex, "Error getting plugin list");
//                // Play it safe here
//                if (requireAppStoreEnabled)
//                {
//                    result = new PluginInfo[] { };
//                }
//            }
            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetPluginConfiguration request)
        {
            var guid = new Guid(request.Id);
            var plugin = _appHost.Plugins.First(p => p.Id == guid) as IHasPluginConfiguration;

            return ToOptimizedResult(plugin.Configuration);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task Post(UpdatePluginConfiguration request)
        {
            // We need to parse this manually because we told service stack not to with IRequiresRequestStream
            // https://code.google.com/p/servicestack/source/browse/trunk/Common/ServiceStack.Text/ServiceStack.Text/Controller/PathInfo.cs
            var id = new Guid(GetPathValue(1));

            var plugin = _appHost.Plugins.First(p => p.Id == id) as IHasPluginConfiguration;

            if (plugin == null)
            {
                throw new FileNotFoundException();
            }

            var configuration = (await _jsonSerializer.DeserializeFromStreamAsync(request.RequestStream, plugin.ConfigurationType).ConfigureAwait(false)) as BasePluginConfiguration;

            plugin.UpdateConfiguration(configuration);
        }

        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Delete(UninstallPlugin request)
        {
            var guid = new Guid(request.Id);
            var plugin = _appHost.Plugins.First(p => p.Id == guid);

            _installationManager.UninstallPlugin(plugin);
        }
    }
}
