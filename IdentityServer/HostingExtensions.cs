using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
            SqlConnection conn = new SqlConnection(
                new SqlConnectionStringBuilder()
                {
                    DataSource = ".",
                    InitialCatalog = "IdentityServer",
                    UserID = "sa",
                    Password = "1qaz@WSX",
                    TrustServerCertificate = true
                }.ConnectionString
            );

            // uncomment if you want to add a UI
            builder.Services.AddRazorPages();
            builder.Services.AddIdentityServer()
                //.AddInMemoryApiScopes(Config.ApiScopes)
                //.AddInMemoryClients(Config.Clients)
                //.AddInMemoryApiResources(Config.GetApiResource)
                .AddTestUsers(Config.Users)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(conn,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(conn,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
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

            InitializeDatabase(app);

            return app;
        }

        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }
                /*
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetApiResource)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                */

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResource)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
