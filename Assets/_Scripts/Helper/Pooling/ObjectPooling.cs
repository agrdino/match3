using UnityEngine;

namespace _Scripts.Helper.Pooling
{
    public abstract class ObjectPooling : MonoBehaviour
    {
        protected bool _isRelease = false;
        public bool IsRelease => _isRelease;

        public void Release()
        {
            _isRelease = true;
        }

        public void Take()
        {
            _isRelease = false;
        }
    }
}