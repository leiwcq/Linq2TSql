namespace Linq2TSql.SchemaReader.SqlGen
{
    interface ISqlFormatProvider
    {
        string Escape(string name);
        string LineEnding();
        string RunStatements();
        int MaximumNameLength { get; }
    }
}
