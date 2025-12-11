using UnityEngine;

namespace _Scripts.Helper
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        public static T Instance => _instance;

        private void Awake()
        {
            _instance = this as T;
        }
    }
}