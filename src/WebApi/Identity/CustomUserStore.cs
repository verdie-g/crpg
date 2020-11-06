using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Users.Models;
using Crpg.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Crpg.WebApi.Identity
{
    internal class CustomUserStore : IUserLoginStore<UserViewModel>, IUserRoleStore<UserViewModel>
    {
        private readonly IMediator _mediator;

        public CustomUserStore(IMediator mediator) => _mediator = mediator;

        public async Task<UserViewModel> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var res = await _mediator.Send(new GetUserQuery { UserId = int.Parse(userId) }, cancellationToken);
            return res.Data!;
        }

        public Task<string> GetUserIdAsync(UserViewModel user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
        public Task<string> GetUserNameAsync(UserViewModel user, CancellationToken cancellationToken) => Task.FromResult(user.Name);
        public Task<IList<string>> GetRolesAsync(UserViewModel user, CancellationToken cancellationToken) => Task.FromResult((IList<string>)new[] { user.Role.ToString() });

        public Task<IdentityResult> CreateAsync(UserViewModel user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(UserViewModel user, string normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<UserViewModel> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<UserViewModel> FindByNameAsync(string normalizedUserViewModelName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetNormalizedUserNameAsync(UserViewModel user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> DeleteAsync(UserViewModel user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetUserNameAsync(UserViewModel user, string userName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(UserViewModel user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task AddLoginAsync(UserViewModel user, UserLoginInfo login, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<UserLoginInfo>> GetLoginsAsync(UserViewModel user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task RemoveLoginAsync(UserViewModel user, string loginProvider, string providerKey, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task AddToRoleAsync(UserViewModel user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IList<UserViewModel>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> IsInRoleAsync(UserViewModel user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task RemoveFromRoleAsync(UserViewModel user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public void Dispose() { }
    }
}
