using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace NetCoreAzureAD
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static string token = null;
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                  .AddAzureAD(options => Configuration.Bind("AzureAd", options));
            services.Configure<AzureADOptions>(options => Configuration.Bind("AzureAd", options));
            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {

                options.Authority = options.Authority + "/v2.0/";
                options.Scope.Add("https://database.windows.net//.default"); // get token fro azure sql
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                options.Events.OnAuthorizationCodeReceived = async context =>
                {
                    var request = context.HttpContext.Request;
                    string currentUri = UriHelper.BuildAbsolute(
                       request.Scheme,
                       request.Host,
                       request.PathBase,
                       options.CallbackPath);

                    var code = context.ProtocolMessage.Code;
                    string signedInUserID = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                    IConfidentialClientApplication cca = ConfidentialClientApplicationBuilder
                              .Create(options.ClientId)
                              .WithClientSecret(options.ClientSecret)
                              .WithRedirectUri(currentUri)
                              .WithAuthority(options.Authority)
                              .Build();
                    new MSALStaticCache(signedInUserID, context.HttpContext).EnablePersistence(cca.UserTokenCache);
                    AuthenticationResult result = await cca.AcquireTokenByAuthorizationCode(options.Scope, code)
                        .ExecuteAsync();

                    context.HandleCodeRedemption(result.AccessToken, result.IdToken);

                };

            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
