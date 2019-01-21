using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sambori.Expenses.API.Extensions
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policyBuilder,
            params string[] scopes)
        {
            if (scopes == null)
                throw new ArgumentNullException(nameof(scopes));
            return policyBuilder.RequireScope((IEnumerable<string>)scopes);
        }

        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policyBuilder,
            IEnumerable<string> scopes)
        {
            policyBuilder.Requirements.Add(new ScopesAuthorizationRequirement(scopes));
            return policyBuilder;
        }
    }
}
