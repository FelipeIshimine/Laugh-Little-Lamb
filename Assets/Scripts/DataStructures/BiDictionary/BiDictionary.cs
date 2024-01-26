using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace DataStructures.BiDictionary
{
    public class BiDictionary<TKey, TValue> : IBiDictionary<TKey, TValue>
    {
        public event Action<TKey, TValue> OnAdd; 
        public event Action<TKey, TValue> OnRemove;
        public event Action OnClear;
        
        [ShowInInspector] private readonly Dictionary<TKey, TValue> _forward = new Dictionary<TKey, TValue>();
        [ShowInInspector] private readonly Dictionary<TValue, TKey> _backward = new Dictionary<TValue, TKey>();

        public Dictionary<TKey, TValue> KeyMap => _forward;
        public Dictionary<TValue, TKey> ValueMap => _backward;

        public int Count() => _forward.Count;
        public bool IsEmpty => _forward.Count == 0;
        
        public TValue this[TKey key]
        {
            get => KeyMap[key];
            set => Add(key,value);
        }

        public TKey this[TValue key]
        {
            get => ValueMap[key];
            set => Add(value,key);
        }
        
        public void Add(TKey key, TValue value)
        {
            _forward.Add(key, value);
            _backward.Add(value, key);
            
            OnAdd?.Invoke(key,value);
        }

        public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

        public bool ContainsValue(TValue value) => _backward.ContainsKey(value);

        public bool RemoveKey(TKey key) => Remove(key,_forward[key]);

        public bool RemoveValue(TValue value) => Remove(_backward[value], value);

        public IEnumerator<KeyValuePair<TValue, TKey>> GetValueEnumerator() => _backward.GetEnumerator();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _forward.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _forward.GetEnumerator();

        private bool Remove(TKey key, TValue value)
        {
            var result = _backward.Remove(_forward[key]) && _forward.Remove(key);
            if(result) OnRemove?.Invoke(key,value);
            return result;
        }
        
        public void Clear()
        {
            _forward.Clear();
            _backward.Clear();
            OnClear?.Invoke();
        }

        public bool TryGetValue(TKey key, out TValue value) => _forward.TryGetValue(key, out value);
        public bool TryGetValue(TValue key, out TKey value) => _backward.TryGetValue(key, out value);

        public (TKey key, TValue value)[] ToArray()
        {
            int count = 0;
            (TKey key, TValue value)[] results = new (TKey key, TValue value)[_forward.Count];
            foreach (var pair in _forward)
                results[count++] = (pair.Key, pair.Value);
            return results;
        }

        public IEnumerable<TValue> GetValues()
        {
            foreach (KeyValuePair<TKey,TValue> pair in _forward)
                yield return pair.Value;
        }
        
        public IEnumerable<TKey> GetKeys()
        {
            foreach (KeyValuePair<TKey,TValue> pair in _forward)
                yield return pair.Key;
        }
    }
}
