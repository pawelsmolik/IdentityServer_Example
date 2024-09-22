using Microsoft.AspNetCore.Authentication.Cookies;

namespace IdentityServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // uncomment if you want to add a UI
            builder.Services.AddRazorPages();
            builder.Services.AddIdentityServer()
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddInMemoryApiResources(Config.GetApiResource)
                .AddTestUsers(Config.Users)
                .AddProfileService<CustomProfileService>();
                

            
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
           .AddCookie(options =>
           {
               // add an instance of the patched manager to the options:
               //options.CookieManager = new ChunkingCookieManager();
               options.Cookie.SameSite = SameSiteMode.None;
               options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
               //options.Cookie.Domain = "azurewebsites.net";
           });

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();
            
            // uncomment if you want to add a UI
            app.UseStaticFiles();
            app.UseRouting();

            //
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            /*
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
                Secure = CookieSecurePolicy.Always,
            });
            */

            // uncomment if you want to add a UI
            
            app.MapRazorPages().RequireAuthorization();

            return app;
        }
    }
}
