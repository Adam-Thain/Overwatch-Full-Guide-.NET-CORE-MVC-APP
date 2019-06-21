using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace OW_Full_Guide_NetCore_MVC.Web.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            //
            IoCContainer.Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add ApplicationDbContext to DI
            // NOTE: The default connection is found in appsettings.json
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(IoCContainer.Configuration.GetConnectionString("DefaultConnection")));

            // AddIdentity adds cookie based authentication
            // Adds scoped Classes for things like UserManager, SigninManager, PasswordHashers etc..
            // NOTE: Automatically adds the validated user from a cookie to the HTTPContext.User
            // https://github.com/aspnet/Identity/blob/85f8a49aef68bf9763cd9854ce1dd4a26a7c5d3c/src/Identity/IdentityServiceCollectionExtensions.cs
            services.AddIdentity<ApplicationUser, IdentityRole>()

            // Adds UserStore and RoleStore from this context
            // That are consumed by the UserManager and RoleManager
            // https://github.com/aspnet/Identity/blob/dev/src/EF/IdentityEntityFrameworkBuilderExtensions.cs
            .AddEntityFrameworkStores<ApplicationDbContext>()

            // Adds a provider that generates unique keys and hashes for things like
            // forget password links, phone number verifactions codes etc...
            .AddDefaultTokenProviders();

            // Add JWT Authentication for api clients
            services.AddAuthentication().
                AddJwtBearer(options => 
                {
                    // Set Validation Parameters
                    options.TokenValidationParameters = new TokenValidationParameters
                    {

                        ValidateIssuer = true,

                        ValidateAudience = true,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,

                        ValidIssuer = IoCContainer.Configuration["Jwt:Issuer"],

                        ValidAudience = IoCContainer.Configuration["Jwt:Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IoCContainer.Configuration["Jwt:SecretKey"])),
                    };
                });

            // Change password policy
            services.Configure<IdentityOptions>(options =>
            {
                // Make really weak passwords possible
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            // After application cookie info
            services.ConfigureApplicationCookie(options =>
            {
                // Redirect to /login
                options.LoginPath = "/login";

                // Change cookie timeout to expire after 15 seconds
                options.ExpireTimeSpan = TimeSpan.FromSeconds(15);
            });

            // User MVC style website
            services.AddMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Store instance of the DI service provider so our application can access it anywhere
            IoCContainer.Provider = (ServiceProvider)app.ApplicationServices;

            // Setup Identity
            app.UseAuthentication();

            // If in Development
            // Elese Just show Generic Error Code
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            // Serve Static Files
            app.UseStaticFiles();

            // Setup MVC Routes
            app.UseMvc(routes =>
            {
                // Default route of /controller/action/info
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{moreInfo?}");

                routes.MapRoute(
                    name: "aboutPage",
                    template: "more",
                    defaults: new { controller = "About", action = "TellMeMore" });
            });
        }
    }
}