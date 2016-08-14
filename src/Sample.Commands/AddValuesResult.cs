using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract]
    public class AddValuesResult
    {
        [DataMember]
        public decimal Result { get; set; }
    }
}
