namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to update a user for this service.
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// Optional. If provided changes the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Optional. If provided changes the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Optional. If provided changes the user name.
        /// </summary>
        public string UserName { get; set; }
    }
}
