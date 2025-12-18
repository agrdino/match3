using System.Collections.Generic;
using System.Linq;
using _Data.LevelConfig;
using _Scripts.Controller;
using _Scripts.Tile;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Redcode.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Grid
{
    public partial class BoardController : MonoBehaviour
    {
        #region ----- Component Config -----
        
        [SerializeField] private GridController _gridController;

        private EBoardState _boardState;
        private float _shuffleTimer;

        #endregion
        
        #region ----- Variable -----

        private LevelConfigModel _levelConfig;
        private static NormalTilePosition[,] _tilePositions;
        
        #endregion

        #region ----- Unity Event -----

        private void Awake()
        {
            _tilePositions = new NormalTilePosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
        }

        private void Start()
        {
            _boardState = EBoardState.Creating;
            _LoadConfig();
            
            foreach (var gridConfig in _levelConfig.gridConfigs)
            {
                ITilePosition position = _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].CreatTilePosition(gridConfig);
                if (gridConfig.type is EGridPositionType.Tile)
                {
                    _tilePositions[gridConfig.coordinates.x, gridConfig.coordinates.y] = position as NormalTilePosition;
                    _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].onBeginSwipe += _OnBeginSwipe;
                    _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].onEndSwipe += _OnEndSwipe;
                }
            }
            _gridController.FindSpawnPosition();

            List<GridPosition> checkingPosition = new();
            bool down = false;
            for (int x = 0; x < Definition.BOARD_WIDTH; x++)
            {
                down = false;
                for (int y = 0; y < Definition.BOARD_HEIGHT; y++)
                {
                    if (!down)
                    {
                        if (_gridController.Grids[x, y].TilePosition.GridPositionType() == EGridPositionType.None)
                        {
                            continue;
                        }

                        down = true;
                    }
                    else
                    {
                        if (_gridController.Grids[x, y].TilePosition.GridPositionType() == EGridPositionType.None)
                        {
                            checkingPosition.Add(_gridController.Grids[x, y]);
                        }
                        else
                        {
                            if (checkingPosition.Count == 0)
                            {
                                continue;
                            }
                            checkingPosition.ForEach(p => p.SetMask(true));   
                            checkingPosition.Clear();
                            down = false;
                        }
                    }
                }
                checkingPosition.Clear();
            }
            
            _FillBoardFirstTime(true);
            _boardState = EBoardState.Free;
        }

        private void Update()
        {
            if (_boardState is not EBoardState.PreShuffle)
            {
                return;
            }

            if (Time.time >= _shuffleTimer)
            {
                _Shuffle();
            }
        }

        #endregion

        #region ----- Private Function -----

        private void _LoadConfig()
        {
            _levelConfig = Config.Instance.levelConfigs[0];
        }

        private async void _FillBoardFirstTime(bool firstTime = false, bool isSub = false)
        {
            int order = 0;
            ITilePosition currentPosition;

            // ngưng drop nếu tile trên đầu busy
            // sau khi hết busy => fill lại
            for (int x = 0; x < Definition.BOARD_WIDTH; x++)
            {
                order = 1;
                for (int y = 0; y < Definition.BOARD_HEIGHT; y++)
                {
                    currentPosition = _tilePositions[x, y];
                    if (currentPosition is not NormalTilePosition tilePosition)
                    {
                        continue;
                    }

                    if (!tilePosition.IsAvailable())
                    {
                        continue;
                    }

                    (ETileState reason, NormalTilePosition nearestTile) = _FindNearestGem(x, y);
                    ITile nextTile = null;
                    
                    if (nearestTile == null)
                    {
                        ETileType newTileType;
                        do
                        {
                            nextTile?.Release();
                            newTileType = (ETileType)Random.Range((int)ETileType.Yellow, (int)ETileType.Orange + 1);
                            nextTile = _CreateNewTile(x, newTileType, order);
                            nextTile.GameObject().SetActive(true);
                            tilePosition.SetFutureTile(nextTile);
                        } while (firstTime && _IsMatchAt(tilePosition.Coordinates(), predict: true));

                        order++;
                    }
                    else
                    {
                        nextTile = nearestTile.CurrentTile;
                        tilePosition.SetFutureTile(nextTile);
                        nearestTile.ReleaseTile();
                    }

                    tilePosition.CurrentTile.MoveTo(tilePosition.Transform().position, order, async () =>
                    {
                        tilePosition.CompleteReceivedTile();
                    });
                }
            }

            if (isSub)
            {
                return;
            }

            //check available match
            if (_boardState is EBoardState.Free && !_HasAnyPossibleMove())
            {
                _boardState = EBoardState.PreShuffle;
                _shuffleTimer = Time.time + Definition.DELAY_TO_SHUFFLE;
            }
        }

        private async void _FillBoard()
        {
            for (int x = 0; x < Definition.BOARD_WIDTH; x++)
            {
                _FillColumn(x);
            }
        }

        private async void _FillColumn(int column)
        {
            //trên cùng fall xuống từng nấc
            for (int y = 0; y < Definition.BOARD_HEIGHT; y++)
            {
                ITile currentTile = null;
                NormalTilePosition upperTile = null;
                
                if (_tilePositions[column, y] == null)
                {
                    continue;
                }
                
                if (!_tilePositions[column, y].IsAvailable())
                {
                    if (!_tilePositions[column, y].CurrentTile.IsMoving()
                        && _tilePositions[column, y].CurrentTile.TileState() == ETileState.Moving)
                    {
                        Debug.LogError($"make {_tilePositions[column, y].CurrentTile.GameObject().name} stop");
                        _tilePositions[column, y].CompleteReceivedTile();
                        CheckMatchAt(_tilePositions[column, y]);
                    }
                    continue;
                }
                
                for (int upperPosition = y + 1; upperPosition < Definition.BOARD_HEIGHT; upperPosition++)
                {
                    if (_tilePositions[column, upperPosition] == null)
                    {
                        continue;
                    }

                    upperTile = _tilePositions[column, upperPosition];
                    break;
                }

                if (upperTile == null)
                {
                    ETileType newTileType = (ETileType)Random.Range((int)ETileType.Yellow, (int)ETileType.Orange + 1);
                    currentTile = _CreateNewTile(column, newTileType, 1);
                    currentTile.GameObject().SetActive(true);
                }
                else
                {
                    if (upperTile.IsAvailable())
                    {
                        continue;
                    }
                    
                    if (upperTile.CurrentTile.IsMoving())
                    {
                        continue;
                    }
                    else
                    {
                        currentTile = upperTile.CurrentTile;
                        upperTile.ReleaseTile();
                    }
                }
                
                _tilePositions[column, y].SetFutureTile(currentTile);
                Debug.LogError($"make {currentTile.GameObject().name} move");
                currentTile.MoveTo(_tilePositions[column, y].Transform().position, 0, () =>
                {
                    _FillColumn(column);
                });
            }

            async void CheckMatchAt(NormalTilePosition position)
            {
                await UniTask.Delay(250);
                
                if (position.IsAvailable())
                {
                    return;
                }

                if (position.TileState() is not ETileState.Free)
                {
                    return;
                }
                
                if (_IsMatchAt(position.Coordinates(), out (NormalTilePosition origin, List<NormalTilePosition> matchedTile) match))
                {
                    _ = _MatchHandler(match, 0);
                }
            }
        }

        private bool _HasAnyPossibleMove()
        {
            NormalTilePosition origin, horizontal, vertical;
            ITile originTile, horizontalTile, verticalTile;

            bool hasAvailableMove = false;
            for (int y = 0; y < Definition.BOARD_HEIGHT - 1; y++)
            {
                for (int x = 0; x < Definition.BOARD_WIDTH - 1; x++)
                {
                    origin = _tilePositions[x, y];
                    if (origin == null)
                    {
                        continue;
                    }

                    //horizontal
                    horizontal = _tilePositions[x + 1, y];
                    if (horizontal != null)
                    {
                        originTile = origin.CurrentTile;
                        horizontalTile = horizontal.CurrentTile;

                        //swap
                        horizontal.SetFutureTile(originTile);
                        origin.SetFutureTile(horizontalTile);

                        if (_IsMatchAt(origin.Coordinates(), true) || _IsMatchAt(horizontal.Coordinates(), true))
                        {
                            Debug.Log($"Has match at {origin.Coordinates()} - {horizontal.Coordinates()}");
                            hasAvailableMove = true;
                        }
                        
                        horizontal.SetFutureTile(horizontalTile, true);
                        origin.SetFutureTile(originTile, true);
                    }
                    
                    //vertical
                    vertical = _tilePositions[x, y + 1];
                    if (vertical != null)
                    {
                        originTile = origin.CurrentTile;
                        verticalTile = vertical.CurrentTile;

                        //swap
                        vertical.SetFutureTile(originTile);
                        origin.SetFutureTile(verticalTile);

                        if (_IsMatchAt(origin.Coordinates(), true) || _IsMatchAt(vertical.Coordinates(), true))
                        {
                            Debug.Log($"Has match at {origin.Coordinates()} - {vertical.Coordinates()}");
                            hasAvailableMove = true;
                        }
                        
                        vertical.SetFutureTile(verticalTile, true);
                        origin.SetFutureTile(originTile, true);
                    }

                    if (hasAvailableMove)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [Button]
        private async void _Shuffle()
        {
            _currentPosition = null;
            _boardState = EBoardState.Shuffling;

            List<Coordinates> coordinates = new();
            List<ITile> gems = new();
            
            foreach (var gemPosition in _tilePositions)
            {
                if (gemPosition == null)
                {
                    continue;
                }
                coordinates.Add(gemPosition.Coordinates());
                gems.Add(gemPosition.CurrentTile);
                gemPosition.ReleaseTile();
            }

            do
            {
                // coordinates = coordinates.Shuffled().ToList();
                gems = gems.Shuffled().ToList();
                
                for (var i = 0; i < coordinates.Count; i++)
                {
                    NormalTilePosition current = _tilePositions[coordinates[i].x, coordinates[i].y];
                    current.SetFutureTile(gems[i]);
                }
            } while (!_HasAnyPossibleMove());

            for (int i = 0; i < coordinates.Count; i++)
            {
                NormalTilePosition current = _tilePositions[coordinates[i].x, coordinates[i].y];
                current.ChangePositionState(ETileState.Moving);
                gems[i].MoveTo(current.Transform().position, 0, () =>
                {
                    current.ChangePositionState(ETileState.Free);
                });
            }

            await UniTask.Delay(1000);

            Dictionary<NormalTilePosition, List<NormalTilePosition>> allMatch = new();
            foreach (var tilePosition in _tilePositions)
            {
                if (tilePosition == null)
                {
                    continue;
                }
                
                if (_IsMatchAt(tilePosition.Coordinates(), out (NormalTilePosition origin, List<NormalTilePosition> matchedTile) data, predict: true))
                {
                    if (allMatch.ContainsKey(tilePosition))
                    {
                        allMatch[tilePosition].AddRange(data.matchedTile);
                    }
                    else
                    {
                        allMatch.Add(data.origin, data.matchedTile);
                    }
                }
            }
            
            foreach (var key in allMatch.Keys)
            {
                allMatch[key] = allMatch[key].Distinct().ToList();
                _ = _MatchHandler((key, allMatch[key]));
            }
                                    
            _boardState = EBoardState.Free;
            _FillBoard();
        }
        
        private (ETileState reason, NormalTilePosition nearest) _FindNearestGem(int x, int y)
        {
            for (int i = y + 1; i < Definition.BOARD_HEIGHT; i++)
            {
                if (_tilePositions[x, i] == null)
                {
                    continue;
                }
                
                if (_tilePositions[x, i].IsAvailable())
                {
                    continue;
                }
                
                if (_tilePositions[x, i].TileState() is not ETileState.Free)
                {
                    return (_tilePositions[x, i].TileState(), null);
                }
                
                return (ETileState.Free, _tilePositions[x, i]);
            }

            return (ETileState.Free,null);
        }

        private ITile _CreateNewTile(int x, ETileType tileType, int order)
        {
            Vector3 spawnPosition = _gridController.DictSpawnPosition[x].Transform().position + order * Vector3.up;
            ITile newTile = TileFactory(tileType, spawnPosition, order);
            return newTile;
        }

        #endregion

        #region ----- Public Function -----

        public static bool IsNormalTilePosition(int x, int y, out NormalTilePosition normalTilePosition)
        {
            normalTilePosition = null;
            if (!GridController.IsInBounds(x, y))
            {
                return false;
            }

            normalTilePosition = _tilePositions[x, y];
            return normalTilePosition != null;
        }

        #endregion
        
#if UNITY_EDITOR
        [Button]
        [Tooltip("Để test fill board")]
        private void _ClearRandomGem()
        {
            int amount = 10;
            ITilePosition temp = null;
            for (int i = 0; i < amount; i++)
            {
                do
                {
                    temp = _tilePositions[Random.Range(0, Definition.BOARD_WIDTH), Random.Range(0, Definition.BOARD_HEIGHT)];
                } while (temp is not NormalTilePosition normalGemPosition || normalGemPosition.IsAvailable());

                ITile tile = (temp as NormalTilePosition).CurrentTile;
                NormalTilePooling.Instance.Release(tile as NormalTile);
                temp.CrushTile();
            }
            _FillBoard();   
        }

        [Button]
        [Tooltip("Để test khởi tạo có tạo match không")]
        private async void _ReCreateBoard()
        {
            foreach (var tilePosition in _tilePositions)
            {
                if (tilePosition == null)
                {
                    continue;
                }
                
                tilePosition.CrushTile();
            }
            _FillBoardFirstTime();
            await UniTask.Delay(3000);
            _ReCreateBoard();
        }
#endif
    }
}