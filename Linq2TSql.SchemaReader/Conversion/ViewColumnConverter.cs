using System.Data;
using Linq2TSql.SchemaReader.Conversion.KeyMaps;

namespace Linq2TSql.SchemaReader.Conversion
{
    class ViewColumnConverter : ColumnConverter
    {
        public ViewColumnConverter(DataTable columnsDataTable) : base(columnsDataTable)
        {
        }

        protected override ColumnsKeyMap LoadColumnsKeyMap()
        {
            var columnsKeyMap = new ColumnsKeyMap(ColumnsDataTable);
            if (ColumnsDataTable.Columns.Contains("VIEW_NAME")) columnsKeyMap.TableKey = "VIEW_NAME";
            return columnsKeyMap;
        }
    }
}
