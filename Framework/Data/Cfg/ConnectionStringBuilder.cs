using System.Configuration;

namespace Framework.Data.Cfg
{
    public class ConnectionStringBuilder
    {
        private string connectionString;

        public ConnectionStringBuilder FromAppSetting(string appSettingKey)
        {
            this.connectionString = ConfigurationManager.AppSettings[appSettingKey];
            this.IsDirty = true;
            return this;
        }

        public ConnectionStringBuilder FromConnectionStringWithKey(string connectionStringKey)
        {
            this.connectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
            this.IsDirty = true;
            return this;
        }

        public ConnectionStringBuilder Is(string rawConnectionString)
        {
            this.connectionString = rawConnectionString;
            this.IsDirty = true;
            return this;
        }

        protected internal bool IsDirty { get; set; }

        protected internal virtual string Create()
        {
            return this.connectionString;
        }
    }
}