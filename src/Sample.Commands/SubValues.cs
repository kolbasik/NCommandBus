using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract]
    public class SubValues
    {
        [DataMember]
        public decimal A { get; set; }
        [DataMember]
        public decimal B { get; set; }
    }
}