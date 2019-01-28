using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sambori.Expenses.API.Data;
using Sambori.Expenses.API.Extensions;
using Sambori.Expenses.API.Http;

namespace Sambori.Expenses.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ExpensesContext>(opt => opt.UseInMemoryDatabase("ExpensesDb"));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApproversOnly", policy =>
                {
                    //validate token scopes:
                    //https://github.com/juunas11/Joonasw.AzureAdApiSample/blob/master/Joonasw.AzureAdApiSample.Api/Extensions/AuthorizationPolicyBuilderExtensions.cs
                    policy.RequireScope("Expenses.Approve");

                    policy.RequireAssertion(context => context.User.IsInRole("Admin")
                                                       || context.User.IsInRole("Approver"));
                });

                options.AddPolicy("AdminsOnly", policy =>
                {
                    policy.RequireRole("Admin");
                    policy.RequireScope("Expenses.Manage.All");
                });
            });

            services.AddHttpContextAccessor();

            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));

            services
                .AddTokenAcquisition()
                .AddDistributedMemoryCache()
                .AddSession()
                .AddSessionBasedTokenCache();

            services.AddHttpClient<MsGraphApiHttpClient>();

            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // This is an Azure AD v2.0 Web API
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api URI
                options.TokenValidationParameters.ValidAudiences = 
                    new string[] { options.Audience, "https://inheritscloud.com/Sambori.Expenses.API" };

                //// Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                //// we inject our own multitenant validation logic (which even accepts both V1 and V2 tokens)
                //options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.ValidateAadIssuer;

                // When an access token for our own Web API is validated, we add it to MSAL.NET's cache so that it can
                // be used from the controllers.
                options.Events = new JwtBearerEvents();

                //// If you want to debug, or just understand the JwtBearer events, uncomment the following line of code
                //// options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);

                options.Events.OnTokenValidated = async context =>
                {
                    var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                    //var scopes = new[] { "Mail.Send" };
                    var scopes = new[] { "https://graph.microsoft.com/.default" };

                    context.Success();

                    // Adds the token to the cache, and also handles the incremental consent and claim challenges
                    tokenAcquisition.AddAccountToCacheFromJwt(context, scopes);
                };
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();

            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is disabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                 Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
