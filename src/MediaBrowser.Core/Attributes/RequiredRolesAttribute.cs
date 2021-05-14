using MediaBrowser.Models;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// The user must have all the following roles to use the Api.
    /// </summary>
    public class RequiredRoleAttribute : BaseRoleRequirementAttribute
    {
        /// <inheritdoc/>
        public RequiredRoleAttribute(params string[] roles) => Roles = new RoleSet(roles);

        /// <summary>
        /// The required roles.
        /// </summary>
        public RoleSet Roles { get; }

        /// <inheritdoc/>
        public override bool MeetsRequirements(IUser user) => user?.Roles != null && Roles.IsSubsetOf(user.Roles);
    }
}
