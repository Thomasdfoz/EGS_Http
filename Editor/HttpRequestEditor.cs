using EGS.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EGS.Utils.Editor 
{
    public class HttpRequestEditor : EditorWindow
    {
        private List<RequestData> requests;
        private Vector2 scrollPosition;
    
        public class RequestData
        {
            public string url;
            public string type;
            public object data;
            public long responseCode;
            public string message;
            public RequestType requestType;
    
            public RequestData(string type, string url, object data, long responseCode, string message, RequestType requestType)
            {
                this.type = type;
                this.url = url;
                this.data = data;
                this.responseCode = responseCode;
                this.message = message;
                this.requestType = requestType;
            }
        }
    
        [MenuItem("Tools/Requests/HttpRequestEditor")]
        public static void ShowWindow()
        {
            GetWindow<HttpRequestEditor>("HttpRequestEditor");
        }
    
        private void OnEnable()
        {
            requests = new List<RequestData>();
            HttpRequest.OnRequestedOnEditor += HandleRequest;
        }
    
        private void OnDisable()
        {
            HttpRequest.OnRequestedOnEditor -= HandleRequest;
        }
    
        private void HandleRequest(string type, string url, object value, long responseCode, string message, RequestType requestType)
        {
            //receive the request and add it to the list
            requests.Add(new RequestData(type, url, value, responseCode, message, requestType));
        }
    
    
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
    
            // draw a button to clear the list
            if (GUILayout.Button("Clear"))
            {
                requests.Clear();
            }
    
            // draw a list of requests
            foreach (var request in requests)
            {
                EditorGUILayout.LabelField($"{request.requestType} ({request.responseCode}) / step: {request.type}");
                EditorGUILayout.LabelField("Route: " + request.url);
                string json = JsonUtility.ToJson(request.data);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Data: ", json);
                //create copy paste button
                if (GUILayout.Button("Copy"))
                {
                    EditorGUIUtility.systemCopyBuffer = json;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
    
            EditorGUILayout.EndScrollView();
        }
    }
}