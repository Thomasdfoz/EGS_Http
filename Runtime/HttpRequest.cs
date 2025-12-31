using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System;
using EGS.Http;

namespace EGS.Utils
{
  
    public static class HttpRequest
    {
# if UNITY_EDITOR
        //ONLY FOR DEBUGGING, DO NOT USE IN PRODUCTION
        [Obsolete]
        public static event System.Action<string, string, object, long, string, RequestType> OnRequestedOnEditor; //"request" or "payload", url, value, responseCode, message, requestType
#endif

        public class HttpOptions
        {
            public string Session_Id { get; set; }
            public string Event_Id { get; set; }
            public string Language { get; set; }
            public string Token { get; set; }
            public string UserID { get; set; }
            public string Url { get; set; }
            public bool IsMobile { get; set; }

            public HttpOptions(string language, string token)
            {
                Language = language;
                Token = token;
            }
        }

        public delegate void HttpRequestReturn<T>(T requestObject, long errorCode, string messageCode) where T : new();

        public static IEnumerator Send<TResponse>(HttpOptions options, HttpRequestReturn<TResponse> callback, RequestType requestType, string query = null, bool sslVerification = false) where TResponse : HttpResponseData, new()
        {
            yield return Send(options, null, null, callback, requestType, query, sslVerification);
        }

        public static IEnumerator Send<TResponse>(HttpOptions options, HttpRequestData requestData, HttpRequestReturn<TResponse> callback, RequestType requestType, string query = null, bool sslVerification = false) where TResponse : HttpResponseData, new()
        {
            string lJson = JsonUtility.ToJson(requestData);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(lJson);
            yield return Send(options, bodyRaw, requestData, callback, requestType, query, sslVerification);
        }

        public static IEnumerator Send<TResponse>(HttpOptions options, byte[] bodyRaw, HttpRequestReturn<TResponse> callback, RequestType requestType, string query = null, bool sslVerification = false) where TResponse : HttpResponseData, new()
        {
            yield return Send(options, bodyRaw, null, callback, requestType, query, sslVerification);
        }


        private static IEnumerator Send<TResponse>(HttpOptions options, byte[] bodyRaw, HttpRequestData requestData, HttpRequestReturn<TResponse> callback, RequestType requestType, string query = null, bool sslVerification = false) where TResponse : HttpResponseData, new()
        {
            TResponse lDeserializedData = new TResponse();

            var lUrl = !string.IsNullOrEmpty(query) ? $"{options.Url}{lDeserializedData.Route}?{query}" : $"{options.Url}{lDeserializedData.Route}";

            UnityWebRequest lRequest = new UnityWebRequest(lUrl, requestType.ToString(), new DownloadHandlerBuffer(), null);

            if (sslVerification)
            {
                ServicePointManager.ServerCertificateValidationCallback += OnCertificateValidation;
                lRequest.certificateHandler = new CustomCertificateHandler();
            }

            lRequest.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            lRequest.SetRequestHeader("Accept", "application/json");

            if (RequestType.POST == requestType || RequestType.PUT == requestType)
            {
                lRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

                if (requestData != null)
                {
                    if (requestData.Forms != null)
                    {
                        if (requestData.Forms.Count > 0)
                        {
                            WWWForm formData = new WWWForm();
                            foreach (var f in requestData.Forms)
                            {
                                if (f.content != null && f.content.Length > 0)
                                    formData.AddBinaryData(f.fieldName, f.content, f.fileName, f.mimeType);
                                else
                                    formData.AddField(f.fieldName, f.value);
                            }

                            lRequest = UnityWebRequest.Post(lUrl, formData);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(options.Token))
                lRequest.SetRequestHeader("Authorization", "Bearer " + options.Token);

            if (!string.IsNullOrEmpty(options.Language))
                lRequest.SetRequestHeader("language", options.Language);

            lRequest.SendWebRequest();

#if UNITY_EDITOR
            OnRequestedOnEditor?.Invoke("requesting", lUrl, requestData, lRequest.responseCode, lRequest.error, requestType);
#endif

            while (!lRequest.isDone)
            {
                yield return null;
            }

#if UNITY_EDITOR
            OnRequestedOnEditor?.Invoke("payload", lUrl, lDeserializedData, lRequest.responseCode, lRequest.error, requestType);
#endif

            if (lRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log($"HTTP :{lRequest.error} Link:{options.Url}{lDeserializedData.Route}");

                callback?.Invoke(null, 0, "connection_falied");
                yield break;
            }

            if (lRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"HTTP{lRequest.error} Link:{options.Url}{lDeserializedData.Route}");

                if (lRequest.responseCode.ToString()[0] == '5')
                {
                    callback?.Invoke(null, lRequest.responseCode, "internal_server_error");
                    yield break;
                }

            }

            Debug.Log($"GetValue from link {lUrl} \n Data:\n {lRequest.downloadHandler.text}");

            JsonUtility.FromJsonOverwrite(lRequest.downloadHandler.text, lDeserializedData);

            callback?.Invoke(lDeserializedData, lRequest.responseCode, string.Empty);

            lRequest.Dispose();
        }

        private static bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Ignorar todos os erros de validação do certificado
            return true;
        }
    }

    public class CustomCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}