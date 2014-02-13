namespace NServiceBus.Features
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ConnectionStringBuilder : DbConnectionStringBuilder
    {
        private ConnectionConfiguration connectionConfiguration;

        public ConnectionConfiguration Parse(string connectionString)
        {
            ConnectionString = connectionString;

            connectionConfiguration = new ConnectionConfiguration();

            foreach (var pair in
                from property in typeof(ConnectionConfiguration).GetProperties()
                 let match = Regex.Match(connectionString, string.Format("[^\\w]*{0}=(?<{0}>[^;]+)", property.Name), RegexOptions.IgnoreCase)
                 where match.Success
                 select new
                 {
                     Property = property,
                     match.Groups[property.Name].Value
                 })
                pair.Property.SetValue(
                    connectionConfiguration,
                    TypeDescriptor.GetConverter(pair.Property.PropertyType).ConvertFromString(pair.Value),
                    null);

            return connectionConfiguration;
        }
    }
}