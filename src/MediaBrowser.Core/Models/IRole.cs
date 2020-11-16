using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A role for a user.
    /// </summary>
    public interface IRole
    {
        /// <summary>
        /// The id for this role.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// A friendly description for the role.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The role name.
        /// </summary>
        string Name { get; }
    }
}
