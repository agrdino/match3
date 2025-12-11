using System.Collections.Generic;
using _Scripts.Grid.Gem;
using NaughtyAttributes;
using UnityEngine;

namespace _Scripts.Grid
{
    public partial class GridController : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private GridPosition _gridPositionPrefab;
        [ReadOnly] [SerializeField] private List<GridPosition> _gridPositions;


        #endregion

        #region ----- Properties -----

        public GridPosition[,] Grids => _grids;
        public Dictionary<int, IGemPosition> DictSpawnPosition => _dictSpawnPosition;        
        
        #endregion
        
        #region ----- Variable -----

        private Dictionary<int, IGemPosition> _dictSpawnPosition = new();
        private GridPosition[,] _grids;

        #endregion

        #region ----- Unity Event -----
        
        private void Awake()
        {
            _grids = new GridPosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
            foreach (var gridPosition in _gridPositions)
            {
                _grids[gridPosition.Coordinates.x, gridPosition.Coordinates.y] = gridPosition;
            }
        }
        
        #endregion

        #region ----- PublicFunction -----

        public void FindSpawnPosition()
        {
            for (int x = 0 ; x < Definition.BOARD_WIDTH; x++)
            {
                for (int y = Definition.BOARD_HEIGHT - 1; y >= 0; y--)
                {
                    if (_grids[x, y].GemPosition is not NormalGemPosition normalGemPosition)
                    {
                        continue;
                    }
                    normalGemPosition.Transform().name = "SpawnPosition";
                    _dictSpawnPosition.Add(x, normalGemPosition);
                    break;
                }
            }
        }
        
        #endregion
    }
}
