using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A response model for <see cref="SearchRolesRequest"/>.
    /// </summary>
    public class SearchRolesResponse<TRole> : SearchRolesRequest
    {
        /// <inheritdoc/>
        public SearchRolesResponse(SearchRolesRequest request, int count, IEnumerable<TRole> roles)
        {
            Ascending = request.Ascending;
            Count = count;
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
        /// The number of roles the query could return.
        /// </summary>
        public int Count { get; set; }
    }
}
