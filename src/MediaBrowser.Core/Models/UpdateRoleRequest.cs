namespace MediaBrowser.Models
{
    /// <summary>
    /// A request to update a user role for this service.
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// Optional. If provided changes the friendly description for the role.
        /// </summary>
        public string Description { get; set; }
    }
}
