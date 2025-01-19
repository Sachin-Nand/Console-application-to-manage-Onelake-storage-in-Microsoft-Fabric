using System.Net.Http.Headers;
using Microsoft.Identity.Client;

namespace Http
{
    internal class HttpMethods : Security.Authentication
    {
        public async static Task<string> GetAsync(string url)
        {

            AuthenticationResult result = await ReturnAuthenticationResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.GetAsync(url);
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                return null;
            }

        }

        public async static Task<byte[]> SendAsync(HttpRequestMessage httprequestMessage)
        {
            AuthenticationResult result = await ReturnAuthenticationResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(httprequestMessage);
            response.EnsureSuccessStatusCode();
            try
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                Console.WriteLine(response.Content.ReadAsByteArrayAsync().Result);
                return null;
            }
        }

        public async static Task<string> DeleteAsync(string url)
        {
            AuthenticationResult result = await ReturnAuthenticationResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.DeleteAsync(url);
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                return null;
            }

        }

        public async static Task<string> PutAsync(string url, HttpContent content)
        {
            AuthenticationResult result = await ReturnAuthenticationResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.PutAsync(url, content);
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                return null;
            }

        }

        public async static Task<string> PostAsync(string url, HttpContent content)
        {

            AuthenticationResult result = await ReturnAuthenticationResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.PostAsync(url, content);
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                return null;
            }

        }
    }
}
