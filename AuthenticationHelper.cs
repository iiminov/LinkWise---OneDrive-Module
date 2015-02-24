using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Office365.Discovery;
using Microsoft.Office365.SharePoint.CoreServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkWise.Modules.OneDrive
{
    // Provides a valid OutlookServices client that contains the bearer token for issuing requests against Calendar, Mail, and Contact resources.
    internal class AuthenticationHelper
    {
        internal static async Task<SharePointClient> EnsureSharePointClientCreatedAsync(string capabilityName)
        {

            var signInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            AuthenticationContext authContext = new AuthenticationContext(SettingsHelper.Authority, new ADALTokenCache(signInUserId));

            try
            {
                DiscoveryClient discClient = new DiscoveryClient(SettingsHelper.DiscoveryServiceEndpointUri,
                    async () =>
                    {
                        var authResult = await authContext.AcquireTokenSilentAsync(SettingsHelper.DiscoveryServiceResourceId,
                                                                                   new ClientCredential(SettingsHelper.ClientId, SettingsHelper.AppKey),
                                                                                   new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));
                        return authResult.AccessToken;
                    });

                var dcr = await discClient.DiscoverCapabilityAsync(capabilityName);

                return new SharePointClient(dcr.ServiceEndpointUri,
                    async () =>
                    {
                        var authResult = await authContext.AcquireTokenSilentAsync(dcr.ServiceResourceId,
                                                                                   new ClientCredential(SettingsHelper.ClientId, SettingsHelper.AppKey),
                                                                                   new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));

                        return authResult.AccessToken;
                    });
            }
            catch (AdalException exception)
            {
                //Partially handle token acquisition failure here and bubble it up to the controller
                if (exception.ErrorCode == AdalError.FailedToAcquireTokenSilently)
                {
                    authContext.TokenCache.Clear();
                    throw exception;
                }
                return null;
            }
        }
    }
}