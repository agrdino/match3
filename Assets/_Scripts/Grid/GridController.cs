using System.Collections.Generic;
using _Data.LevelConfig;
using _Scripts.Controller;
using _Scripts.Gem;
using _Scripts.Grid.Gem;
using _Scripts.Helper;
using DG.Tweening;
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

        private LevelConfigModel _levelConfig;
        private GridPosition[,] _grid;
        private Dictionary<int, Vector3> _dictSpawnPosition = new();

        #endregion

        #region ----- Unity Event -----

        private void Awake()
        {
            _grid = new GridPosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
        }

        private void Start()
        {
            _LoadConfig();
            
            foreach (var gridPosition in _gridPositions)
            {
                _grid[gridPosition.Coordinates.x, gridPosition.Coordinates.y] = gridPosition;
            }
            
            foreach (var gridConfig in _levelConfig.gridConfigs)
            {
                _grid[gridConfig.coordinates.x, gridConfig.coordinates.y].CreateGemPosition(gridConfig);
            }
            
            _FindSpawnPosition();
            _FillBoard();
        }
        
        private void _LoadConfig()
        {
            _levelConfig = Config.Instance.levelConfigs[0];
        }

        private void _FindSpawnPosition()
        {
            for (int y = 0 ; y < Definition.BOARD_WIDTH; y++)
            {
                for (int x = Definition.BOARD_HEIGHT - 1; x >= 0; x--)
                {
                    if (_grid[x,y].GridPositionType is EGridPositionType.None)
                    {
                        continue;
                    }
                    
                    _dictSpawnPosition.Add(y, _grid[x,y].transform.position + Vector3.up);
                    break;
                }
            }
        }

        private void _FillBoard()
        {
            foreach (var gridPosition in _grid)
            {
                gridPosition.GemPosition.ChangePositionState(EGridPositionState.Free);
            }
            GridPosition currentPosition;
            for (int x = 0; x < Definition.BOARD_HEIGHT; x++)
            {
                for (int y = 0; y < Definition.BOARD_WIDTH; y++)
                {
                    currentPosition = _grid[x, y];
                    if (currentPosition.GemPosition is not NormalGemPosition gemPosition)
                    {
                        continue;
                    }

                    if (!gemPosition.IsAvailable())
                    {
                        continue;
                    }
                    
                    NormalGemPosition nearestGame = _FindNearestGem(x, y);
                    IGem nextGem = null;
                    if (nearestGame == null)
                    {
                        nextGem = _CreateNewGem(x, y);
                    }
                    else
                    {
                        nextGem = nearestGame.CurrentGem;
                        nearestGame.ReleaseGem();
                    }
                    gemPosition.SetFutureGem(nextGem);
                    
                    nextGem.Transform()
                        .DOMove(gemPosition.Transform().position, nextGem.Transform().position.MoveTimeCalculate(gemPosition.Transform().position))
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                    {
                        gemPosition.CompleteReceivedGem();
                    });
                }
            }
        }

        private NormalGemPosition _FindNearestGem(int x, int y)
        {
            for (int i = x + 1; i < Definition.BOARD_HEIGHT; i++)
            {
                if (_grid[x,y].GemPosition is not NormalGemPosition gemPosition)
                {
                    continue;
                }

                if (gemPosition.IsAvailable())
                {
                    continue;
                }
                
                return gemPosition;
            }

            return null;
        }

        private IGem _CreateNewGem(int x, int y)
        {
            Vector3 spawnPosition = _dictSpawnPosition[y] + x * Vector3.up;
            IGem newGem = _GemFactory((EGemType)Random.Range((int)EGemType.Yellow, (int)EGemType.Orange + 1),
                spawnPosition);
            return newGem;
        }

        #endregion
    }
}
