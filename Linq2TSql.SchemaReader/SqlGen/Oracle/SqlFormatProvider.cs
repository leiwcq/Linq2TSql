namespace Linq2TSql.SchemaReader.SqlGen.Oracle
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "\"" + name + "\"";
        }

        public string LineEnding()
        {
            return ";";
        }

        public string RunStatements()
        {
            return @"
/
";
        }


        public int MaximumNameLength
        {
            get { return 30; }
        }
    }
}
