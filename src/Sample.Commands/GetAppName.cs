using System;
using System.Runtime.Serialization;

namespace Sample.Commands
{
    [DataContract, Serializable]
    public class GetAppName
    {
        [DataContract, Serializable]
        public class Result
        {
            [DataMember]
            public string AppName { get; set; }
        }
    }
}