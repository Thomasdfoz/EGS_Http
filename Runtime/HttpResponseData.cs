using System;
using System.Linq;

namespace EGS.Utils 
{
    [Serializable]
    public abstract class HttpResponseData
    { 
        public int code;
        public string message_code;
        public string message;
        public Errors[] errors;
        public abstract string Route { get; }
        public abstract string Query { get; }

        public override string ToString()
        {
            string errorsString = string.Empty;
            if (errors != null)
                errorsString = string.Join(", ", errors.Select(e => e.ToString()));

            return $"Code: {code}, Message Code: {message_code}, Message: {message}, Errors: {errorsString}, Route: {Route}, Query: {Query}";
        }

        [Serializable]
        public struct Errors
        {
            public string type;
            public string error;
        }
    }
}