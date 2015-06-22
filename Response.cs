using System;
using System.Runtime.Serialization;

namespace Closedown
{
    [DataContract]
    public class Response
    {
        [DataMember]
        public long code;
        [DataMember]
        public String message;
    }
}
