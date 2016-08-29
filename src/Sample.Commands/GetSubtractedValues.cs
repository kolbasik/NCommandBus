using System;
using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract, Serializable]
    public sealed class GetSubtractedValues
    {
        [DataMember]
        public decimal A { get; set; }
        [DataMember]
        public decimal B { get; set; }
    }
}