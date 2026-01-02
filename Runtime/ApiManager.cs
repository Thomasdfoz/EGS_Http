using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EGS.Http
{
    public static class ApiManager
    {
        private static ApiConfig _globalConfig;
        private static string _sessionToken; 

        public static void Initialize(ApiConfig config) => _globalConfig = config;
  
        public static void SetToken(string token) => _sessionToken = token;

        public static void ClearToken() => _sessionToken = null;
      
        private static async Awaitable<TResponse> ExecuteRequest<TResponse, TRequest>(
            string endpoint,
            RequestType type,
            TRequest payload,
            ApiConfig config,
            string tokenOverride = null)
            where TResponse : BaseResponse, new()
            where TRequest : class
        {
            if (config == null)
            {
                Debug.LogError("[ApiManager] Erro: Configuração não encontrada!");
                return new TResponse { code = 0, message = "Missing Config" };
            }

            string url = config.GetFullUrl(endpoint);
            using var request = new UnityWebRequest(url, type.ToString());

            request.timeout = config.timeout;
            request.downloadHandler = new DownloadHandlerBuffer();

            if (payload != null && type != RequestType.GET)
            {
                string json = JsonUtility.ToJson(payload);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.SetRequestHeader("Content-Type", config.contentType);
            request.SetRequestHeader("Accept", config.accept);

            foreach (var h in config.globalHeaders)
                if (!string.IsNullOrEmpty(h.key)) request.SetRequestHeader(h.key, h.value);
            
            string tokenToUse = !string.IsNullOrEmpty(tokenOverride) ? tokenOverride : _sessionToken;
            if (!string.IsNullOrEmpty(tokenToUse))
            {
                string authValue = string.IsNullOrEmpty(config.authPrefix)
                    ? tokenToUse
                    : $"{config.authPrefix} {tokenToUse}";
                request.SetRequestHeader(config.authHeaderKey, authValue);
            }

            try
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[API ERROR] {url} | {request.responseCode} | {request.error}");
                    return new TResponse { code = (int)request.responseCode, message = request.error };
                }

                try
                {
                    TResponse result = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);

                    result.code = (int)request.responseCode;
                    result.message = request.error;

                    return result;
                }
                catch (Exception jsonEx)
                {
                    // Se der erro aqui, o problema é o seu DTO ou o formato do JSON vindo do servidor
                    Debug.LogError($"[JSON MISMATCH] Falha ao converter resposta de {url}.\nErro: {jsonEx.Message}\nJSON Recebido: {request.downloadHandler.text}");
                    return new TResponse { code = 998, message = "JSON Conversion Error" };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[API EXCEPTION] {e.Message}");
                return new TResponse { code = 999, message = e.Message };
            }
        }

        public static async Awaitable<TResponse> Request<TResponse, TRequest>(
            string endpoint, RequestType type, TRequest payload)
            where TResponse : BaseResponse, new() where TRequest : class
        {
            return await ExecuteRequest<TResponse, TRequest>(endpoint, type, payload, _globalConfig);
        }

        public static async Awaitable<TResponse> Request<TResponse, TRequest>(
            string endpoint, RequestType type, TRequest payload, ApiConfig config)
            where TResponse : BaseResponse, new() where TRequest : class
        {
            return await ExecuteRequest<TResponse, TRequest>(endpoint, type, payload, config);
        }

        public static async Awaitable<TResponse> Request<TResponse>(
            string endpoint, RequestType type, string tokenOverride = null, ApiConfig config = null)
            where TResponse : BaseResponse, new()
        {
            var targetConfig = config != null ? config : _globalConfig;
            return await ExecuteRequest<TResponse, object>(endpoint, type, null, targetConfig, tokenOverride);
        }
    }
}