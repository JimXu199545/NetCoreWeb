namespace NetCoreWebAPI
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;

    /// <summary>
    /// Defines the <see cref="Startup" />.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration<see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The ConfigureServices.
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
           
            services.AddSwaggerGen(o =>
            {
                // Setup our document's basic info
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Protected Api",
                    Version = "1.0"
                });

                // Define that the API requires OAuth 2 tokens
                o.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,

                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            Scopes = new Dictionary<string, string>
                            {
                                { "api://872ebcec-c24a-4399-835a-201cdaf7d68b/user_impersonation","allow user to access api"}
                            },
                            AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{Configuration["AzureAD:TenantId"]}/oauth2/v2.0/authorize"),
                            TokenUrl = new Uri($"https://login.microsoftonline.com/{Configuration["AzureAD:TenantId"]}/oauth2/v2.0/token")
                        }
                    }
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement{
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference{
                            Id = "oauth2",
                            Type = ReferenceType.SecurityScheme
                        }
                    },new List<string>()
                    }
                });

            });
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));

            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                options.Authority += "/v2.0";


                options.TokenValidationParameters = new TokenValidationParameters
                {
                    
                    ValidIssuers = new[] {
                      $"https://sts.windows.net/{Configuration["AzureAD:TenantId"]}/",
                      $"https://login.microsoftonline.com/{Configuration["AzureAD:TenantId"]}/v2.0"

                    },
                    RoleClaimType = "roles",
                    // The web API accepts as audiences both the Client ID (options.Audience) and api://{ClientID}.
                    ValidAudiences = new[]
                    {
                           options.Audience,
                           $"api://{options.Audience}"
                    }

                };

            });
            /*services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
          .AddJwtBearer(x =>
          {
              x.Authority = "https://login.microsoftonline.com/e4c9ab4e-bd27-40d5-8459-230ba2a757fb/v2.0";
              x.TokenValidationParameters = new TokenValidationParameters
              {
                  
                  ValidateIssuer = false,
                  //ValidateAudience = false,
                  ValidAudiences = new[] { "872ebcec-c24a-4399-835a-201cdaf7d68b", "api://872ebcec-c24a-4399-835a-201cdaf7d68b" }
              };
          });*/
            services.AddControllers();
            services.AddApplicationInsightsTelemetry();
        }

        /// <summary>
        /// The Configure.
        /// </summary>
        /// <param name="app">The app<see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">The env<see cref="IWebHostEnvironment"/>.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId(Configuration["Swagger:ClientId"]);
                c.OAuthScopeSeparator(" ");
                c.OAuthAppName("Protected Api");

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
