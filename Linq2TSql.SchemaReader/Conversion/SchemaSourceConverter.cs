using System;
using System.Collections.Generic;
using System.Data;
using Linq2TSql.SchemaReader.DataSchema;

namespace Linq2TSql.SchemaReader.Conversion
{
    class SchemaSourceConverter
    {
        public static void AddSources(DatabaseSchema schema, DataTable dt)
        {
            //oracle sources come in lines; assume in order, so we can just concatenate
            //if they already have source, we don't want to overwrite, so we use a cache
            var functionCache = new Dictionary<string, DatabaseFunction>();

            foreach (DataRow row in dt.Rows)
            {
                string owner = row["OWNER"].ToString();
                string name = row["NAME"].ToString();
                string type = row["TYPE"].ToString().Trim();
                string text = row["TEXT"].ToString();
                DatabaseStoredProcedure matchProcedure;
                switch (type)
                {
                    case "PACKAGE": //oracle package
                        var package = FindPackage(schema, name, owner);
                        //text will have a newline but not cReturn
                        package.Definition += text;
                        break;

                    case "PACKAGE BODY": //oracle package body
                        var package2 = FindPackage(schema, name, owner);
                        //text will have a newline but not cReturn
                        package2.Body += text;
                        break;

                    case "PROCEDURE": //oracle procedure
                        matchProcedure = FindStoredProcedure(schema, name);
                        if (matchProcedure == null) continue;
                        //text will have a newline but not cReturn
                        matchProcedure.Sql += text;
                        break;

                    case "FUNCTION": //oracle function
                        var function = FindFunction(name, schema, functionCache);
                        if (function == null) continue;
                        function.Sql += text;
                        break;

                    case "P": //sql server procedure
                        matchProcedure = FindStoredProcedure(schema, name);
                        if (matchProcedure == null) continue;
                        matchProcedure.Sql = text;
                        break;

                    case "TF": //sql server table-valued function
                    case "FN": //sql server scalar function
                        var matchFunction = FindFunction(schema, name);
                        if (matchFunction == null) continue;
                        matchFunction.Sql = text;
                        break;

                    case "V": //sql server view
                        DatabaseView matchView = FindView(schema, name);
                        if (matchView == null) continue;
                        matchView.Sql = text;
                        break;
                }
            }
        }

        private static DatabaseFunction FindFunction(string name, DatabaseSchema schema, Dictionary<string, DatabaseFunction> functionCache)
        {
            DatabaseFunction function;
            if (functionCache.ContainsKey(name))
            {
                function = functionCache[name];
            }
            else
            {
                function = FindFunction(schema, name);
                if (function == null) return null;
                //we already have sql from the functions collection. Don't add to it.
                if (!string.IsNullOrEmpty(function.Sql)) return null;
                functionCache.Add(name, function);
            }
            return function;
        }

        private static DatabaseView FindView(DatabaseSchema schema, string name)
        {
            return schema.Views.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static DatabaseFunction FindFunction(DatabaseSchema schema, string name)
        {
            return schema.Functions.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static DatabaseStoredProcedure FindStoredProcedure(DatabaseSchema schema, string name)
        {
            return schema.StoredProcedures.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static DatabasePackage FindPackage(DatabaseSchema schema, string name, string owner)
        {
            var matchPackage = schema.Packages.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (matchPackage == null)
            {
                matchPackage = AddPackage(name, owner);
                schema.Packages.Add(matchPackage);
            }
            return matchPackage;
        }

        private static DatabasePackage AddPackage(string name, string owner)
        {
            var pack = new DatabasePackage
            {
                Name = name,
                SchemaOwner = owner
            };
            return pack;
        }
    }
}
