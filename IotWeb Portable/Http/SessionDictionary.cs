using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Http
{
    public class SessionDictionary : IDictionary<string, string>
    {
        private IDictionary<string, string> m_inner = new Dictionary<string, string>();

        public bool IsChanged { get; set; }

        public string this[string key]
        {
            get
            {
                return m_inner[key];
            }

            set
            {
                m_inner[key] = value;
                IsChanged = true;
            }
        }

        public int Count
        {
            get
            {
                return m_inner.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return m_inner.Keys;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                return m_inner.Values;
            }
        }

        public SessionDictionary()
        {
            IsChanged = false;
        }

        public void Add(KeyValuePair<string, string> item)
        {
            m_inner.Add(item);
            IsChanged = true;
        }

        public void Add(string key, string value)
        {
            m_inner.Add(key, value);
            IsChanged = true;
        }

        public void Clear()
        {
            m_inner.Clear();
            IsChanged = true;
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return m_inner.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return m_inner.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            m_inner.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return m_inner.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            IsChanged = true;
            return m_inner.Remove(item);
        }

        public bool Remove(string key)
        {
            IsChanged = true;
            return m_inner.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return m_inner.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_inner).GetEnumerator();
        }
    }
}
