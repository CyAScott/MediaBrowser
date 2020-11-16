using MediaBrowser.Models;
using System;

namespace MediaBrowser.Attributes
{
    /// <summary>
    /// A base class for evaluating roles.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class BaseRoleRequirementAttribute : Attribute
    {
        /// <summary>
        /// Returns true of the user meets the role requirements.
        /// </summary>
        public abstract bool MeetsRequirements(IUser user);
    }
}
