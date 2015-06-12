using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.SqlGen.SqlServerCe
{
    class ConstraintWriter : SqlServer.ConstraintWriter
    {
        public ConstraintWriter(DatabaseTable table)
            : base(table)
        {
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlServerCeFormatProvider();
        }
    }
}
