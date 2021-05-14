using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A read model for a user of this service.
    /// </summary>
    public class UserReadModel
    {
        private readonly IUser user;

        /// <inheritdoc/>
        public UserReadModel(IUser user) => this.user = user ?? throw new ArgumentNullException(nameof(user));

        /// <summary>
        /// When the user was soft deleted.
        /// </summary>
        public DateTime? DeletedOn => user.DeletedOn;

        /// <summary>
        /// The role names the user was assigned to.
        /// </summary>
        public RoleSet Roles => user.Roles;

        /// <summary>
        /// The id for this user.
        /// </summary>
        public Guid Id => user.Id;

        /// <summary>
        /// The first name for this user.
        /// </summary>
        public string FirstName => user.FirstName;

        /// <summary>
        /// The last name for this user.
        /// </summary>
        public string LastName => user.LastName;

        /// <summary>
        /// The user name for the user.
        /// </summary>
        public string UserName => user.UserName;
    }
}
