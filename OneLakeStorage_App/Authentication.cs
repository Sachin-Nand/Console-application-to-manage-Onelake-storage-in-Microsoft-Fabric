using Microsoft.Identity.Client;

namespace Security
{
    internal class Authentication
    {
        public static bool istokencached = false;
        public static string clientId = "";
        private static string[] scopes = new string[] { "https://storage.azure.com/.default" };
        private static string Authority = "https://login.microsoftonline.com/organizations";
        private static string RedirectURI = "http://localhost";
        public static readonly HttpClient client = new HttpClient();
        protected HttpClient Client => client;
        public async static Task<AuthenticationResult> ReturnAuthenticationResult()
        {
            string AccessToken;
            PublicClientApplicationBuilder PublicClientAppBuilder =
                PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(Authority)
                .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                .WithRedirectUri(RedirectURI);

            IPublicClientApplication PublicClientApplication = PublicClientAppBuilder.Build();
            var accounts = await PublicClientApplication.GetAccountsAsync();
            AuthenticationResult result;
            try
            {

                result = await PublicClientApplication.AcquireTokenSilent(scopes, accounts.First())
                                 .ExecuteAsync()
                                 .ConfigureAwait(false);

            }
            catch
            {
                result = await PublicClientApplication.AcquireTokenInteractive(scopes)
                                 .ExecuteAsync()
                                 .ConfigureAwait(false);

            }
            istokencached = true;
            return result;

        }
    }
}
