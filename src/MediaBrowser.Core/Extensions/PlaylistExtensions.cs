using MediaBrowser.Attributes;
using MediaBrowser.Models;
using System;
using System.Collections.Generic;

namespace MediaBrowser.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IPlaylist"/>.
    /// </summary>
    public static class PlaylistExtensions
    {
        /// <summary>
        /// Checks if a user has read access to a file.
        /// </summary>
        public static bool CanRead(this IPlaylist playlist, Guid userId, HashSet<string> userRoles) =>
            userId == playlist.CreatedBy ||
            userRoles.Contains(RequiresAdminRoleAttribute.AdminRole) ||
            userRoles.Overlaps(playlist.ReadRoles);

        /// <summary>
        /// Checks if a user has read access to a file.
        /// </summary>
        public static bool CanUpdate(this IPlaylist playlist, Guid userId, HashSet<string> userRoles) =>
            userId == playlist.CreatedBy ||
            userRoles.Contains(RequiresAdminRoleAttribute.AdminRole) ||
            userRoles.Overlaps(playlist.UpdateRoles);
    }
}
