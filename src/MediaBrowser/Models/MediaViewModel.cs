using System.Collections.Generic;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A view model for the media view.
    /// </summary>
    public class MediaViewModel
    {
        /// <summary>
        /// The role names the user was assigned to.
        /// </summary>
        public HashSet<string> Roles { get; set; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The user's id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The user's user name.
        /// </summary>
        public string UserName { get; set; }
    }
}
