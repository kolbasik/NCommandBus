using System;
using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract, Serializable]
    public class AddValues
    {
        [DataMember]
        public decimal A { get; set; }
        [DataMember]
        public decimal B { get; set; }
    }
}