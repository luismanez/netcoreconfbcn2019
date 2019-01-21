using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sambori.Expenses.Management.Extensions;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect();
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;
            private readonly ITokenAcquisition _tokenAcquisition;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions, ITokenAcquisition tokenAcquisition)
            {
                _azureOptions = azureOptions.Value;
                _tokenAcquisition = tokenAcquisition;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _azureOptions.ClientId;
                options.Authority = _azureOptions.Authority;   // V2 specific
                options.UseTokenLifetime = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateIssuer = false;     // accept several tenants
                options.Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceived
                };

                options.Scope.Add("https://inheritscloud.com/Sambori.Expenses.API/Expenses.Manage.All");
                options.Scope.Add("https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read.All");
                options.Scope.Add("https://inheritscloud.com/Sambori.Expenses.API/Expenses.Approve");
                options.Scope.Add("https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read");

                options.ResponseType = "code id_token";
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            {
                await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, 
                    new[]
                    {
                        "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Manage.All",
                        "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read.All",
                        "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Approve",
                        "https://inheritscloud.com/Sambori.Expenses.API/Expenses.Read"
                    });
            }
        }
    }
}
