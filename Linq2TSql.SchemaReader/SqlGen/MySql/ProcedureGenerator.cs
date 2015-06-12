using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.SqlGen.MySql
{
    /// <summary>
    /// Generate MySql stored procedures.
    /// </summary>
    internal class ProcedureGenerator : ProcedureGeneratorBase
    {
        public ProcedureGenerator(DatabaseTable table) : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.MySql)
            {
                InStoredProcedure = true, 
                FormatParameter = x => "p_" + x
            };
        }
        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            return new ProcedureWriter(procName, TableName);
        }
        protected override string ColumnDataType(DatabaseColumn column)
        {
            return new DataTypeWriter().WriteDataType(column);
        }

        protected override string ColumnDataType(string dataType)
        {
            return dataType;
        }
    }
}
