using System.ComponentModel.DataAnnotations;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to create a new user.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// The first name for this user.
        /// </summary>
        [RegularExpression(@"[a-zA-Z\-]{1,255}"), Required]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name for this user.
        /// </summary>
        [RegularExpression(@"[a-zA-Z\-]{1,255}"), Required]
        public string LastName { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        [RegularExpression(@".{8,255}"), Required]
        public string Password { get; set; }

        /// <summary>
        /// The user name for the user.
        /// </summary>
        [RegularExpression(@"[a-zA-Z\-]{1,255}"), Required]
        public string UserName { get; set; }

        /// <summary>
        /// The roles to assign the user.
        /// </summary>
        public RoleSet Roles { get; set; }
    }
}
