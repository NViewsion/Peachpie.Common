using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

namespace Pchpie.Common.Network
{
    public class NetHttpClient
    {
        private readonly HttpClient _httpClient;

        private int StatusCode = int.MinValue;

        private string CharSet = string.Empty; // eg: ISO-8859-1

        public NetHttpClient(string uri, string username = "", string password = "", byte authMode = 0, string accept = "")
        {
            HttpClientHandler handler = new();

            if (!authMode.Equals(0) && username.Length > 0 && password.Length >= 0)
            {
                CredentialCache credentials = new()
                {
                    { new Uri(new Uri(uri).GetLeftPart(UriPartial.Authority)), (authMode == 1 ? "Basic": "Digest"), new NetworkCredential(username, password) }
                };

                handler.Credentials = credentials;
                handler.PreAuthenticate = true;
            }

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(uri),
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (accept.Length > 0)
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            }
        }

        public int GetStatusCode() => StatusCode;

        public void SetCharSet(string charSet)
        {
            CharSet = charSet;
        }

        public string Get(string urn)
        {
            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, urn);
                HttpResponseMessage response = _httpClient.Send(request);

                StatusCode = (int)response.StatusCode;

                if (string.IsNullOrEmpty(CharSet))
                {
                    using StreamReader reader = new(response.Content.ReadAsStream());

                    return reader.ReadToEnd();
                }
                else
                {
                    byte[] data = response.Content.ReadAsByteArrayAsync().Result;
                    return Encoding.GetEncoding(CharSet).GetString(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                StatusCode = 0;
                return e.Message;
            }
        }
    }
}