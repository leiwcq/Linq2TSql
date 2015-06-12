namespace Linq2TSql.SchemaReader.SqlGen.SqlServerCe
{
    class SqlServerCeFormatProvider : SqlServer.SqlFormatProvider
    {
        public override string LineEnding()
        {
            //SQL Server CE can't batch, so "GO" statements are more useful
            return RunStatements();
        }
    }
}
