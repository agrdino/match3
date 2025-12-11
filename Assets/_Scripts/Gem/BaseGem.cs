using _Scripts.Controller;
using _Scripts.Helper.Pooling;
using UnityEngine;

namespace _Scripts.Gem
{
    public abstract class BaseGem : ObjectPooling
    {
        protected EGemType _gemType;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        
        public Transform Transform() => transform;
        public GameObject GameObject() => gameObject;

        public virtual EGemType GemType() => _gemType;
        
        public virtual void SetUp(EGemType gemType)
        {
            _gemType = gemType;
            _spriteRenderer.sprite = Config.Instance[gemType];
        }
    }
}