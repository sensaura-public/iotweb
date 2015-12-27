using System;
using System.Collections;
using System.Collections.Generic;

namespace IotWeb.Common.Http
{
    public class CaseInsensitiveDictionary<T> : IDictionary<string, T>
    {
        // The inner class
        private IDictionary<string, T> m_inner = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        public T this[string key]
        {
            get
            {
                return m_inner[key];
            }

            set
            {
                m_inner[key] = value;
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

        public ICollection<T> Values
        {
            get
            {
                return m_inner.Values;
            }
        }

        public void Add(KeyValuePair<string, T> item)
        {
            m_inner.Add(item);
        }

        public void Add(string key, T value)
        {
            m_inner.Add(key, value);
        }

        public void Clear()
        {
            m_inner.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return m_inner.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return m_inner.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            m_inner.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return m_inner.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return m_inner.Remove(item);
        }

        public bool Remove(string key)
        {
            return m_inner.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            return m_inner.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_inner).GetEnumerator();
        }
    }
}
