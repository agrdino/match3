using UnityEngine;

namespace _Scripts.Gem
{
    public abstract class BaseGem : MonoBehaviour
    {
        protected EGemType _gemType;

        public virtual EGemType GemType() => _gemType;

    }
}