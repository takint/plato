﻿using System.Runtime.Serialization;
using Newtonsoft.Json;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Plato.StopForumSpam.Client.Models
{
    [DataContract]
    public class SpamFrequencies
    {

        [DataMember(Name = "userName")]
        public SpamFrequency UserName { get; set; } = new SpamFrequency();

        [DataMember(Name = "email")]
        public SpamFrequency Email { get; set; } = new SpamFrequency();

        [DataMember(Name = "ipAddress")]
        public SpamFrequency IpAddress { get; set; } = new SpamFrequency();

        [DataMember(Name = "success")]
        public bool Success { get; set; }

    }

    [DataContract]
    public class SpamFrequency {

        public SpamFrequency()
        {
        }

        public SpamFrequency(int count) : this()
        {
            Count = count;
        }

        [JsonIgnore]
        public bool Appears => Count > 0;

        [DataMember(Name = "count")]
        public int Count { get; set; }

    }
    
}