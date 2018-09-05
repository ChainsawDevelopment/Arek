using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Arek.Bitbucket
{
    public class BitBucketApiClient
    {
        private readonly Uri _baseUrl;
        private AuthenticationHeaderValue _authenticationHeader = null;

        public BitBucketApiClient(string baseUrl, string base64Auth)
        {
            this._baseUrl = new Uri(baseUrl);

            SetBasicAuthentication(base64Auth);
        }

        public BitBucketApiClient(string baseUrl, string username, string password)
        {
            this._baseUrl = new Uri(baseUrl);

            SetBasicAuthentication(username, password);
        }

        public void SetBasicAuthentication(string base64Auth)
        {
            this._authenticationHeader = new AuthenticationHeaderValue("Basic", base64Auth);
        }

        public void SetBasicAuthentication(string username, string password)
        {
            byte[] userPassBytes = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            string userPassBase64 = Convert.ToBase64String(userPassBytes);

            SetBasicAuthentication(userPassBase64);
        }

        /// <summary>
        /// Creates a new instance of System.Net.Http.HttpClient
        /// </summary>
        /// <remarks>must be disposed by caller</remarks>
        private HttpClient CreateHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = this._baseUrl;

            if (this._authenticationHeader != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = this._authenticationHeader;
            }

            return httpClient;
        }

        public async Task<JObject> GetAsync(string requestUrl)
        {
            using (HttpClient httpClient = CreateHttpClient())
            using (HttpResponseMessage httpResponse = await httpClient.GetAsync("2.0/" + requestUrl).ConfigureAwait(false))
            {
                string json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JObject.Parse(json);
            }
        }
        
        public async Task<JArray> GetManyAsync(string requestUrl)
        {
            using (HttpClient httpClient = CreateHttpClient())
            using (HttpResponseMessage httpResponse = await httpClient.GetAsync("2.0/" + requestUrl).ConfigureAwait(false))
            {
                string json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JArray.Parse(json);
            }
        }
    }
}
