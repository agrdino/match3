using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Controller
{
    public class PinWheelHandler : ISpecialTileHandler
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            NormalTilePosition target = null;
            ITile pinWheel = origin.CurrentTile;
            origin.ReleaseTile();
            completedActionCallback?.Invoke();
            
            ChangeTarget(null);

            void ChangeTarget(BaseTile oldTarget)
            {
                do
                {
                    if (oldTarget != null)
                    {
                        oldTarget.onCrushed -= ChangeTarget;
                    }

                    target = grid[UnityEngine.Random.Range(0, Definition.BOARD_WIDTH),
                        UnityEngine.Random.Range(0, Definition.BOARD_HEIGHT)];
                } while (target == null || target.IsAvailable() || target.TileState() == ETileState.Free);

                target.CurrentTile.onCrushed += ChangeTarget;

                pinWheel.SetSortingOrder(100);
                pinWheel.SetMask(SpriteMaskInteraction.None);
                pinWheel.Transform().DOMove(target.Transform().position, 0.75f).SetEase(Ease.InCubic)
                    .OnComplete(async () =>
                    {
                        target.CurrentTile.onCrushed -= ChangeTarget;
                        pinWheel.Crush();
                        crushTileAction?.Invoke(new List<NormalTilePosition>() { target });
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
                {
                    ITile targetTile = target.CurrentTile;
                    targetTile.GameObject().SetActive(false);
                    target.ReleaseTile();
                    
                    Active(origin, grid, async targets =>
                    {
                        targets[0].CrushTile();
                        targets[0].SetFutureTile(targetTile);
                        targetTile.Transform().position = targets[0].Transform().position;
                        targetTile.GameObject().SetActive(true);
                        crushTileAction?.Invoke(targets);
                    }, null);
                    break;
                }
                case ETileType.PinWheel:
                {
                    Active(target, grid, crushTileAction, null);
                    Active(origin, grid, crushTileAction, null);
                    crushTileAction?.Invoke(new List<NormalTilePosition>(){origin, target});
                    target.ReleaseTile();
                    ITile newSpinWheel =
                        BoardController.TileFactory(ETileType.PinWheel, origin.Transform().position, 0);
                    newSpinWheel.GameObject().SetActive(true);
                    target.SetFutureTile(newSpinWheel);
                    crushTileAction?.Invoke(new List<NormalTilePosition>(){target});
                    // Active(target, grid, crushTileAction, null);
                    completedActionCallback?.Invoke();
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