using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.SqlGen
{
    /// <summary>
    /// Consistency for the data type converter classes
    /// </summary>
    interface IDataTypeWriter
    {
        /// <summary>
        /// Writes the data as a string (suitable for column DDL)
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>A string</returns>
        string WriteDataType(DatabaseColumn column);
    }
}
