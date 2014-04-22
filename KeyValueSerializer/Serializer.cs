using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FoundationDB.Client;

namespace FoundationDbSerializer
{
    public class Serializer
    {
        private static IList<PropertyInfo> FindKeyProperties<T>()
        {
            IList<PropertyInfo> keys = new List<PropertyInfo>();

            // First search through the properties
            foreach (var prop in typeof(T).GetProperties())
                foreach (var attrib in prop.CustomAttributes)
                    if (attrib.AttributeType == typeof(Key))
                        keys.Add(prop);
            return keys;
        }

        private static IList<FieldInfo> FindKeyFields<T>()
        {
            IList<FieldInfo> keys = new List<FieldInfo>();

            foreach (var field in typeof(T).GetFields())
                foreach (var attrib in field.CustomAttributes)
                    if (attrib.AttributeType == typeof(Key))
                        keys.Add(field);

            return keys;
        }

        private static IEnumerable<KeyValuePair<string,string>> ConvertToKeyValuePairs<T>(IEnumerable<T> values)
        {
            // Get a collection of all the key properties
            IList<PropertyInfo> pKeys = FindKeyProperties<T>();

            // Get a collection of all the key fields
            IList<FieldInfo> fKeys = FindKeyFields<T>();

            // Make sure we have at least 1 key
            if ((pKeys.Count + fKeys.Count) < 1)
                throw new KeyNotFoundException("No properties or fields with the Key attribute were found in " + typeof(T).FullName);

            // Get a collection of all the storable properties (writable primitives and strings)
            IEnumerable<PropertyInfo> pVals = typeof(T).GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string)));

            // Get a collection of all the storable fields (writable primitives and strings)
            IEnumerable<FieldInfo> fVals = typeof(T).GetFields().Where(f => (f.FieldType.IsPrimitive || f.FieldType == typeof(string)));

            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();

            // Iterate through the values passed in, serializing them out to key/value pairs
            foreach (var item in values)
            {
                // Build the key
                StringBuilder key = new StringBuilder("");
                foreach (var k in pKeys)
                    key.Append(k.GetValue(item));
                foreach (var k in fKeys)
                    key.Append(k.GetValue(item));

                // Build the value
                // This serialization should be improved by wrapping the value to support commas and semicolons.
                StringBuilder value = new StringBuilder("");
                foreach (var v in pVals)
                {
                    value.AppendFormat("{0},{1};", v.Name, v.GetValue(item));
                }
                foreach (var v in fVals)
                {
                    value.AppendFormat("{0},{1};", v.Name, v.GetValue(item));
                }

                results.Add(new KeyValuePair<string,string>(key.ToString(),value.ToString()));
            }

            return results;
        }

        /// <summary>
        /// Writes a collection of objects with at least one Key field into the 
        /// foundationDB key/value store as a document containing the primitives of that object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects">The object to be serialized</param>
        public async static void Write<T>(IEnumerable<T> objects)
        {
            var keyValuePairs = ConvertToKeyValuePairs(objects);

            using (var db = await Fdb.OpenAsync())
            {
                // We'll use a subspace with the same name as the class including the namespace
                var location = db.Partition(typeof(T).FullName);

                // We need a transaction to be able to make changes to the db
                using (var trans = db.BeginTransaction(System.Threading.CancellationToken.None))
                {
                    // Loop over each pair and throw it into the transaction
                    foreach (var pair in keyValuePairs)
                    {
                        trans.Set(location.Pack(pair.Key), Slice.FromString(pair.Value.ToString()));
                    }
                    
                    // Commit the changes to the db
                    await trans.CommitAsync();
                }
            }
        }

        /// <summary>
        /// Retrieves an object with the given Key from the foundationDB key/value store.
        /// Note that this has no support yet for handling null fields/properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="result"></param>
        public async static Task<T> Read<T>(string key)
        {
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

            using (var db = await Fdb.OpenAsync())
            {
                // We'll use a subspace with the same name as the class including the namespace
                var location = db.Partition(typeof(T).FullName);

                using (var trans = db.BeginTransaction(System.Threading.CancellationToken.None))
                {
                    Slice value = await trans.GetAsync(location.Pack(key));
                    if (value.HasValue)
                        keyValuePairs.Add(new KeyValuePair<string, string>(key, value.ToString()));
                }
            }

            return ConvertToObject<T>(keyValuePairs).First();
        }

        /// <summary>
        /// Retrieves an object with the given Key from the foundationDB key/value store.
        /// Note that this has no support yet for handling null fields/properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="result"></param>
        public async static void Delete<T>(string key)
        {
            List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>();

            using (var db = await Fdb.OpenAsync())
            {
                // We'll use a subspace with the same name as the class including the namespace
                var location = db.Partition(typeof(T).FullName);
                
                using (var trans = db.BeginTransaction(System.Threading.CancellationToken.None))
                {
                    //Delete the value?
                }
            }
        }

        private static Slice GetSlice(object value)
        {
            // For now we'll write everything out as a string to prevent excessive casting (there's also not a slice method for floats)
            if (value is double)
                return Slice.FromString(((double)value).ToString("R"));
            if (value is float)
                return Slice.FromString(((float)value).ToString("R"));

            return Slice.FromString(value.ToString());
        }

        private static IEnumerable<T> ConvertToObject<T>(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            // Get a collection of all the storable properties (writable primitives and strings)
            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
            foreach (var prop in typeof(T).GetProperties().Where(p => p.CanWrite && 
                (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string))))
            {
                properties.Add(prop.Name, prop);
            }

            // Get a collection of all the storable fields (writable primitives and strings)
            Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
            foreach (var field in typeof(T).GetFields().Where(f => (f.FieldType.IsPrimitive || f.FieldType == typeof(string))))
            {
                fields.Add(field.Name, field);
            }

            List<T> results = new List<T>();

            foreach (var pair in pairs)
            {
                T result = (T)FormatterServices.GetUninitializedObject(typeof(T));
                string[] values = pair.Value.Split(';');

                foreach (var value in values)
                {
                    var splitValue = value.Split(',');

                    if (properties.ContainsKey(splitValue[0]))
                        properties[splitValue[0]].SetValue(result, Convert.ChangeType(splitValue[1], properties[splitValue[0]].PropertyType));

                    if (fields.ContainsKey(splitValue[0]))
                        fields[splitValue[0]].SetValue(result, Convert.ChangeType(splitValue[1], fields[splitValue[0]].FieldType));
                }
                results.Add(result);
            }

            return results;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Key : Attribute
    {}
}
