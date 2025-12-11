using System.Collections.Generic;
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
        
        #region ----- Variable -----

        private GridPosition[,] _grid;

        #endregion

        #region ----- Unity Event -----

        private void Awake()
        {
            _grid = new GridPosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
        }

        private void Start()
        {
            foreach (var gridPosition in _gridPositions)
            {
                _grid[gridPosition.Coordinates.x, gridPosition.Coordinates.y] = gridPosition;
            }
        }

        #endregion
    }
}
