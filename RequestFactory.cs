using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ApiClient
{
    public static class RequestFactory
    {
        public static HttpWebRequest GetRequest(string uri, Method method, string userAgent, string contentType, IEnumerable<Parameter> parameters, IEnumerable<Header> headers, string content, string consumerKey, string consumerSecret, string accessKey, string accessSecret, string verifier)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("uri", "Uri can't be Null");
            }

            //Only add querystring for GET requests
            string fullUri = uri;

            if (method == Method.Get)
            {
                fullUri = BuildUriQueryString(uri, parameters);
            }

            HttpWebRequest request = HttpWebRequest.Create(fullUri) as HttpWebRequest;

            request.AllowReadStreamBuffering = false;
            request.Method = GetMethodString(method);

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }

            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }

            if (headers != null && headers.Count() > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            if (!string.IsNullOrEmpty(consumerKey) && !string.IsNullOrEmpty(accessKey))
            {
                AddOAuthHeader(request.Headers, uri, method, new Token(consumerKey, consumerSecret), new Token(accessKey, accessSecret), verifier);
            }

            //Only add content for POST, PUT or DELETE requests
            if (method != Method.Get && !string.IsNullOrEmpty(content))
            {
                var contentRequest = new ContentRequest()
                {
                    WebRequest = request,
                    Content = content
                };

                contentRequest.WebRequest.BeginGetRequestStream(GetRequestStreamCallback, contentRequest);

                contentRequest.AllDone.WaitOne();
            }

            return request;
        }

        private static void AddOAuthHeader(WebHeaderCollection headers, string uri, Method method, Token consumerToken, Token accessToken, string verifier)
        {
            List<Parameter> parameters = new List<Parameter>();

            parameters.Add(new Parameter("oauth_consumer_key", consumerToken.Key));
            parameters.Add(new Parameter("oauth_nonce", new Random().Next().ToString()));
            parameters.Add(new Parameter("oauth_timestamp", DateTime.UtcNow.ToUnixTime().ToString()));
            parameters.Add(new Parameter("oauth_signature_method", "HMAC-SHA1"));
            parameters.Add(new Parameter("oauth_version", "1.0"));

            if (accessToken != null)
            {
                parameters.Add(new Parameter("oauth_token", accessToken.Key));
            }

            if (verifier != null)
            {
                parameters.Add(new Parameter("oauth_verifier", verifier));
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("OAuth ");

            foreach (var parameter in parameters)
            {
                sb.Append(string.Format("{0}=\"{1}\",", parameter.Key, parameter.Value));
            }

            sb.Append(string.Format("oauth_signature=\"{0}\"", GenerateSignature(uri, method, consumerToken.Secret, accessToken, parameters)));

            headers[HttpRequestHeader.Authorization.ToString()] = sb.ToString();
        }

        private static string GetMethodString(Method? method)
        {
            switch (method)
            {
                case Method.Get:
                    return "GET";
                case Method.Post:
                    return "POST";
                case Method.Put:
                    return "PUT";
                case Method.Delete:
                    return "DELETE";
                default:
                    return "GET";
            }
        }

        private static string BuildUriQueryString(string uri, IEnumerable<Parameter> parameters)
        {
            if (parameters != null && parameters.Count() > 0)
            {
                uri = string.Format("{0}?{1}", uri, string.Join("&", parameters));
            }

            return uri;
        }

        private static void GetRequestStreamCallback(IAsyncResult result)
        {
            var request = result.AsyncState as ContentRequest;

            try
            {
                using (Stream stream = request.WebRequest.EndGetRequestStream(result))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(request.Content);
                    stream.Write(byteArray, 0, request.Content.Length);
                }
            }
            catch (Exception)
            {}
            finally
            {
                request.AllDone.Set();
            }
        }

        private static string GenerateSignature(string uri, Method method, string consumerSecret, Token token, IEnumerable<Parameter> parameters)
        {
            var hmacKeyBase = consumerSecret.UrlEncode() + "&" + ((token == null) ? "" : token.Secret).UrlEncode();
            using (var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(hmacKeyBase)))
            {
                var orderedParameters = parameters.OrderBy(p => p.Key).ThenBy(p => p.Value);

                StringBuilder stringParameter = new StringBuilder();
                foreach (var parameter in orderedParameters)
                {
                    stringParameter.Append(string.Format("{0}&", parameter.ToString()));
                }

                stringParameter.Remove(stringParameter.Length - 1, 1);

                var signatureBase = string.Format("{0}&{1}&{2}",
                    GetMethodString(method),
                    new Uri(uri).GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped).UrlEncode(),
                    stringParameter.ToString().UrlEncode());

                var hash = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBase));

                return Convert.ToBase64String(hash).UrlEncode();
            }
        }
    }
}
