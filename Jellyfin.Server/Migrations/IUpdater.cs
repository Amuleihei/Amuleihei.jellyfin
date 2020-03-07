using System;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Migrations
{
    /// <summary>
    /// Interface that descibes a migration routine.
    /// </summary>
    internal interface IUpdater
    {
        /// <summary>
        /// Gets the name of the migration, must be unique.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Execute the migration routine.
        /// </summary>
        /// <param name="host">Host that hosts current version.</param>
        /// <param name="logger">Host logger.</param>
        public abstract void Perform(CoreAppHost host, ILogger logger);
    }
}
