using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A user that can access media system.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// When the user was soft deleted.
        /// </summary>
        DateTime? DeletedOn { get; }

        /// <summary>
        /// The id for this user.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The role names the user was assigned to.
        /// </summary>
        RoleSet Roles { get; }

        /// <summary>
        /// Checks if a password is valid.
        /// </summary>
        bool IsPasswordValid(string password);

        /// <summary>
        /// The first name for this user.
        /// </summary>
        string FirstName { get; }

        /// <summary>
        /// The last name for this user.
        /// </summary>
        string LastName { get; }

        /// <summary>
        /// The user name for the user.
        /// </summary>
        string UserName { get; }
    }
}
