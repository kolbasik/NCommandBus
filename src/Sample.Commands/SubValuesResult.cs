using System;
using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract, Serializable]
    public class SubValuesResult
    {
        [DataMember]
        public decimal Result { get; set; }
    }
}