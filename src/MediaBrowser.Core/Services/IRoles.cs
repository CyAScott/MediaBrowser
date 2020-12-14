using MediaBrowser.Models;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Services
{
    /// <summary>
    /// An object for role CRUD.
    /// </summary>
    public interface IRoles
    {
        /// <summary>
        /// Reads all roles.
        /// </summary>
        Task<IRole[]> All();

        /// <summary>
        /// Counts all roles.
        /// </summary>
        Task<long> Count();

        /// <summary>
        /// Gets a role by name.
        /// </summary>
        Task<IRole> GetByName(string name);

        /// <summary>
        /// Create a role.
        /// </summary>
        Task<IRole> Create(CreateRoleRequest request);

        /// <summary>
        /// Read a role by id.
        /// </summary>
        Task<IRole> Get(Guid roleId);

        /// <summary>
        /// Search roles.
        /// </summary>
        Task<SearchRolesResponse<IRole>> Search(SearchRolesRequest request);

        /// <summary>
        /// Updates a role by id.
        /// </summary>
        Task<IRole> Update(Guid roleId, UpdateRoleRequest request);
    }
}
