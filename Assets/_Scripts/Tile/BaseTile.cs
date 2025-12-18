using System;
using _Scripts.Controller;
using _Scripts.Helper.Pooling;
using _Scripts.Tile.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Redcode.Extensions;
using UnityEngine;

namespace _Scripts.Tile
{
    public abstract class BaseTile : ObjectPooling
    {
        #region ----- Component Config -----

        [SerializeField] protected SpriteRenderer _spriteRenderer;

        private ITileAnimation _tileAnimation;

        #endregion

        #region ----- Variable -----
        
        protected ETileState _tileState;
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
        public virtual ETileState TileState() => _tileState;
        public bool IsMoving() => _isMoving;

        public event Action<BaseTile> onCrushed;

        #endregion
        
        public virtual void SetUp(ETileType tileType, int order)
        {
            _tileType = tileType;
            _spriteRenderer.sprite = Config.Instance[tileType];
            SetMask(SpriteMaskInteraction.VisibleInsideMask);
            SetSortingOrder(0);
        }

        public virtual void SetSortingOrder(int sortingOrder)
        {
            _spriteRenderer.sortingOrder = sortingOrder;
        }

        public virtual void ChangeState(ETileState newState)
        {
            _tileState = newState;
        }
        
        public virtual void SetMask(SpriteMaskInteraction spriteMaskInteraction)
        {
            _spriteRenderer.maskInteraction = spriteMaskInteraction;
        }

        public virtual void MoveTo(Vector3 targetPosition, int order, Action onCompleteMoveCallback)
        {
            _targetPosition = targetPosition;
            _onCompleteMove = onCompleteMoveCallback;
            _isMoving = true;

            if (_tileState != ETileState.Moving)
            {
                DOVirtual.Float(0, 1, 0.25f, value => _velocity = value).SetEase(Ease.InCubic);
            }
            _tileState = ETileState.Moving;
        }

        public virtual void StopMove()
        {
            _isMoving = false;
            _velocity = 0;
            transform.position = _targetPosition;
            _tileState = ETileState.Free;
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
            transform.DOKill();
            onCrushed?.Invoke(this);
        }

        private void LateUpdate()
        {
            if (_tileState != ETileState.Moving)
            {
                return;
            }

            if (transform.position.y < _targetPosition.y)
            {
                _isMoving = false;
                _onCompleteMove?.Invoke();
            }
            
            transform.position += Time.deltaTime / Definition.MOVE_TIME_PER_UNIT * _velocity * Vector3.down;
        }
    }
}