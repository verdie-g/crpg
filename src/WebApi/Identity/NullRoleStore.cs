using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Crpg.WebApi.Identity
{
    // TODO: Needed if the user doesn't inherit IdentityUser. Find a way to get rid of it.
    internal class NullRoleStore : IRoleStore<IdentityRole>
    {
        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken) => throw new NotImplementedException();
        public void Dispose() { }
    }
}
