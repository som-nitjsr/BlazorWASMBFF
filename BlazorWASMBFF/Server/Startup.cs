using BlazorWASMBFF.Server.Data;
using BlazorWASMBFF.Server.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlazorWASMBFF.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddIdentityServer(opt =>
            {
                opt.UserInteraction.LoginUrl = "/Identity/Account/Login";
                opt.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                opt.Events.RaiseErrorEvents = true;
                opt.Events.RaiseInformationEvents = true;
                opt.Events.RaiseFailureEvents = true;
                opt.Events.RaiseSuccessEvents = true;
                opt.Authentication.CookieAuthenticationScheme = IdentityServer4.IdentityServerConstants.DefaultCookieAuthenticationScheme;
                //Disable various info
                opt.Discovery.ShowApiScopes = false;
                opt.Discovery.ShowClaims = false;
                opt.Discovery.ShowExtensionGrantTypes = false;
                opt.Discovery.ShowGrantTypes = false;
                opt.Discovery.ShowIdentityScopes = false;
                opt.Discovery.ShowResponseModes = false;
                opt.Discovery.ShowResponseTypes = false;
                opt.Discovery.ShowTokenEndpointAuthenticationMethods = false;
                opt.Endpoints.EnableDeviceAuthorizationEndpoint = false;

            })

               .AddInMemoryIdentityResources(Config.GetIdentityResources())
             .AddInMemoryApiScopes(Config.GetApiResources())
             .AddInMemoryClients(

                    Config.GetClients()
           )
            .AddDeveloperSigningCredential();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "mytestapp";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                
                    options.Events.OnSigningOut = async e =>
                    {
                        await e.HttpContext.RevokeUserRefreshTokenAsync();
                    };
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = "https://localhost:44346/";
                    options.SignedOutRedirectUri = "/";
                  
                    options.ClientId = "testclient";
                    options.ClientSecret = "Secret";
                    options.ResponseType = OpenIdConnectResponseType.Code;
                  
                    options.Scope.Add("core");
                    options.Scope.Add("offline_access");
                    options.SaveTokens = true;
                    options.UsePkce = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                    };
                });
          
         
            services.AddAccessTokenManagement();
        


            services.AddControllersWithViews();
            services.AddRazorPages();

            var proxyBuilder = services.AddReverseProxy();
            // Initialize the reverse proxy from the "ReverseProxy" section of configuration
            proxyBuilder.LoadFromConfig(Configuration.GetSection("ReverseProxy"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");

                endpoints.MapReverseProxy(proxyPipeline =>
                {
                    proxyPipeline.Use(async (context, next) =>
                    {
                        var token = await context.GetClientAccessTokenAsync();
                        context.Request.Headers.Add("Authorization", $"Bearer {token}");

                        await next().ConfigureAwait(false);
                    });
                });
            });
        }
    }
}
