using System;
using System.Data.SqlClient;
using Castle.Core.Logging;

namespace DbAdvance.Host.DbConnectors
{
    public class DatabaseConnectorFactory
    {
        private readonly ILogger _logger;
        private readonly IDatabaseConnectorConfiguration _configuration;
        private readonly DbAdvanceCommandLineOptions _options;

        public DatabaseConnectorFactory(ILogger logger, 
            IDatabaseConnectorConfiguration configuration, 
            DbAdvanceCommandLineOptions options)
        {
            this._logger = logger;
            this._configuration = configuration;
            _options = options;
        }

        public IDatabaseConnector Create()
        {
            if (_options.UseSqlCmdUtility)
            {
                return new SqlCmdDatabaseConnector(new SqlCmdRunner(_logger), _logger, _configuration);
            }

            return new DefaultDatabaseConnector(_logger, _configuration);
        }

        public SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_configuration.ConnectionString);
            connection.Open();
            return connection;
        }

        public IDatabaseConnector UseBasicConnector()
        {
            return new BasicDatabaseConnector(_logger, _configuration);
        }
    }
}