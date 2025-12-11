using System;
using _Data.LevelConfig;
using _Scripts.Grid.Gem;
using NaughtyAttributes;
using UnityEngine;

namespace _Scripts.Grid
{
    public class GridPosition : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteMask _mask;
        
        [ReadOnly] [SerializeField] private Coordinates _coordinates;
        private IGemPosition _gemPosition;

        public event Action<Coordinates> onBeginSwipe;
        public event Action<ESwipeDirection> onEndSwipe; 
        
        private Vector3 _startSwipePosition;

        #endregion

        #region ----- Properties -----

        public Coordinates Coordinates => _coordinates;
        public EGridPositionType GridPositionType => _gemPosition.GridPositionType();
        public IGemPosition GemPosition => _gemPosition;

        #endregion
        
        #region ----- Public Function -----

        public void SetCoordinates(Coordinates coordinates)
        {
            _coordinates = coordinates;
        }

        public IGemPosition CreateGemPosition(GridConfigModel gridConfig)
        {
            switch (gridConfig.type)
            {
                case EGridPositionType.None:
                {
                    _spriteRenderer.enabled = false;
                    _mask.enabled = false;
                    _gemPosition = new EmptyGemPosition();
                    break;
                }
                case EGridPositionType.Gem:
                {
                    _gemPosition = new NormalGemPosition();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(EGridPositionType), gridConfig.type, null);
                }
            }
            
            _gemPosition.CreateGemPosition(_coordinates, gameObject, transform);
            return _gemPosition;
        }

        #endregion

        #region ----- Unity Event -----

        private void OnMouseDown()
        {
            onBeginSwipe?.Invoke(Coordinates);
            _startSwipePosition = Input.mousePosition;
        }
        
        private void OnMouseUp()
        {
            Vector3 delta = Input.mousePosition - _startSwipePosition;

            if (delta.magnitude < Definition.MIN_SWIPE_DISTENCE)
            {
                onEndSwipe?.Invoke(ESwipeDirection.Cancel);
                return;
            }
            
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                onEndSwipe?.Invoke(delta.x > 0 ? ESwipeDirection.Right : ESwipeDirection.Left);
            }
            else
            {
                onEndSwipe?.Invoke(delta.y > 0 ? ESwipeDirection.Up : ESwipeDirection.Down);
            }
        }

        #endregion
    }
}