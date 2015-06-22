using System;
using System.Runtime.Serialization;

namespace Closedown
{
    [DataContract]
    public class CorpState
    {
        [DataMember]
        public String corpNum;
        [DataMember]
        public String type;
        [DataMember]
        public String state;
        [DataMember]
        public String stateDate;
        [DataMember]
        public String checkDate;
    }
}
