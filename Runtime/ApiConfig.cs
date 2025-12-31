using System;
using System.Collections.Generic;
using UnityEngine;

namespace EGS.Http
{
    [CreateAssetMenu(fileName = "ApiConfig", menuName = "EGS/Http/ApiConfig")]
    public class ApiConfig : ScriptableObject
    {
        [Header("Network Settings")]
        [Tooltip("Ex: https://api.meujogo.com/v1")]
        public string baseUrl;

        [Range(5, 60)]
        public int timeout = 20;

        [Header("Default Headers")]
        public string contentType = "application/json";
        public string accept = "application/json";
        public string userAgent = "UnityGame/1.0";

        [Header("Security")]
        public string authPrefix = "Bearer";
        public string authHeaderKey = "Authorization";

        [Header("Global Custom Headers")]
        [Tooltip("Headers que serão enviados em TODAS as requisições (ex: X-API-KEY)")]
        public List<HeaderItem> globalHeaders = new List<HeaderItem>();

        [Header("Debug Settings")]
        public bool enableLogging = true;

        [Serializable]
        public struct HeaderItem
        {
            public string key;
            public string value;
        }

        public string GetFullUrl(string endpoint)
        {
            string baseClean = baseUrl.TrimEnd('/');
            string endpointClean = endpoint.TrimStart('/');
            return $"{baseClean}/{endpointClean}";
        }
    }
}