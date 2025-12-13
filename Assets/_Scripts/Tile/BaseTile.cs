using System;
using _Scripts.Controller;
using _Scripts.Helper.Pooling;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Tile
{
    public abstract class BaseTile : ObjectPooling
    {
        #region ----- Component Config -----

        [SerializeField] protected SpriteRenderer _spriteRenderer;

        #endregion

        #region ----- Variable -----

        protected float _delayMove; 
        
        protected ETileType _tileType;
        protected Vector3 _targetPosition;
        protected float _velocity;
        protected Action _onCompleteMove;
        protected bool _isMoving;
        
        #endregion

        #region ----- Properties -----

        public Transform Transform() => transform;
        public GameObject GameObject() => gameObject;
        public virtual ETileType TileType() => _tileType;

        public event Action onCrushed;

        #endregion
        
        public virtual void SetUp(ETileType tileType, int order)
        {
            _tileType = tileType;
            _spriteRenderer.sprite = Config.Instance[tileType];
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

        public virtual async UniTask Swap(Vector3 target, Action callback = null)
        {
            await transform.DOMove(target, 0.25f).ToUniTask();
            callback?.Invoke();
        }

        public virtual async UniTask SwapAndSwapBack(Vector3 target, Action callback = null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(target, 0.15f));
            sequence.Append(transform.DOMove(transform.position, 0.15f));
            await sequence.ToUniTask();
            callback?.Invoke();
        }

        public virtual void Crush()
        {
            gameObject.SetActive(false);
            onCrushed?.Invoke();
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