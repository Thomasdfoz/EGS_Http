using System;

namespace EGS.Http
{
    [Serializable]
    public class BaseResponse
    {
        public int code;
        public string message;
        public ErrorData[] errors;

        [Serializable]
        public struct ErrorData
        {
            public string type;
            public string error;
        }
    }
}