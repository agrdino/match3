using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using _Scripts.Tile.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Controller
{
    public class BoomHandler : ISpecialTileHandler
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback)
        {
            List<NormalTilePosition> targets = _GetAffectedPosition(origin, 3, grid);

            ITile boomTile = origin.CurrentTile;
            origin.ReleaseTile();

            boomTile.SetSortingOrder(100);
            boomTile.SetMask(SpriteMaskInteraction.None);
            await TileAnimationController.ZoomAnimation.Play(boomTile.GameObject());
            boomTile.Crush();
            origin.ChangePositionState(EPositionState.Free);
            
            crushTileAction?.Invoke(targets);
            await UniTask.Delay(100);
            completedActionCallback?.Invoke();
        }

        public async void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback, bool isSwapped = false)
        {
            switch (target.CurrentTile.TileType())
            {
                case ETileType.Rocket:
                case ETileType.PinWheel:
                case ETileType.LightBall:
                {
                    SpecialTileHandler.Merge(target, origin, grid, crushTileAction, completedActionCallback, true);
                    return;
                }
                case ETileType.Boom:
                {
                    origin.CrushTile();
                    //get all target
                    
                    ITile boomTile = target.CurrentTile;
                    boomTile.SetMask(SpriteMaskInteraction.None);
                    boomTile.SetSortingOrder(100);
                    target.ReleaseTile();
                    
                    List<NormalTilePosition> targets = _GetAffectedPosition(target, 5, grid);
                    await TileAnimationController.ZoomAnimation.Play(boomTile.GameObject());

                    boomTile.Crush();
                    crushTileAction?.Invoke(targets);
                    await UniTask.Delay(100);
                    completedActionCallback?.Invoke();

                    break;
                }
                default:
                {
                    throw new ArgumentException();
                }
            }
        }

        private List<NormalTilePosition> _GetAffectedPosition(NormalTilePosition center, int range, NormalTilePosition[,] grid)
        {
            int x = center.Coordinates().x;
            int y = center.Coordinates().y;

            List<NormalTilePosition> targets = new();

            int mid = (int)(range / 2f);
            
            for (int i = -mid; i <= mid; i++)
            {
                for (int j = -mid; j <= mid; j++)
                {
                    if (!GridController.IsInBounds(x + i, y + j))
                    {
                        continue;
                    }

                    if (grid[x + i, y + j] == null)
                    {
                        continue;
                    }
                    
                    if (grid[x + i, y + j].IsAvailable())
                    {
                        continue;
                    }
                    
                    if (grid[x + i, y + j].PositionState() is EPositionState.Busy)
                    {
                        continue;
                    }

                    grid[x + i, y + j].ChangePositionState(EPositionState.Busy);
                    
                    targets.Add(grid[x + i, y + j]);
                }
            }

            return targets;
        }
    }
}