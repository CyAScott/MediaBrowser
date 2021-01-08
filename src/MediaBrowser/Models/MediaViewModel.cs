using Newtonsoft.Json;
using System.Collections.Generic;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A view model for the media view.
    /// </summary>
    public class MediaViewModel
    {
        /// <summary>
        /// All roles.
        /// </summary>
        [JsonProperty("allRoles")]
        public HashSet<string> AllRoles { get; set; }

        /// <summary>
        /// The role names the user was assigned to.
        /// </summary>
        [JsonProperty("roles")]
        public HashSet<string> Roles { get; set; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// The user's user name.
        /// </summary>
        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}
