﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.Conversion.Loaders
{
    class IndexLoader
    {
        private readonly SchemaExtendedReader _sr;
        private readonly IndexConverter _indexConverter;
        /// <summary>
        /// <c>True</c> if cannot get indexes for all tables
        /// </summary>
        private readonly bool _noIndexColumns;
        private readonly bool _noIndexes;

        public IndexLoader(SchemaExtendedReader schemaReader)
        {
            _sr = schemaReader;
            DataTable indexes = _sr.Indexes(null);
            DataTable indexColumns = _sr.IndexColumns(null);
            //MySql and Postgresql only allow indexcolumns per table
            _noIndexColumns = (indexColumns.Rows.Count == 0 && indexes.Rows.Count > 0);
            _noIndexes = (indexes.Rows.Count == 0);


            _indexConverter = new IndexConverter(indexes, indexColumns);
        }

        public void AddIndexes(DatabaseTable table)
        {
            var tableName = table.Name;
            if (!_noIndexes && !_noIndexColumns)
            {
                var indexes = _indexConverter.Indexes(tableName, table.SchemaOwner);
                var databaseIndices = indexes as DatabaseIndex[] ?? indexes.ToArray();
                table.Indexes.AddRange(databaseIndices);
                MarkIndexedColumns(table, databaseIndices);
                return;
            }
            //what about indexes but no indexcolumns?
            if (_noIndexColumns)
            {
                _indexConverter.AddIndexColumns(table.Indexes, _sr.IndexColumns(tableName));
                return;
            }

            var converter = new IndexConverter(_sr.Indexes(tableName), _sr.IndexColumns(tableName));
            var indices = converter.Indexes(tableName, table.SchemaOwner);
            var collection = indices as DatabaseIndex[] ?? indices.ToArray();
            table.Indexes.AddRange(collection);
            MarkIndexedColumns(table, collection);
        }

        private static void MarkIndexedColumns(DatabaseTable table, IEnumerable<DatabaseIndex> indexes)
        {
            foreach (var index in indexes)
            {
                foreach (var column in index.Columns)
                {
                    var tableColumn = table.FindColumn(column.Name);
                    if (tableColumn != null) tableColumn.IsIndexed = true;
                }
            }
        }
    }
}
