using LiteDB;
using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class LiteDbRoles : IRoles
    {
        public LiteDbRoles(ILiteDatabase db)
        {
            Collection = db.GetCollection<LiteDbRole>("roles");
            Collection.EnsureIndex(it => it.Name, true);
            Database = db;
        }

        public ILiteCollection<LiteDbRole> Collection { get; }
        public ILiteDatabase Database { get; }

        public Task<IRole[]> All() => Task.FromResult(Collection.FindAll().OfType<IRole>().ToArray());

        public Task<long> Count() => Task.FromResult(Collection.LongCount());

        public Task<IRole> Create(CreateRoleRequest request)
        {
            var role = new LiteDbRole
            {
                Description = request.Description,
                Id = Guid.NewGuid(),
                Name = request.Name.ToLower()
            };

            Collection.Insert(role);

            return Task.FromResult((IRole)role);
        }

        public Task<IRole> Get(Guid roleId) =>
            Task.FromResult((IRole)Collection.FindById(roleId));

        public Task<IRole> GetByName(string name) =>
            Task.FromResult((IRole)Collection.FindOne(Query.EQ(nameof(LiteDbRole.Name), name.ToLower())));

        public Task<SearchRolesResponse<IRole>> Search(SearchRolesRequest request)
        {
            var query = Query.All();

            query.Offset = request.Skip;
            query.Limit = request.Take;

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var keywordQuery = new List<BsonExpression>();

                foreach (var term in Regex.Split(request.Keywords, @"\s+"))
                {
                    keywordQuery.Add(Query.Contains(nameof(LiteDbRole.Description), term));
                    keywordQuery.Add(Query.Contains(nameof(LiteDbRole.Name), term));
                }

                if (keywordQuery.Count > 0)
                {
                    query.Where.Add(Query.Or(keywordQuery.ToArray()));
                }
            }

            switch (request.Sort)
            {
                case RoleSortOptions.Description:
                    query.OrderBy = nameof(LiteDbRole.Description);
                    break;
                case RoleSortOptions.Name:
                    query.OrderBy = nameof(LiteDbRole.Name);
                    break;
            }

            query.Order = request.Ascending ? Query.Ascending : Query.Descending;

            var results = Collection.Find(query).ToArray();

            var response = new SearchRolesResponse<IRole>(request, 0, results.Cast<IRole>());

            if (results.Length < request.Take)
            {
                response.Count = request.Skip + results.Length;
            }
            else
            {
                query.Offset = 0;
                query.Limit = int.MaxValue;
                response.Count = Collection.Count(query);
            }

            return Task.FromResult(response);
        }

        public Task<IRole> Update(Guid roleId, UpdateRoleRequest request)
        {
            var role = Collection.FindById(roleId);

            if (role == null)
            {
                return Task.FromResult((IRole)null);
            }

            role.Description = request.Description ?? role.Description;

            Collection.Update(role.Id, role);

            return Task.FromResult((IRole)role);
        }
    }

    public class LiteDbRole : IRole
    {
        [BsonId]
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }
    }
}
