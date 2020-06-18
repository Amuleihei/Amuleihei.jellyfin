﻿#nullable enable
#pragma warning disable CA1801

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Api.Constants;
using Jellyfin.Api.Models.PluginDtos;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Jellyfin.Api.Controllers
{
    /// <summary>
    /// Plugins controller.
    /// </summary>
    [Authorize]
    public class PluginsController : BaseJellyfinApiController
    {
        private readonly IApplicationHost _appHost;
        private readonly IInstallationManager _installationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginsController"/> class.
        /// </summary>
        /// <param name="appHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="installationManager">Instance of the <see cref="IInstallationManager"/> interface.</param>
        public PluginsController(
            IApplicationHost appHost,
            IInstallationManager installationManager)
        {
            _appHost = appHost;
            _installationManager = installationManager;
        }

        /// <summary>
        /// Gets a list of currently installed plugins.
        /// </summary>
        /// <param name="isAppStoreEnabled">Optional. Unused.</param>
        /// <response code="200">Installed plugins returned.</response>
        /// <returns>List of currently installed plugins.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<PluginInfo>> GetPlugins([FromRoute] bool? isAppStoreEnabled)
        {
            return Ok(_appHost.Plugins.OrderBy(p => p.Name).Select(p => p.GetPluginInfo()));
        }

        /// <summary>
        /// Uninstalls a plugin.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="200">Plugin uninstalled.</response>
        /// <response code="404">Plugin not found.</response>
        /// <returns>An <see cref="OkResult"/> on success, or a <see cref="NotFoundResult"/> if the file could not be found.</returns>
        [HttpDelete("{pluginId}")]
        [Authorize(Policy = Policies.RequiresElevation)]
        public ActionResult UninstallPlugin([FromRoute] Guid pluginId)
        {
            var plugin = _appHost.Plugins.FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
            {
                return NotFound();
            }

            _installationManager.UninstallPlugin(plugin);
            return Ok();
        }

        /// <summary>
        /// Gets plugin configuration.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="200">Plugin configuration returned.</response>
        /// <response code="404">Plugin not found or plugin configuration not found.</response>
        /// <returns>Plugin configuration.</returns>
        [HttpGet("{pluginId}/Configuration")]
        public ActionResult<BasePluginConfiguration> GetPluginConfiguration([FromRoute] Guid pluginId)
        {
            if (!(_appHost.Plugins.FirstOrDefault(p => p.Id == pluginId) is IHasPluginConfiguration plugin))
            {
                return NotFound();
            }

            return plugin.Configuration;
        }

        /// <summary>
        /// Updates plugin configuration.
        /// </summary>
        /// <remarks>
        /// Accepts plugin configuration as JSON body.
        /// </remarks>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="200">Plugin configuration updated.</response>
        /// <response code="200">Plugin not found or plugin does not have configuration.</response>
        /// <returns>
        /// A <see cref="Task" /> that represents the asynchronous operation to update plugin configuration.
        ///    The task result contains an <see cref="OkResult"/> indicating success, or <see cref="NotFoundResult"/>
        ///    when plugin not found or plugin doesn't have configuration.
        /// </returns>
        [HttpPost("{pluginId}/Configuration")]
        public async Task<ActionResult> UpdatePluginConfiguration([FromRoute] Guid pluginId)
        {
            if (!(_appHost.Plugins.FirstOrDefault(p => p.Id == pluginId) is IHasPluginConfiguration plugin))
            {
                return NotFound();
            }

            var configuration = (BasePluginConfiguration)await JsonSerializer.DeserializeAsync(Request.Body, plugin.ConfigurationType)
                .ConfigureAwait(false);

            plugin.UpdateConfiguration(configuration);
            return Ok();
        }

        /// <summary>
        /// Get plugin security info.
        /// </summary>
        /// <response code="200">Plugin security info returned.</response>
        /// <returns>Plugin security info.</returns>
        [Obsolete("This endpoint should not be used.")]
        [HttpGet("SecurityInfo")]
        public ActionResult<PluginSecurityInfo> GetPluginSecurityInfo()
        {
            return new PluginSecurityInfo
            {
                IsMbSupporter = true,
                SupporterKey = "IAmTotallyLegit"
            };
        }

        /// <summary>
        /// Updates plugin security info.
        /// </summary>
        /// <param name="pluginSecurityInfo">Plugin security info.</param>
        /// <response code="200">Plugin security info updated.</response>
        /// <returns>An <see cref="OkResult"/>.</returns>
        [Obsolete("This endpoint should not be used.")]
        [HttpPost("SecurityInfo")]
        [Authorize(Policy = Policies.RequiresElevation)]
        public ActionResult UpdatePluginSecurityInfo([FromBody, BindRequired] PluginSecurityInfo pluginSecurityInfo)
        {
            return Ok();
        }

        /// <summary>
        /// Gets registration status for a feature.
        /// </summary>
        /// <param name="name">Feature name.</param>
        /// <response code="200">Registration status returned.</response>
        /// <returns>Mb registration record.</returns>
        [Obsolete("This endpoint should not be used.")]
        [HttpPost("RegistrationRecords/{name}")]
        public ActionResult<MBRegistrationRecord> GetRegistrationStatus([FromRoute] string name)
        {
            return new MBRegistrationRecord
            {
                IsRegistered = true,
                RegChecked = true,
                TrialVersion = false,
                IsValid = true,
                RegError = false
            };
        }

        /// <summary>
        /// Gets registration status for a feature.
        /// </summary>
        /// <param name="name">Feature name.</param>
        /// <response code="501">Not implemented.</response>
        /// <returns>Not Implemented.</returns>
        /// <exception cref="NotImplementedException">This endpoint is not implemented.</exception>
        [Obsolete("Paid plugins are not supported")]
        [HttpGet("/Registrations/{name}")]
        public ActionResult GetRegistration([FromRoute] string name)
        {
            // TODO Once we have proper apps and plugins and decide to break compatibility with paid plugins,
            // delete all these registration endpoints. They are only kept for compatibility.
            throw new NotImplementedException();
        }
    }
}
