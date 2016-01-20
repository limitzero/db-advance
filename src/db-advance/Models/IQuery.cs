using System.Data.SqlClient;

namespace DbAdvance.Host.Models
{
    public interface IQuery
    {
    }

    public abstract class BaseQuery<T> : IQuery
    {
        public abstract T Execute(SqlConnection connection);
    }
}