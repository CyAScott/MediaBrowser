using MediaBrowser.Attributes;
using MediaBrowser.Models;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class DbInit : IHaveInit
    {
        public DbInit(AuthConfig config, IRoles roles, IUsers users)
        {
            Config = config;
            Roles = roles;
            Users = users;
        }

        public AuthConfig Config { get; }
        public IRoles Roles { get; }
        public IUsers Users { get; }

        public async Task Init()
        {
            var role = await Roles.GetByName(RequiresAdminRoleAttribute.AdminRole);
            if (role == null)
            {
                await Roles.Create(new CreateRoleRequest
                {
                    Description = "The admin role for user and role management.",
                    Name = RequiresAdminRoleAttribute.AdminRole
                });
            }

            var response = await Users.Search(new SearchUsersRequest
            {
                Take = 2
            });
            if (response.Results.Length == 0)
            {
                await Users.Create(new CreateUserRequest
                {
                    FirstName = Config.InitFirstName,
                    LastName = Config.InitLastName,
                    Password = Config.InitUserPassword,
                    Roles = new [] { RequiresAdminRoleAttribute.AdminRole },
                    UserName = Config.InitUserName
                });
            }
        }
    }
}
