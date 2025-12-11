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
        
        [ReadOnly] [SerializeField] private Coordinates _coordinates;
        private IGemPosition _gemPosition;

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

        public void CreateGemPosition(GridConfigModel gridConfig)
        {
            switch (gridConfig.type)
            {
                case EGridPositionType.None:
                {
                    _spriteRenderer.enabled = false;
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
            
            _gemPosition.CreateGemPosition(gameObject, transform);
        }

        #endregion
    }
}