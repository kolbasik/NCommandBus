using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract]
    public class GetAppName
    {
        [DataContract]
        public class Result
        {
            [DataMember]
            public string AppName { get; set; }
        }
    }
}