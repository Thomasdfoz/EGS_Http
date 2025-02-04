using System.Collections.Generic;


namespace EGS.Utils 
{
    public class HttpRequestData
    {
        public class Form
        {
            public string fieldName = string.Empty;
            public string fileName = string.Empty;
            public byte[] content = new byte[0];
            public string value = string.Empty;
            public string mimeType = string.Empty;
        }

        public virtual List<Form> Forms { get; protected set; } = new List<Form>();
    }
}
