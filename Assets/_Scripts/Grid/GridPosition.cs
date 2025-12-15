using System;
using _Data.LevelConfig;
using _Scripts.Tile;
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
        private ITilePosition _tilePosition;

        public event Action<Coordinates> onBeginSwipe;
        public event Action<ESwipeDirection> onEndSwipe; 
        
        private Vector3 _startSwipePosition;

        #endregion

        #region ----- Properties -----

        public Coordinates Coordinates => _coordinates;
        public EGridPositionType GridPositionType => _tilePosition.GridPositionType();
        public ITilePosition TilePosition => _tilePosition;

        #endregion
        
        #region ----- Public Function -----

        public void SetCoordinates(Coordinates coordinates)
        {
            _coordinates = coordinates;
        }

        public ITilePosition CreatTilePosition(GridConfigModel gridConfig)
        {
            switch (gridConfig.type)
            {
                case EGridPositionType.None:
                {
                    _spriteRenderer.enabled = false;
                    SetMask(false);
                    _tilePosition = new EmptyTilePosition();
                    break;
                }
                case EGridPositionType.Tile:
                {
                    _tilePosition = new NormalTilePosition();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(EGridPositionType), gridConfig.type, null);
                }
            }
            
            _tilePosition.CreateTilePosition(_coordinates, gameObject, transform);
            return _tilePosition;
        }

        public void SetMask(bool enable)
        {
            _mask.enabled = enable;
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

#if UNITY_EDITOR
        [SerializeField] private ETileType _toType;

        [Button]
        private void _TransformToType()
        {
            if (_toType == ETileType.None)
            {
                return;
            }
            _tilePosition.CrushTile();
            ITile newTile = BoardController.TileFactory(_toType, transform.position, 0);
            newTile.GameObject().SetActive(true);
            (_tilePosition as NormalTilePosition)?.SetFutureGem(newTile, true);
        } 

#endif
    }
}