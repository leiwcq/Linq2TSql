using System;
using System.Data;
using System.Data.Common;

namespace Linq2TSql.SchemaReader.ProviderSchemaReaders
{
    class Db2SchemaReader : SchemaExtendedReader
    {
        public Db2SchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        protected override DataTable Sequences(DbConnection connection)
        {
            DataTable dt = CreateDataTable(SequencesCollectionName);

            const string SQL_COMMAND = @"SELECT seqschema AS SCHEMA, seqname AS SEQUENCE_NAME, increment AS INCREMENTBY, minvalue, maxvalue 
FROM sysibm.syssequences 
WHERE seqschema <> 'SYSIBM' AND seqtype = 'S'";

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                if (da == null) throw new NullReferenceException("数据适配器为空");
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = SQL_COMMAND;

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection connection)
        {
            DataTable dt = CreateDataTable(IdentityColumnsCollectionName);
            const string SQL_COMMAND = @"SELECT tabschema, tabname As TableName, colname As ColumnName
FROM syscat.colidentattributes
WHERE tabname = @tableName or @tableName Is NULL
AND tabschema = @schemaOwner or @schemaOwner Is NULL";

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                if (da == null) throw new NullReferenceException("数据适配器为空");
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = SQL_COMMAND;
                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            const string SQL_COMMAND = @"select tabschema as Owner, 
trigname as Trigger_Name, 
tabname as table_name, 
CASE trigevent 
WHEN 'I' THEN 'INSERT'
WHEN 'D' THEN 'DELETE'
WHEN 'U' THEN 'UPDATE'
END as TRIGGERING_EVENT,
CASE trigtime
WHEN 'A' THEN 'AFTER'
WHEN 'B' THEN 'BEFORE'
WHEN 'I' THEN 'INSTEAD OF'
END as TRIGGER_TYPE,
text as TRIGGER_BODY
from syscat.triggers
where tabschema <> 'SYSTOOLS'
AND valid= 'Y'
AND (tabname = @tableName OR @tableName IS NULL) 
AND (tabschema = @schemaOwner OR @schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, TriggersCollectionName, SQL_COMMAND);
        }

        public override DataTable TableDescription(string tableName)
        {
            const string SQL_COMMAND = @"SELECT 
    TABSCHEMA AS 'SchemaOwner', 
    TABNAME AS 'TableName', 
    REMARKS AS 'TableDescription'
FROM SYSCAT.TABLES
WHERE 
    REMARKS IS NOT NULL AND
    (TABNAME = @tableName OR @tableName IS NULL) AND 
    (TABSCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

            using (DbConnection conn = Factory.CreateConnection())
            {
                if (conn == null) throw new NullReferenceException("连接为空");
                conn.ConnectionString = ConnectionString;
                conn.Open();

                return CommandForTable(tableName, conn, TableDescriptionCollectionName, SQL_COMMAND);
            }
        }

        public override DataTable ColumnDescription(string tableName)
        {
            const string SQL_COMMAND = @"SELECT 
    TABSCHEMA AS 'SchemaOwner', 
    TABNAME AS 'TableName', 
    COLNAME AS 'ColumnName',
    REMARKS AS 'ColumnDescription'
FROM SYSCAT.COLUMNS
WHERE 
    REMARKS IS NOT NULL AND
    (TABNAME = @tableName OR @tableName IS NULL) AND 
    (TABSCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

            using (DbConnection conn = Factory.CreateConnection())
            {
                if (conn == null) throw new NullReferenceException("连接为空");
                conn.ConnectionString = ConnectionString;
                conn.Open();

                return CommandForTable(tableName, conn, ColumnDescriptionCollectionName, SQL_COMMAND);
            }
        }
    }
}