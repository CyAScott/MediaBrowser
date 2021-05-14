using MediaBrowser.Models;
using MediaBrowser.Services;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IRoles"/>.
    /// </summary>
    public static class RolesExtensions
    {
        /// <summary>
        /// Checks to see if some roles to do exists.
        /// </summary>
        public static async Task<bool> DoRolesExists(this IRoles roles, params RoleSet[] roleSets)
        {
            var combinedRoles = new RoleSet();

            foreach (var set in roleSets)
            {
                if (set != null)
                {
                    combinedRoles.UnionWith(set);
                }
            }

            if (combinedRoles.Count == 0)
            {
                return true;
            }

            var matchedRoles = (await Task.WhenAll(combinedRoles
                .Select(it => it.ToUpper())
                .Select(roles.GetByName)))
                .Where(it => !string.IsNullOrEmpty(it?.Name))
                .Count();

            return combinedRoles.Count == matchedRoles;
        }

    }
}
