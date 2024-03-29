﻿using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A response model for <see cref="SearchUsersRequest"/>.
    /// </summary>
    public class SearchUsersResponse<TUser> : SearchUsersRequest
    {
        /// <inheritdoc/>
        public SearchUsersResponse(SearchUsersRequest request, int count, IEnumerable<TUser> users)
        {
            Ascending = request.Ascending;
            Count = count;
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
        /// The number of users the query could return.
        /// </summary>
        public int Count { get; set; }
    }
}
