using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Helper.Pooling
{
    public class SystemPooling<T> : Singleton<SystemPooling<T>> where T : ObjectPooling
    {
        [SerializeField] private T _prefab;
        
        private List<T> _pool = new List<T>();
        private int count;

        public T Get()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].IsRelease())
                {
                    continue;
                }
                
                _pool[i].Take();
                return _pool[i];
            }

            count += 1;
            T newObject = Instantiate(_prefab, transform);
            newObject.GameObject().name = count.ToString();
            _pool.Add(newObject);
            return newObject;
        }

        public void Release(T oldObject)
        {
            oldObject.Release();
        }
    }
}