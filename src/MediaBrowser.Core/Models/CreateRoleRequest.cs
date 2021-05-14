using System.ComponentModel.DataAnnotations;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to create a user role.
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// A friendly description for the role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The role name.
        /// </summary>
        [Required, RegularExpression(@"[A-Z\d_-]{1,255}")]
        public string Name { get; set; }
    }
}
