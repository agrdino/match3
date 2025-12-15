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
            
            _FillBoard(true);
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
        
        private async void _FillBoard(bool firstTime = false, bool isSub = false)
        {
            Debug.LogError("fill");
            int order = 0;
            ITilePosition currentPosition;
            
            //ngưng drop nếu tile trên đầu busy 
            //sau khi hết busy => fill lại
            bool needToRefill = false;
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

                    (EPositionState reason, NormalTilePosition nearestTile) = _FindNearestGem(x, y);
                    ITile nextTile = null;
                    if (reason is EPositionState.Busy)
                    {
                        Debug.LogError(currentPosition.Coordinates());
                        needToRefill = true;
                        continue;
                    }
                    if (nearestTile == null)
                    {
                        ETileType newTileType;
                        do
                        {
                            nextTile?.Release();
                            newTileType = (ETileType)Random.Range((int)ETileType.Yellow, (int)ETileType.Orange + 1);
                            nextTile = _CreateNewTile(x, y, newTileType, order);
                            nextTile.GameObject().SetActive(true);
                            tilePosition.SetFutureGem(nextTile);
                        } while (firstTime && _IsMatchAt(tilePosition.Coordinates(), predict: true));

                        order++;
                    }
                    else
                    {
                        nextTile = nearestTile.CurrentTile;
                        tilePosition.SetFutureGem(nextTile);
                        nearestTile.ReleaseTile();
                    }

                    var refill = needToRefill;
                    tilePosition.CurrentTile.MoveTo(tilePosition.Transform().position, order, async () =>
                    {
                        tilePosition.CompleteReceivedTile();
                        if (firstTime)
                        {
                            return;
                        }
                        
                        if (_IsMatchAt(tilePosition.Coordinates(), out (NormalTilePosition origin, List<NormalTilePosition> matchedTile) match))
                        {
                            _ = _MatchHandler(match, 250);
                        }
                        else if (refill)
                        {
                            _FillBoard();
                        }
                    });
                    // nextGem.Transform()
                    //     .DOMove(gemPosition.Transform().position, nextGem.Transform().position.MoveTimeCalculate(gemPosition.Transform().position))
                    //     .SetEase(Ease.Linear)
                    //     .OnComplete(() =>
                    //     {
                    //         gemPosition.CompleteReceivedGem();
                    //     });
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
                        horizontal.SetFutureGem(originTile);
                        origin.SetFutureGem(horizontalTile);

                        if (_IsMatchAt(origin.Coordinates(), true) || _IsMatchAt(horizontal.Coordinates(), true))
                        {
                            Debug.Log($"Has match at {origin.Coordinates()} - {horizontal.Coordinates()}");
                            hasAvailableMove = true;
                        }
                        
                        horizontal.SetFutureGem(horizontalTile, true);
                        origin.SetFutureGem(originTile, true);
                    }
                    
                    //vertical
                    vertical = _tilePositions[x, y + 1];
                    if (vertical != null)
                    {
                        originTile = origin.CurrentTile;
                        verticalTile = vertical.CurrentTile;

                        //swap
                        vertical.SetFutureGem(originTile);
                        origin.SetFutureGem(verticalTile);

                        if (_IsMatchAt(origin.Coordinates(), true) || _IsMatchAt(vertical.Coordinates(), true))
                        {
                            Debug.Log($"Has match at {origin.Coordinates()} - {vertical.Coordinates()}");
                            hasAvailableMove = true;
                        }
                        
                        vertical.SetFutureGem(verticalTile, true);
                        origin.SetFutureGem(originTile, true);
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
                    current.SetFutureGem(gems[i]);
                }
            } while (!_HasAnyPossibleMove());

            for (int i = 0; i < coordinates.Count; i++)
            {
                NormalTilePosition current = _tilePositions[coordinates[i].x, coordinates[i].y];
                current.ChangePositionState(EPositionState.Busy);
                gems[i].MoveTo(current.Transform().position, 0, () =>
                {
                    current.ChangePositionState(EPositionState.Free);
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
        
        private (EPositionState reason, NormalTilePosition nearest) _FindNearestGem(int x, int y)
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
                
                if (_tilePositions[x, i].PositionState() is EPositionState.Busy)
                {
                    return (EPositionState.Busy, null);
                }
                
                return (EPositionState.Free, _tilePositions[x, i]);
            }

            return (EPositionState.Free,null);
        }

        private ITile _CreateNewTile(int x, int y, ETileType tileType, int order)
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
            _FillBoard(true);
            await UniTask.Delay(3000);
            _ReCreateBoard();
        }
#endif
    }
}