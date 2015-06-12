using System.Collections.Generic;
using System.Data;
using System.Linq;
using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.Conversion
{
    class TableDescriptionConverter
    {
        private readonly IList<DatabaseTable> _list;

        public TableDescriptionConverter(DataTable dataTable)
        {
            _list = Convert(dataTable);
        }

        private static IList<DatabaseTable> Convert(DataTable dataTable)
        {
            var list = new List<DatabaseTable>();
            if ((dataTable == null) || (dataTable.Columns.Count == 0) || (dataTable.Rows.Count == 0))
            {
                return list;
            }

            const string SCHEMA_KEY = "SchemaOwner";
            const string TABLE_KEY = "TableName";
            const string DESC_KEY = "TableDescription";

            list.AddRange(from DataRow row in dataTable.Rows
                let schema = row[SCHEMA_KEY].ToString()
                let name = row[TABLE_KEY].ToString()
                let desc = row[DESC_KEY].ToString()
                select new DatabaseTable
                {
                    Name = name,
                    SchemaOwner = schema, 
                    Description = desc
                });
            return list;
        }


        public string FindDescription(string schema, string tableName)
        {
            var find = _list.FirstOrDefault(t => t.SchemaOwner == schema && t.Name == tableName);
            return find == null ? null : find.Description;
        }
    }
}
