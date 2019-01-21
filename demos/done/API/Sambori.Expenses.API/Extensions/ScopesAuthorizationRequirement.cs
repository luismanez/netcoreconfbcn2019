using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Sambori.Expenses.API.Extensions
{
    public class ScopesAuthorizationRequirement : AuthorizationHandler<ScopesAuthorizationRequirement>, IAuthorizationRequirement
    {
        private readonly IEnumerable<string> _scopes;

        public ScopesAuthorizationRequirement(IEnumerable<string> scopes)
        {
            _scopes = scopes;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopesAuthorizationRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope"))
                return Task.CompletedTask;

            var scopesInClaims = context.User.FindFirst(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope").Value.Split(' ');

            var hasAllScopes = !_scopes.Except(scopesInClaims).Any();

            if (hasAllScopes)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
