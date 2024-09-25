using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("WebApiExampleApp", "Test Api", userClaims: new[] { "Role", "name" }),
                new ApiScope(IdentityServerConstants.StandardScopes.OpenId),
                new ApiScope(IdentityServerConstants.StandardScopes.Profile),
                new ApiScope(IdentityServerConstants.StandardScopes.Email)
            };

        public static IEnumerable<ApiResource> GetApiResource => new ApiResource[] {

            new ApiResource("WebApiExample"){ 
                Scopes = new List<string> {
                    "WebApiExampleApp"
                },
                ApiSecrets = { 
                    new Secret{ 
                        Value = "ABC123".Sha256(),
                        Type = "SharedSecret"
                    }
                }
            },
            new ApiResource("ReverseProxy"){
                Scopes = new List<string> {
                    "WebApiExampleApp"
                },
                ApiSecrets = {
                    new Secret{
                        Value = "ABC123".Sha256(),
                        Type = "SharedSecret"
                    }
                }
            }
        };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "AngularExampleAppFront",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 60,
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "WebApiExampleApp",
                    },
                    AllowedCorsOrigins = new List<string>(){ 
                        "http://localhost:4200"
                    },
                    RedirectUris = new List<string>{
                        "http://localhost:4200"
                    },
                    PostLogoutRedirectUris = new List<string>{
                        "http://localhost:4200"
                    },
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    Claims = new List<ClientClaim>(){},
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                }
            };

        public static List<TestUser> Users =>
            new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "bob",
                    Password = "bob",
                    Claims = new List<Claim>(){ 
                    },
                    
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "pawel",
                    Password = "pawel",
                    Claims = new List<Claim>(){ 
                        new Claim("Role", "Admin")
                    }
                }
            };
    }

    public sealed class CustomProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            if (context.RequestedClaimTypes.Any())
            {
                context.AddRequestedClaims(new[] { 
                    new Claim("name", context.Subject.Identity?.Name ?? string.Empty)
                });

                var user = Config.Users.First(a => a.SubjectId == context.Subject.GetSubjectId());
                context.IssuedClaims.AddRange(user.Claims);
            }

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            if (context.Subject.GetSubjectId() == "0")
            {
                context.IsActive = false;
            }

            return Task.CompletedTask;
        }
    }
}
