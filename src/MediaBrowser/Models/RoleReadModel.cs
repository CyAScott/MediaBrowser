using System;

namespace MediaBrowser.Models
{
    /// <summary>
    /// A read model for a user role.
    /// </summary>
    public class RoleReadModel : IRole
    {
        private readonly IRole role;

        /// <inheritdoc/>
        public RoleReadModel(IRole role) => this.role = role ?? throw new ArgumentNullException(nameof(role));

        /// <inheritdoc/>
        public Guid Id => role.Id;

        /// <inheritdoc/>
        public string Description => role.Description;

        /// <inheritdoc/>
        public string Name => role.Name;
    }
}
