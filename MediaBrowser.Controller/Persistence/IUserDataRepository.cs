﻿using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using System.Threading;

namespace MediaBrowser.Controller.Persistence
{
    /// <summary>
    /// Provides an interface to implement a UserData repository
    /// </summary>
    public interface IUserDataRepository : IRepository
    {
        /// <summary>
        /// Saves the user data.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="key">The key.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        void SaveUserData(long userId, string key, UserItemData userData, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="key">The key.</param>
        /// <returns>Task{UserItemData}.</returns>
        UserItemData GetUserData(long userId, string key);

        UserItemData GetUserData(long userId, List<string> keys);

        /// <summary>
        /// Return all user data associated with the given user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<UserItemData> GetAllUserData(long userId);

        /// <summary>
        /// Save all user data associated with the given user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        void SaveAllUserData(long userId, UserItemData[] userData, CancellationToken cancellationToken);

    }
}
