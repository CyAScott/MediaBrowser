using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A response model for <see cref="SearchUsersRequest"/>.
    /// </summary>
    public class SearchUsersResponse<TUser>
    {
        /// <inheritdoc/>
        public SearchUsersResponse(SearchUsersRequest request, IEnumerable<TUser> users)
        {
            Filter = request.Filter;
            Keywords = request.Keywords;
            Results = users.ToArray();
            Roles = request.Roles;
            Skip = request.Skip;
            Sort = request.Sort;
            Take = request.Take;
        }

        /// <summary>
        /// The found users.
        /// </summary>
        public TUser[] Results { get; }

        /// <summary>
        /// Optional filter option.
        /// </summary>
        public UserFilterOptions Filter { get; set; }

        /// <summary>
        /// The field to sort by.
        /// </summary>
        public UserSortOptions Sort { get; set; } = UserSortOptions.UserNameAscending;

        /// <summary>
        /// The number of users the query could return.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The number of users to skip.
        /// </summary>
        public int Skip { get; }

        /// <summary>
        /// The number of users to read.
        /// </summary>
        public int Take { get; }

        /// <summary>
        /// Optional. Keywords to search with.
        /// </summary>
        public string Keywords { get; }

        /// <summary>
        /// Optional. The roles the user must have.
        /// </summary>
        public string[] Roles { get; set; }
    }
}
