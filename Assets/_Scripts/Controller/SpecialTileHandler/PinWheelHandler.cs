using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace _Scripts.Controller
{
    public class PinWheelHandler : ISpecialTileHandler
    {
        private ITile _pinWheel;
        
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            NormalTilePosition target;
            _pinWheel = origin.CurrentTile;
            origin.ReleaseTile();
            completedActionCallback?.Invoke();

            _pinWheel.onCrushed += ChangeTarget;
            ChangeTarget();
            
            void ChangeTarget()
            {
                do
                {
                    target = grid[UnityEngine.Random.Range(0, Definition.BOARD_WIDTH),
                        UnityEngine.Random.Range(0, Definition.BOARD_HEIGHT)];
                } while (target == null || target.IsAvailable() || target.PositionState() == EPositionState.Busy);

                _pinWheel.Transform().DOKill();
                _pinWheel.Transform().DOMove(target.Transform().position, 0.75f).SetEase(Ease.InCubic).OnComplete(async () =>
                {
                    _pinWheel.onCrushed -= ChangeTarget;
                    _pinWheel.Crush();
                    crushTileAction?.Invoke(new List<NormalTilePosition>(){target});
                    await UniTask.Delay(150);
                    completedActionCallback?.Invoke();
                });
            }
        }

        public void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback, bool isSwapped = false)
        {
            switch (target.CurrentTile.TileType())
            {
                case ETileType.LightBall:
                {
                    SpecialTileHandler.Merge(target, origin, grid, crushTileAction, completedActionCallback, true);
                    return;
                }
                case ETileType.Rocket:
                case ETileType.Boom:
                case ETileType.PinWheel:
                {
                    ITile targetTile = target.CurrentTile;
                    targetTile.GameObject().SetActive(false);
                    target.ReleaseTile();
                    completedActionCallback?.Invoke();
                    Active(origin, grid, async targets =>
                    {
                        targets[0].CrushTile();
                        targets[0].SetFutureGem(targetTile);
                        targetTile.Transform().position = targets[0].Transform().position;
                        targetTile.GameObject().SetActive(true);
                        crushTileAction?.Invoke(targets);
                    }, completedActionCallback);
                    break;
                }
                default:
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}