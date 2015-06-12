using System.Collections.Generic;
using System.Data;
using System.Linq;
using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.Conversion
{
    class ColumnDescriptionConverter
    {
        private readonly IList<DatabaseTable> _list;

        public ColumnDescriptionConverter(DataTable dataTable)
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
            const string DESC_KEY = "ColumnDescription";
            const string COLUMN_KEY = "ColumnName";

            foreach (DataRow row in dataTable.Rows)
            {
                var schema = row[SCHEMA_KEY].ToString();
                var name = row[TABLE_KEY].ToString();
                var col = row[COLUMN_KEY].ToString();
                var desc = row[DESC_KEY].ToString();
                var table = list.FirstOrDefault(t => t.SchemaOwner == schema && t.Name == name);
                if (table == null)
                {
                    table = new DatabaseTable
                    {
                        Name = name, 
                        SchemaOwner = schema
                    };
                    list.Add(table);
                }
                table.AddColumn(col).Description = desc;
            }
            return list;
        }


        public void AddDescriptions(DatabaseTable table)
        {
            var find = _list.FirstOrDefault(t => t.SchemaOwner == table.SchemaOwner && t.Name == table.Name);
            if (find == null) return;
            foreach (var col in find.Columns)
            {
                var match = table.FindColumn(col.Name);
                if (match != null) match.Description = col.Description;
            }
        }
    }
}
