using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FakeServers
{
    public class HttpHeaderCollection : ILookup<string, string>
    {
        private readonly OrderedDictionary headers;

        public HttpHeaderCollection()
        {
            headers = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(string name, string value)
        {
            List<string> values;
            if (headers.Contains(name))
            {
                values = (List<string>)headers[name];
            }
            else
            {
                values = new List<string>();
                headers.Add(name, values);
            }
            values.Add(value);
        }

        public void Remove(string name)
        {
            headers.Remove(name);
        }

        public void Clear()
        {
            headers.Clear();
        }

        public bool Contains(string key)
        {
            return headers.Contains(key);
        }

        public int Count
        {
            get { return headers.Count; }
        }

        public IEnumerable<string> this[string key]
        {
            get { return (List<string>)headers[key]; }
        }

        public IEnumerator<IGrouping<string, string>> GetEnumerator()
        {
            var pairs = from pair in headers.Cast<DictionaryEntry>()
                        from value in (List<string>)pair.Value
                        group value by (string)pair.Key;
            return pairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
