using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            // Don´t validate Scope in our daemon App
            // This is because when using ClientCredentials flow, the V2 Token doesn´t contain the "scope" claim
            // https://github.com/MicrosoftDocs/azure-docs/issues/10016
            // se comments here: https://blogs.msdn.microsoft.com/gianlucb/2017/12/05/azure-ad-scope-based-authorization/
            if (context.User.FindFirstValue("azp") == "e2b3ebbd-91a3-4bee-ae3a-2667e02f3ba4")
            {
                context.Succeed(requirement);
            }
            else
            {
                if (!context.User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope"))
                    return Task.CompletedTask;

                var scopesInClaims = context.User.FindFirst(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope").Value.Split(' ');

                var hasAllScopes = !_scopes.Except(scopesInClaims).Any();

                if (hasAllScopes)
                {
                    context.Succeed(requirement);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
