using LinkWise.Modules.OneDrive;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartup(typeof(Startup))]

namespace LinkWise.Modules.OneDrive
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = SettingsHelper.ClientId,
                Authority = SettingsHelper.Authority,

                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.

                    AuthorizationCodeReceived = (context) =>
                    {
                        var code = context.Code;

                        ClientCredential credential = new ClientCredential(SettingsHelper.ClientId, SettingsHelper.AppKey);
                        String UserObjectId = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                        AuthenticationContext authContext = new AuthenticationContext(SettingsHelper.Authority, new ADALTokenCache(UserObjectId));

                        AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, SettingsHelper.AADGraphResourceId);

                        return Task.FromResult(0);
                    },


                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;

                        return Task.FromResult(0);
                    },

                    AuthenticationFailed = (context) =>
                    {
                        // Suppress the exception if you don't want to see the error
                        context.HandleResponse();

                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}