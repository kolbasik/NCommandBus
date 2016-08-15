using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract]
    public class SubValuesResult
    {
        [DataMember]
        public decimal Result { get; set; }
    }
}