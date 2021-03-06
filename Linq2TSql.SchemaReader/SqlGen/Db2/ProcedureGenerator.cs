﻿using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.SqlGen.Db2
{
    class ProcedureGenerator : ProcedureGeneratorBase
    {
        public ProcedureGenerator(DatabaseTable table)
            : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.Db2)
            {
                InStoredProcedure = true,
                FormatParameter = x => "p_" + x
            };
            FormatParameter = SqlWriter.FormatParameter;
        }
        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            return new ProcedureWriter(procName, TableName, Table.SchemaOwner);
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
