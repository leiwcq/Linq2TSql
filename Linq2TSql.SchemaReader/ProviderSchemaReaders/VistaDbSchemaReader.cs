using System;
using System.Data;
using System.Data.Common;

namespace Linq2TSql.SchemaReader.ProviderSchemaReaders
{
    /// <summary>
    /// Support for VistaDB database
    /// </summary>
    class VistaDbSchemaReader : SchemaExtendedReader
    {
        public VistaDbSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection connection)
        {
            //thanks to dnparsons
            const string SQL_COMMAND = @"SELECT
            table_schema AS SchemaOwner,
            table_name AS TableName,
            column_name AS ColumnName,
            1 AS IdentitySeed,
            1 AS IdentityIncrement
            from VistaDBColumnSchema() where is_identity = true";

            return CommandForTable(tableName, connection, IdentityColumnsCollectionName, SQL_COMMAND);
        }

        public override DataTable ViewColumns(string viewName)
        {
            using (DbConnection conn = Factory.CreateConnection())
            {
                if (conn == null) throw new NullReferenceException("连接为空");
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var collectionName = ViewColumnsCollectionName;
                return GenericCollection(collectionName, conn, viewName);
            }
        }

        protected override DataTable StoredProcedures(DbConnection connection)
        {
            //it's not reported in the MetaDataCollections, but it is there.
            var collectionName = ProceduresCollectionName;
            return connection.GetSchema(collectionName, new string[] { });
        }

        protected override DataTable StoredProcedureArguments(string storedProcedureName, DbConnection connection)
        {
            //it's not reported in the MetaDataCollections, but it is there.
            var dt = connection.GetSchema("PROCEDUREPARAMETERS", new[] { null, null, storedProcedureName, null });
            dt.TableName = ProcedureParametersCollectionName;
            return dt;
        }

    }
}