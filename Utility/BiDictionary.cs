using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetrosikUtility
{
    internal class BiDictionary<TKey,TValue> : IDictionary<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        private Dictionary<TKey, TValue> Normal = new();
        private Dictionary<TValue, TKey> Reverse = new();

        public BiDictionary() 
        {
            
        }

        public TValue this[TKey key]
        {
            get => Normal[key];
            set => Normal[key] = value;
        }

        public TKey this[TValue key]
        {
            get => Reverse[key];
            set => Reverse[key] = value;
        }

        public ICollection<TKey> Keys => Normal.Keys;
        public ICollection<TValue> ReverseKeys => Reverse.Keys;

        public ICollection<TValue> Values => Normal.Values;
        public ICollection<TKey> ReverseValues => Reverse.Values;

        public int Count => Normal.Count;

        public bool IsReadOnly => false;



        public void Add(TKey key, TValue value)
        {
            Normal.Add(key, value);
            Reverse.Add(value, key);
        }
        public void Add(TValue key, TKey value)
        { 
            Reverse.Add(key, value);
            Normal.Add(value, key);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
           Add(item.Key, item.Value);
        }

        public void Add(KeyValuePair<TValue, TKey> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear() 
        {
            Normal.Clear();
            Reverse.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) =>
            Normal.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

        public bool Contains(KeyValuePair<TValue, TKey> item) =>
           Reverse.TryGetValue(item.Key, out var value) && EqualityComparer<TKey>.Default.Equals(value, item.Value);

        public bool ContainsKey(TKey key) => Normal.ContainsKey(key);
        public bool ContainsKey(TValue key) => Reverse.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var pair in Normal)
            {
                array[arrayIndex++] = pair;
            }
        }
        public void CopyTo(KeyValuePair<TValue, TKey>[] array, int arrayIndex)
        {
            foreach (var pair in Reverse)
            {
                array[arrayIndex++] = pair;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Normal.GetEnumerator();
        public IEnumerator<KeyValuePair<TValue, TKey>> GetReverseEnumerator() => Reverse.GetEnumerator();

        public bool Remove(TKey key) {
            Reverse.Remove(Normal[key]);
            return Normal.Remove(key);
        }
        public bool Remove(TValue key)
        {
            Normal.Remove(Reverse[key]);
            return Reverse.Remove(key);
        }


        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                return false;
            return Remove(item.Key);
        }
        public bool Remove(KeyValuePair<TValue, TKey> item)
        {
            if (!Contains(item))
                return false;

            return Remove(item.Key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => Normal.TryGetValue(key, out value);
        public bool TryGetValue(TValue key, [MaybeNullWhen(false)] out TKey value) => Reverse.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => Normal.GetEnumerator();
    }
}
