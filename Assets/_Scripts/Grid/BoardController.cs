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
    public partial class BoardController : MonoBehaviour
    {
        #region ----- Component Config -----
        
        [SerializeField] private GridController _gridController;

        #endregion
        
        #region ----- Variable -----

        private LevelConfigModel _levelConfig;
        private IGemPosition[,] _gem;
        
        #endregion

        #region ----- Unity Event -----

        private void Awake()
        {
            _gem = new IGemPosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
        }

        private void Start()
        {
            _LoadConfig();
            _gem = new IGemPosition[Definition.BOARD_HEIGHT, Definition.BOARD_WIDTH];
            
            foreach (var gridConfig in _levelConfig.gridConfigs)
            {
                _gem[gridConfig.coordinates.x, gridConfig.coordinates.y] = _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].CreateGemPosition(gridConfig);
                _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].onBeginSwipe += _OnBeginSwipe;
                _gridController.Grids[gridConfig.coordinates.x, gridConfig.coordinates.y].onEndSwipe += _OnEndSwipe;
            }
            _gridController.FindSpawnPosition();
            
            _FillBoard();
        }

        #endregion

        #region ----- Private Function -----

        private void _LoadConfig()
        {
            _levelConfig = Config.Instance.levelConfigs[0];
        }
        
        private void _FillBoard()
        {
            int order = 0;
            IGemPosition currentPosition;
            for (int x = 0; x < Definition.BOARD_WIDTH; x++)
            {
                order = 1;
                for (int y = 0; y < Definition.BOARD_HEIGHT; y++)
                {
                    currentPosition = _gem[x, y];
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
                        nextGem = _CreateNewGem(x, y, order);
                        order++;
                    }
                    else
                    {
                        nextGem = nearestGame.CurrentGem;
                        nearestGame.ReleaseGem();
                    }
                    gemPosition.SetFutureGem(nextGem);
                    
                    nextGem.GameObject().SetActive(true);
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
                if (_gem[x, i] is not NormalGemPosition gemPosition)
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

        private IGem _CreateNewGem(int x, int y, int order)
        {
            Vector3 spawnPosition = _gridController.DictSpawnPosition[x].Transform().position + order * Vector3.up;
            IGem newGem = _GemFactory((EGemType)Random.Range((int)EGemType.Yellow, (int)EGemType.Orange + 1),
                spawnPosition);
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
                    temp = _gem[Random.Range(0, Definition.BOARD_WIDTH), Random.Range(0, Definition.BOARD_HEIGHT)];
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