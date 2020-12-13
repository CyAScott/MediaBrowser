using MediaBrowser.Attributes;
using MediaBrowser.Models;
using System;
using System.Collections.Generic;

namespace MediaBrowser.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IFile"/>.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Checks if a user has read access to a file.
        /// </summary>
        public static bool CanRead(this IFile file, Guid userId, HashSet<string> userRoles) =>
            userId == file.UploadedBy ||
            userRoles.Contains(RequiresAdminRoleAttribute.AdminRole) ||
            userRoles.Overlaps(file.ReadRoles);

        /// <summary>
        /// Checks if a user has read access to a file.
        /// </summary>
        public static bool CanUpdate(this IFile file, Guid userId, HashSet<string> userRoles) =>
            userId == file.UploadedBy ||
            userRoles.Contains(RequiresAdminRoleAttribute.AdminRole) ||
            userRoles.Overlaps(file.UpdateRoles);
    }
}
