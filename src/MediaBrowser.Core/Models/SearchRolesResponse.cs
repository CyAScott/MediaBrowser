using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A response model for <see cref="SearchRolesRequest"/>.
    /// </summary>
    public class SearchRolesResponse<TRole>
    {
        /// <inheritdoc/>
        public SearchRolesResponse(SearchRolesRequest request, IEnumerable<TRole> roles)
        {
            Keywords = request.Keywords;
            Results = roles.ToArray();
            Skip = request.Skip;
            Sort = request.Sort;
            Take = request.Take;
        }

        /// <summary>
        /// The found roles.
        /// </summary>
        public TRole[] Results { get; }

        /// <summary>
        /// The field to sort by.
        /// </summary>
        public RoleSortOptions Sort { get; set; } = RoleSortOptions.NameAscending;

        /// <summary>
        /// The number of roles the query could return.
        /// </summary>
        public int Count { get; set; }

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
}
