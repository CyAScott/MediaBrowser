﻿using System.ComponentModel.DataAnnotations;

namespace MediaBrowser.Models
{
    /// <summary>
    /// The search parameters for finding users.
    /// </summary>
    public class SearchUsersRequest
    {
        /// <summary>
        /// Optional filter option.
        /// </summary>
        public UserFilterOptions Filter { get; set; } = UserFilterOptions.NoFilter;

        /// <summary>
        /// The field to sort by.
        /// </summary>
        public UserSortOptions Sort { get; set; } = UserSortOptions.UserName;

        /// <summary>
        /// Sorts in ascending order when true.
        /// </summary>
        public bool Ascending { get; set; } = true;

        /// <summary>
        /// The number of users to skip.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Skip { get; set; }

        /// <summary>
        /// The number of users to read.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Take { get; set; } = 1;

        /// <summary>
        /// Optional. Keywords to search with.
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Optional. The roles the user must have.
        /// </summary>
        public string[] Roles { get; set; }
    }

    /// <summary>
    /// The list of things to filter users by.
    /// </summary>
    public enum UserFilterOptions
    {
        /// <summary>
        /// Only returns deleted users.
        /// </summary>
        Deleted,

        /// <summary>
        /// No filter will be applied.
        /// </summary>
        NoFilter,

        /// <summary>
        /// Returns non deleted users.
        /// </summary>
        NonDeleted
    }

    /// <summary>
    /// The list of things to sort users by.
    /// </summary>
    public enum UserSortOptions
    {
        /// <summary>
        /// Sorts by deleted on.
        /// </summary>
        DeletedOn,

        /// <summary>
        /// Sorts by first name.
        /// </summary>
        FirstName,

        /// <summary>
        /// Sorts by last name.
        /// </summary>
        LastName,

        /// <summary>
        /// Sorts by user name.
        /// </summary>
        UserName
    }
}
