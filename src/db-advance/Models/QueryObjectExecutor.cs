using System.Data.SqlClient;
using DbAdvance.Host.DbConnectors;

namespace DbAdvance.Host.Models
{
    public class QueryObjectExecutor
    {
        private readonly IDatabaseConnectorConfiguration _configuration;

        public QueryObjectExecutor(IDatabaseConnectorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T Execute<T>(BaseQuery<T> query)
            where T : class
        {
            var result = default(T);

            using (var connection = GetConnection())
            {
                try
                {
                    result = query.Execute(connection);
                }
                catch
                {
                    throw;
                }

                return result;
            }
        }

        protected SqlConnection GetConnection()
        {
            var connection = new SqlConnectionStringBuilder(_configuration.ConnectionString);
            var cxn = new SqlConnection(connection.ToString());
            cxn.Open();
            return cxn;
        }
    }
}