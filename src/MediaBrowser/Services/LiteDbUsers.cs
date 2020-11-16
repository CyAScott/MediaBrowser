using LiteDB;
using MediaBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCryptCs = BCrypt.Net.BCrypt;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MediaBrowser.Services
{
    public class LiteDbUsers : IUsers
    {
        public LiteDbUsers(ILiteDatabase db)
        {
            Collection = db.GetCollection<LiteDbUser>("users");
            Collection.EnsureIndex(it => it.UserName, true);
            Database = db;
        }

        public ILiteCollection<LiteDbUser> Collection { get; }
        public ILiteDatabase Database { get; }

        public Task<IUser> Create(CreateUserRequest request)
        {
            var user = new LiteDbUser
            {
                DeletedOn = null,
                FirstName = request.FirstName,
                Id = Guid.NewGuid(),
                LastName = request.LastName,
                Password = BCryptCs.HashPassword(request.Password),
                Roles = request.Roles ?? new string[0],
                UserName = request.UserName.ToLower()
            };

            Collection.Insert(user);

            return Task.FromResult((IUser)user);
        }

        public Task<IUser> Delete(Guid userId)
        {
            var user = Collection.FindById(userId);

            if (user == null || user.DeletedOn != null)
            {
                return Task.FromResult((IUser)null);
            }

            user.DeletedOn = DateTime.UtcNow;

            Collection.Update(userId, user);

            return Task.FromResult((IUser)user);
        }

        public Task<IUser> Get(Guid userId) =>
            Task.FromResult((IUser)Collection.FindById(userId));

        public Task<IUser> GetByUserName(string userName) =>
            Task.FromResult((IUser)Collection.FindOne(Query.EQ(nameof(LiteDbUser.UserName), userName.ToLower())));

        public Task<SearchUsersResponse<IUser>> Search(SearchUsersRequest request)
        {
            var query = Query.All();

            query.Offset = request.Skip;
            query.Limit = request.Take;

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var keywordQuery = new List<BsonExpression>();

                foreach (var term in Regex.Split(request.Keywords, @"\s+"))
                {
                    keywordQuery.Add(Query.Contains(nameof(LiteDbUser.FirstName), term));
                    keywordQuery.Add(Query.Contains(nameof(LiteDbUser.UserName), term));
                    keywordQuery.Add(Query.Contains(nameof(LiteDbUser.LastName), term));
                }

                if (keywordQuery.Count > 0)
                {
                    query.Where.Add(Query.Or(keywordQuery.ToArray()));
                }
            }

            if (request.Roles != null && request.Roles.Length > 0)
            {
                foreach (var role in request.Roles.Distinct())
                {
                    query.Where.Add(Query.EQ(nameof(LiteDbUser.Roles), role));
                }
            }

            if (request.Filter != UserFilterOptions.Nofilter)
            {
                switch (request.Filter)
                {
                    case UserFilterOptions.Deleted:
                        query.Where.Add(Query.EQ(nameof(LiteDbUser.DeletedOn), BsonValue.Null));
                        break;
                    case UserFilterOptions.NonDelted:
                        query.Where.Add(Query.Not(nameof(LiteDbUser.DeletedOn), BsonValue.Null));
                        break;
                }
            }


            switch (request.Sort)
            {
                case UserSortOptions.DeletedOnAscending:
                    query.OrderBy = nameof(LiteDbUser.DeletedOn);
                    query.Order = Query.Ascending;
                    break;
                case UserSortOptions.DeletedOnDescending:
                    query.OrderBy = nameof(LiteDbUser.DeletedOn);
                    query.Order = Query.Descending;
                    break;
                case UserSortOptions.FirstNameAscending:
                    query.OrderBy = nameof(LiteDbUser.FirstName);
                    query.Order = Query.Ascending;
                    break;
                case UserSortOptions.FirstNameDescending:
                    query.OrderBy = nameof(LiteDbUser.FirstName);
                    query.Order = Query.Descending;
                    break;
                case UserSortOptions.LastNameAscending:
                    query.OrderBy = nameof(LiteDbUser.LastName);
                    query.Order = Query.Ascending;
                    break;
                case UserSortOptions.LastNameDescending:
                    query.OrderBy = nameof(LiteDbUser.LastName);
                    query.Order = Query.Descending;
                    break;
                case UserSortOptions.UserNameAscending:
                    query.OrderBy = nameof(LiteDbUser.UserName);
                    query.Order = Query.Ascending;
                    break;
                case UserSortOptions.UserNameDescending:
                    query.OrderBy = nameof(LiteDbUser.UserName);
                    query.Order = Query.Descending;
                    break;
            }

            var results = Collection.Find(query).ToArray();

            var response = new SearchUsersResponse<IUser>(request, results.Cast<IUser>());

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

        public Task<IUser> Update(Guid userId, UpdateUserRequest request)
        {
            var user = Collection.FindById(userId);

            if (user == null)
            {
                return Task.FromResult((IUser)null);
            }

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.UserName = request.UserName?.ToLower() ?? user.UserName;

            Collection.Update(user.Id, user);

            return Task.FromResult((IUser)user);
        }
    }

    public class LiteDbUser : IUser
    {
        public Guid Id { get; set; }

        public DateTime? DeletedOn { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public string[] Roles { get; set; }

        public bool IsPasswordValid(string password) => BCryptCs.Verify(password, Password);
    }
}
