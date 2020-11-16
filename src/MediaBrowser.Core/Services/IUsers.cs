using MediaBrowser.Models;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// An object for user CRUD.
    /// </summary>
    public interface IUsers
    {
        /// <summary>
        /// Gets a user by the user name.
        /// </summary>
        Task<IUser> GetByUserName(string userName);

        /// <summary>
        /// Create a user.
        /// </summary>
        Task<IUser> Create(CreateUserRequest request);

        /// <summary>
        /// Read a user by id.
        /// </summary>
        Task<IUser> Get(Guid userId);

        /// <summary>
        /// Search users.
        /// </summary>
        Task<SearchUsersResponse<IUser>> Search(SearchUsersRequest request);

        /// <summary>
        /// Read a user by id.
        /// </summary>
        Task<IUser> Update(Guid userId, UpdateUserRequest request);

        /// <summary>
        /// Soft deletes a user by id.
        /// </summary>
        Task<IUser> Delete(Guid userId);
    }
}
