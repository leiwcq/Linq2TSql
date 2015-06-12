using System;
using System.Data.Common;

namespace Linq2TSql.SchemaReader.Utilities
{
    /// <summary>
    /// A simple tool to discover what an ADO provider GetSchema provides
    /// </summary>
    public static class DiscoverProviderFactory
    {
        /// <summary>
        /// Discovers the specified connection string. NO ERROR TRAPPING.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        public static void Discover(string connectionString, string providerName)
        {

            var factory = DbProviderFactories.GetFactory(providerName);
            using (var conn = factory.CreateConnection())
            {
                if (conn == null) throw new NullReferenceException("连接为空");
                conn.ConnectionString = connectionString;
                conn.Open();
                string metaDataCollections = DbMetaDataCollectionNames.MetaDataCollections;
                var dt = conn.GetSchema(metaDataCollections);
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    var collectionName = (string)row["CollectionName"];
                    Console.WriteLine(collectionName);
                    if (collectionName != metaDataCollections)
                    {
                        try
                        {
                            var col = conn.GetSchema(collectionName);
                            foreach (System.Data.DataColumn column in col.Columns)
                            {
                                Console.WriteLine("\t" + column.ColumnName + "\t" + column.DataType.Name);
                            }
                        }
                        catch (NotImplementedException)
                        {
                            Console.WriteLine("\t" + collectionName + " not implemented");
                        }
                        catch (DbException exception)
                        {
                            Console.WriteLine("\t" + collectionName + " database exception (often permissions): " + exception);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("\t" + collectionName + " errors (may require restrictions) " + exception);
                        }
                    }
                }
            }
        }
    }
}
