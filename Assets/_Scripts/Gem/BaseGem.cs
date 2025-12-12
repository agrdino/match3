using System;
using _Scripts.Controller;
using _Scripts.Helper.Pooling;
using DG.Tweening;
using Redcode.Extensions;
using UnityEngine;

namespace _Scripts.Gem
{
    public abstract class BaseGem : ObjectPooling
    {
        #region ----- Component Config -----

        [SerializeField] protected SpriteRenderer _spriteRenderer;

        #endregion

        #region ----- Variable -----

        protected float _delayMove; 
        
        protected EGemType _gemType;
        protected Vector3 _targetPosition;
        protected float _velocity;
        protected Action _onCompleteMove;
        protected bool _isMoving;
        
        #endregion

        #region ----- Properties -----

        public Transform Transform() => transform;
        public GameObject GameObject() => gameObject;
        public virtual EGemType GemType() => _gemType;

        #endregion
        
        public virtual void SetUp(EGemType gemType, int order)
        {
            _gemType = gemType;
            _spriteRenderer.sprite = Config.Instance[gemType];
        }

        public virtual void MoveTo(Vector3 targetPosition, int order, Action onCompleteMoveCallback)
        {
            _delayMove = order * 0.05f;
            _targetPosition = targetPosition;
            _onCompleteMove = onCompleteMoveCallback;
            if (!_isMoving)
            {
                _isMoving = true;
                DOVirtual.Float(0, 1, 0.25f, value => _velocity = value).SetDelay(_delayMove).SetEase(Ease.InCubic);
            }
        }

        private void LateUpdate()
        {
            if (!_isMoving)
            {
                return;
            }

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                _isMoving = false;
                _onCompleteMove?.Invoke();
                _onCompleteMove = null;
                _velocity = 0;
                transform.position = _targetPosition;
                return; 
            }
            
            transform.position += (Time.deltaTime / Definition.MOVE_TIME_PER_UNIT) * _velocity * (_targetPosition - transform.position).normalized;
        }
    }
}