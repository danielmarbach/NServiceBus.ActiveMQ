namespace NServiceBus.Features
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class ConnectionConfiguration
    {
        private const string DefaultServerUrl = "activemq:tcp://localhost:61616";

        public ConnectionConfiguration()
        {
            this.ServerUrl = DefaultServerUrl;
            this.RessourceManagerId = DefaultResourceManagerId;
        }

        public string ServerUrl { get; set; }

        public Guid RessourceManagerId { get; set; }

        private static Guid DefaultResourceManagerId
        {
            get
            {
                var resourceManagerId = "ActiveMQ" + Address.Local + "-" + Configure.DefineEndpointVersionRetriever();
                return DeterministicGuidBuilder(resourceManagerId);
            }
        }

        private static Guid DeterministicGuidBuilder(string input)
        {
            //use MD5 hash to get a 16-byte hash of the string
            using (var provider = new MD5CryptoServiceProvider())
            {
                var inputBytes = Encoding.Default.GetBytes(input);
                var hashBytes = provider.ComputeHash(inputBytes);
                //generate a guid from the hash:
                return new Guid(hashBytes);
            }
        }
    }
}