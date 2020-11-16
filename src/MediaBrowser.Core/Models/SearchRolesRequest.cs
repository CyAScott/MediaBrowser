using System.ComponentModel.DataAnnotations;

namespace MediaBrowser.Models
{
    /// <summary>
    /// The search parameters for finding user roles.
    /// </summary>
    public class SearchRolesRequest
    {
        /// <summary>
        /// The field to sort by.
        /// </summary>
        public RoleSortOptions Sort { get; set; } = RoleSortOptions.NameAscending;

        /// <summary>
        /// The number of roles to skip.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Skip { get; set; }

        /// <summary>
        /// The number of roles to read.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Take { get; set; } = 1;

        /// <summary>
        /// Optional. Keywords to search with.
        /// </summary>
        public string Keywords { get; set; }
    }

    /// <summary>
    /// The list of things to sort user roles by.
    /// </summary>
    public enum RoleSortOptions
    {
        /// <summary>
        /// Sorts by description on in ascending order.
        /// </summary>
        DescriptionAscending,

        /// <summary>
        /// Sorts by description on in descending order.
        /// </summary>
        DescriptionDescending,

        /// <summary>
        /// Sorts by name on in ascending order.
        /// </summary>
        NameAscending,

        /// <summary>
        /// Sorts by name on in descending order.
        /// </summary>
        NameDescending
    }
}
