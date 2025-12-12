using System.Collections.Generic;
using System.Linq;
using _Data.LevelConfig;
using _Scripts.Controller;
using _Scripts.Gem;
using _Scripts.Grid.Gem;
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
        private NormalGemPosition[,] _gemPositions;
        
        #endregion

        #region ----- Unity Event -----

        private void Awake()
        {
            _gemPositions = new NormalGemPosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
        }

        private void Start()
        {
            _boardState = EBoardState.Creating;
            _LoadConfig();
            
            foreach (var gridConfig in _levelConfig.gridConfigs)
            {
                IGemPosition position = _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].CreateGemPosition(gridConfig);
                if (gridConfig.type is EGridPositionType.Gem)
                {
                    _gemPositions[gridConfig.coordinates.x, gridConfig.coordinates.y] = position as NormalGemPosition;
                    _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].onBeginSwipe += _OnBeginSwipe;
                    _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].onEndSwipe += _OnEndSwipe;
                }
            }
            _gridController.FindSpawnPosition();
            
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
        
        private void _FillBoard(bool firstTime = false, bool isSub = false)
        {
            int order = 0;
            IGemPosition currentPosition;
            for (int x = 0; x < Definition.BOARD_WIDTH; x++)
            {
                order = 1;
                for (int y = 0; y < Definition.BOARD_HEIGHT; y++)
                {
                    currentPosition = _gemPositions[x, y];
                    if (currentPosition is not NormalGemPosition gemPosition)
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
                        EGemType newGemType;
                        do
                        {
                            nextGem?.Release();
                            newGemType = (EGemType)Random.Range((int)EGemType.Yellow, (int)EGemType.Orange + 1);
                            nextGem = _CreateNewGem(x, y, newGemType, order);
                            nextGem.GameObject().SetActive(true);
                            gemPosition.SetFutureGem(nextGem);
                        } while (firstTime && _IsMatchAt(gemPosition.Coordinates(), predict: true));
                        order++;
                    }
                    else
                    {
                        nextGem = nearestGame.CurrentGem;
                        gemPosition.SetFutureGem(nextGem);
                        nearestGame.ReleaseGem();
                    }
                    
                    gemPosition.CurrentGem.MoveTo(gemPosition.Transform().position, order, async () =>
                    {
                        gemPosition.CompleteReceivedGem();
                        await UniTask.Delay(200);
                        if (_IsMatchAt(gemPosition.Coordinates(), out (NormalGemPosition origin, List<NormalGemPosition> matchedGem) match))
                        {
                            _MatchHandler(match);

                            _FillBoard(isSub: true);
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
            if (!_HasAnyPossibleMove())
            {
                _boardState = EBoardState.PreShuffle;
                _shuffleTimer = Time.time + Definition.DELAY_TO_SHUFFLE;
            }
        }

        private bool _HasAnyPossibleMove()
        {
            NormalGemPosition origin, horizontal, vertical;
            IGem originGem, horizontalGem, verticalGem;

            bool hasAvailableMove = false;
            for (int y = 0; y < Definition.BOARD_HEIGHT - 1; y++)
            {
                for (int x = 0; x < Definition.BOARD_WIDTH - 1; x++)
                {
                    origin = _gemPositions[x, y];
                    if (origin == null)
                    {
                        continue;
                    }

                    //horizontal
                    horizontal = _gemPositions[x + 1, y];
                    if (horizontal != null)
                    {
                        originGem = origin.CurrentGem;
                        horizontalGem = horizontal.CurrentGem;

                        //swap
                        horizontal.SetFutureGem(originGem);
                        origin.SetFutureGem(horizontalGem);

                        if (_IsMatchAt(origin.Coordinates(), true) || _IsMatchAt(horizontal.Coordinates(), true))
                        {
                            hasAvailableMove = true;
                        }
                        
                        horizontal.SetFutureGem(horizontalGem, true);
                        origin.SetFutureGem(originGem, true);
                    }
                    
                    //vertical
                    vertical = _gemPositions[x, y + 1];
                    if (vertical != null)
                    {
                        originGem = origin.CurrentGem;
                        verticalGem = vertical.CurrentGem;

                        //swap
                        vertical.SetFutureGem(originGem);
                        origin.SetFutureGem(verticalGem);

                        if (_IsMatchAt(origin.Coordinates(), true) || _IsMatchAt(vertical.Coordinates(), true))
                        {
                            hasAvailableMove = true;
                        }
                        
                        vertical.SetFutureGem(verticalGem, true);
                        origin.SetFutureGem(originGem, true);
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
            List<IGem> gems = new();
            do
            {
                foreach (var gemPosition in _gemPositions)
                {
                    if (gemPosition == null)
                    {
                        continue;
                    }
                    coordinates.Add(gemPosition.Coordinates());
                    gems.Add(gemPosition.CurrentGem);
                    gemPosition.ReleaseGem();
                }

                coordinates = coordinates.Shuffled().ToList();
                gems = gems.Shuffled().ToList();
                
                for (var i = 0; i < coordinates.Count; i++)
                {
                    NormalGemPosition current = _gemPositions[coordinates[i].x, coordinates[i].y];
                    current.SetFutureGem(gems[i]);
                }
            } while (!_HasAnyPossibleMove());

            _boardState = EBoardState.Free;
            for (int i = 0; i < coordinates.Count; i++)
            {
                NormalGemPosition current = _gemPositions[coordinates[i].x, coordinates[i].y];
                gems[i].MoveTo(current.Transform().position, 0, () =>
                {
                    current.ChangePositionState(EPositionState.Free);
                });
            }

            await UniTask.Delay(1000);
            //check any match => fill board
        }
        
        private bool _IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }

            if (x >= Definition.BOARD_WIDTH || y >= Definition.BOARD_HEIGHT)
            {
                return false;
            }

            return true;
        }
        
        private NormalGemPosition _FindNearestGem(int x, int y)
        {
            for (int i = y + 1; i < Definition.BOARD_HEIGHT; i++)
            {
                if (_gemPositions[x, i] is not NormalGemPosition gemPosition)
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

        private IGem _CreateNewGem(int x, int y, EGemType gemType, int order)
        {
            Vector3 spawnPosition = _gridController.DictSpawnPosition[x].Transform().position + order * Vector3.up;
            IGem newGem = _GemFactory(gemType, spawnPosition, order);
            return newGem;
        }

        #endregion
        
#if UNITY_EDITOR
        [Button]
        private void _ClearRandomGem()
        {
            int amount = 10;
            IGemPosition temp = null;
            for (int i = 0; i < amount; i++)
            {
                do
                {
                    temp = _gemPositions[Random.Range(0, Definition.BOARD_WIDTH), Random.Range(0, Definition.BOARD_HEIGHT)];
                } while (temp is not NormalGemPosition normalGemPosition || normalGemPosition.IsAvailable());

                IGem gem = (temp as NormalGemPosition).CurrentGem;
                GemPooling.Instance.Release(gem as NormalGem);
                temp.CrushGem();
            }
            _FillBoard();   
        }
#endif

    }
}