using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Linq2TSql.SchemaReader.ProviderSchemaReaders
{
    class SybaseUltraLiteSchemaReader : SchemaExtendedReader
    {
        public SybaseUltraLiteSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        //sybase UltraLite 12 system views: http://dcx.sybase.com/1201/en/uladmin/fo-db-internals.html
        //it's like Sybase Anywhere only more primitive.
        //(table_name = ? OR ? IS NULL) didn't work, and neither did null/dbnull parameters so there's a hacky string concat. Sorry.

        protected override DataTable Columns(string tableName, DbConnection connection)
        {
            //the GetSchema collection doesn't include datatypes.
            //But it seems to be there in the syscolumn table (as "domain")

            const string SQL =
                @"SELECT
t.""table_name"",
c.""column_name"",
c.""default"",
c.""nulls"",
c.""domain"",
c.""domain_info""
FROM syscolumn c, systable t 
WHERE 
c.table_id = t.object_id";

            var columns = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, ColumnsCollectionName, SQL)
                              : SybaseCommandForTable(connection, ColumnsCollectionName, tableName,
                                                      SQL + " AND (t.table_name = ?)");

            //The numbers in syscolumn.domain don't correspond to the ProviderDbType inthe DataTypes collection
            //So we have to create our own mapping.
            var dataTypes = new Dictionary<int, string>
            {
                {1, "SMALLINT"},
                {2, "INT"},
                {3, "NUMERIC"},
                {4, "FLOAT"},
                {5, "DOUBLE"},
                {6, "DATE"},
                {9, "VARCHAR"},
                {10, "LONG VARCHAR"},
                {11, "VARBINARY"},
                {13, "TIMESTAMP"},
                {14, "TIME"},
                {20, "BIGINT"},
                {24, "BIT"},
                {29, "UNIQUEIDENTIFIER"}
            };
            //==REAL
            //and apparently binary too
            //==DATETIME

            columns.Columns.Add("data_type", typeof(string));
            columns.Columns.Add("length", typeof(int));
            columns.Columns.Add("precision", typeof(int));
            foreach (DataRow row in columns.Rows)
            {
                int dataType;
                if (!int.TryParse(row["domain"].ToString(), out dataType)) continue;
                if (!dataTypes.ContainsKey(dataType)) continue;
                var typeName = dataTypes[dataType];
                row["data_type"] = typeName;
                int length;
                if (!int.TryParse(row["domain_info"].ToString(), out length)) continue;
                if (dataType == 9 || dataType == 11) //varchar and varbinary have length
                    row["length"] = length;
                else if (dataType == 3 || dataType == 4 || dataType == 5) //numerics and double have precision
                    row["precision"] = length; //not sure how to get scale?
            }
            return columns;
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            const string SQL =
                @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name,
ic.""sequence"" AS ordinal_position
FROM sysindex i 
JOIN sysixcol ic ON i.object_id = ic.index_id AND ic.table_id = i.table_id
JOIN systable t ON i.table_id = t.object_id
JOIN syscolumn c ON c.table_id = t.object_id AND ic.column_id = c.object_id
WHERE i.type  = 'primary'";

            var data = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, PrimaryKeysCollectionName, SQL)
                              : SybaseCommandForTable(connection, PrimaryKeysCollectionName, tableName,
                                                      SQL + " AND (t.table_name = ?)");
            return data;
        }

        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            const string SQL = @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name,
fkt.table_name AS FK_TABLE,
fki.index_name AS UNIQUE_CONSTRAINT_NAME,
ic.sequence AS ordinal_position
FROM sysindex i
JOIN sysixcol ic 
    ON i.object_id = ic.index_id AND ic.table_id = i.table_id
JOIN systable t 
    ON i.table_id = t.object_id
JOIN syscolumn c 
    ON c.table_id = t.object_id AND ic.column_id = c.object_id
JOIN systable fkt 
    ON fkt.object_id = i.primary_table_id
JOIN sysindex fki
    ON i.primary_index_id = fki.object_id AND i.primary_table_id = fki.table_id
WHERE i.type  = 'foreign'";

            var data = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, ForeignKeysCollectionName, SQL)
                              : SybaseCommandForTable(connection, ForeignKeysCollectionName, tableName,
                                                      SQL + " AND (t.table_name = ?)");
            return data;
        }

        protected override DataTable UniqueKeys(string tableName, DbConnection connection)
        {
            const string SQL = @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name
FROM sysindex i
JOIN sysixcol ic ON i.object_id = ic.index_id AND ic.table_id = i.table_id
JOIN systable t ON i.table_id = t.object_id
JOIN syscolumn c ON c.table_id = t.object_id AND ic.column_id = c.object_id
WHERE i.type  = 'unique'";

            var data = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, UniqueKeysCollectionName, SQL)
                              : SybaseCommandForTable(connection, UniqueKeysCollectionName, tableName,
                                                      SQL + " AND (t.table_name = ?)");
            return data;
        }
        private DataTable SybaseCommandForTable(DbConnection connection, string dataTableName, string sql)
        {
            DataTable dt = CreateDataTable(dataTableName);

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                if (da == null) throw new NullReferenceException("数据适配器为空");
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sql;

                da.Fill(dt);
                return dt;
            }
        }
        private DataTable SybaseCommandForTable(DbConnection connection, string dataTableName, string tableName, string sql)
        {
            DataTable dt = CreateDataTable(dataTableName);

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                if (da == null) throw new NullReferenceException("数据适配器为空");
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sql;

                var parameter = AddDbParameter(string.Empty, tableName);
                da.SelectCommand.Parameters.Add(parameter);

                da.Fill(dt);
                return dt;
            }
        }
    }
}