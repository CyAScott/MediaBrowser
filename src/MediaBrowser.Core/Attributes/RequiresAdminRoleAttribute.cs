using MediaBrowser.Models;
using System;
using System.Linq;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// The user must have the <see cref="AdminRole"/> to use the Api.
    /// </summary>
    public class RequiresAdminRoleAttribute : BaseRoleRequirementAttribute
    {
        /// <summary>
        /// The admin roles grants the user full CRUD access for users and roles.
        /// </summary>
        public const string AdminRole = "Admin";

        /// <inheritdoc/>
        public override bool MeetsRequirements(IUser user) => user?.Roles != null && user.Roles.Contains(AdminRole, StringComparer.OrdinalIgnoreCase);
    }
}
