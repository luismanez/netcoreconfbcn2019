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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sambori.Expenses.API.Data;
using Sambori.Expenses.API.Extensions;

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
                    policy.RequireRole("Approver");

                    //validate token scopes:
                    //https://blogs.msdn.microsoft.com/gianlucb/2017/12/05/azure-ad-scope-based-authorization/
                    //https://joonasw.net/view/azure-ad-authentication-aspnet-core-api-part-2
                    //https://github.com/juunas11/Joonasw.AzureAdApiSample/blob/master/Joonasw.AzureAdApiSample.Api/Extensions/AuthorizationPolicyBuilderExtensions.cs
                    policy.RequireScope("https://inheritscloud.com/Sambori.Expenses.API/Expenses.Approve");


                    //policy.RequireAssertion(context => context.User.IsInRole("Admin") 
                    //                                   || context.User.IsInRole("Manager"));
                });

                options.AddPolicy("AdminsOnly", policy =>
                {
                    policy.RequireRole("Admin");
                    policy.RequireScope("https://inheritscloud.com/Sambori.Expenses.API/Expenses.Manage.All");
                });
            });

            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
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
