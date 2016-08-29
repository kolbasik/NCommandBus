using System;
using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract, Serializable]
    public sealed class GetSubtractedValuesResult
    {
        [DataMember]
        public decimal Result { get; set; }
    }
}