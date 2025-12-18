using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using _Scripts.Tile.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Action = System.Action;
using Random = UnityEngine.Random;

namespace _Scripts.Controller
{
    public class LightBallHandler : ISpecialTileHandler
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback)
        {
            ITile oldTile = origin.CurrentTile;
            List<NormalTilePosition> targets = _GetTargets(grid);
            
            //time
            for (var i = 0; i < targets.Count; i++)
            {
                TileAnimationController.ChargeAnimation.Play(targets[i].CurrentTile.GameObject(),
                    (targets.Count - i) * 0.1f + 0.15f,
                    i * 0.1f);
            }
            
            oldTile.SetSortingOrder(100);
            oldTile.SetMask(SpriteMaskInteraction.None);
            await TileAnimationController.ChargeAnimation.Play(oldTile.GameObject(), targets.Count * 0.1f + 0.15f);
            oldTile.Crush();

            crushTileAction?.Invoke(targets);
 
            await UniTask.Delay(100);
            completedActionCallback?.Invoke();
        }

        public async void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback, bool isSwapped = false)
        {
            NormalTilePosition center = null;
            ETileType targetTile = target.CurrentTile.TileType();

            ITile oldTile = origin.CurrentTile;

            if (!isSwapped)
            {
                oldTile.Transform().position = target.Transform().position;
                center = target;
            }
            else
            {
                center = origin;
            }
            origin.ReleaseTile();
            target.CrushTile();

            List<NormalTilePosition> targets = new();
            switch (targetTile)
            {
                case ETileType.PinWheel:
                case ETileType.Rocket:
                case ETileType.Boom:
                {
                    targets = _GetTargets(grid);
                    oldTile.SetSortingOrder(100);
                    oldTile.SetMask(SpriteMaskInteraction.None);
                    TileAnimationController.ChargeAnimation.Play(oldTile.GameObject(),
                        targets.Count * 0.1f + 0.15f);

                    ITile newTile ;
                    for (var i = 0; i < targets.Count; i++)
                    {
                        targets[i].CrushTile();
                        newTile =
                            BoardController.TileFactory(targetTile, targets[i].Transform().position, 0);
                        newTile.GameObject().SetActive(true);
                        targets[i].SetFutureTile(newTile);
                        TileAnimationController.ChargeAnimation.Play(newTile.GameObject(),
                            (targets.Count - i) * 0.1f + 0.15f);
                        
                        await UniTask.Delay(100);
                    }
                    
                    await UniTask.Delay(100);
                    
                    oldTile.Crush();

                    //create one more
                    newTile =
                        BoardController.TileFactory(targetTile, center.Transform().position, 0);
                    newTile.GameObject().SetActive(true);
                    center.SetFutureTile(newTile);
                    targets.Add(center);

                    await UniTask.Delay(100);

                    break;
                }
                case ETileType.LightBall:
                {
                    targets = _GetTargets(grid, ETileType.All);
                    for (var i = 0; i < targets.Count; i++)
                    {
                        TileAnimationController.ChargeAnimation.Play(targets[i].CurrentTile.GameObject(),
                            (targets.Count - i) * 0.05f + 0.1f,
                            i * 0.05f);
                    }

                    oldTile.SetSortingOrder(100);
                    oldTile.SetMask(SpriteMaskInteraction.None);
                    await TileAnimationController.ChargeAnimation.Play(target.CurrentTile.GameObject(), targets.Count * 0.05f + 0.1f);

                    oldTile.Crush();

                    break;
                }
                case ETileType.Yellow:
                case ETileType.Green:
                case ETileType.Red:
                case ETileType.Blue:
                case ETileType.Orange:
                {
                    targets = _GetTargets(grid, targetTile);
                    TileAnimationController.ChargeAnimation.Play(oldTile.GameObject(), targets.Count * 0.1f + 0.15f);
                    for (var i = 0; i < targets.Count; i++)
                    {
                        TileAnimationController.ChargeAnimation.Play(targets[i].CurrentTile.GameObject(),
                            (targets.Count - i) * 0.1f + 0.15f);
                        
                        await UniTask.Delay(100);
                    }

                    await UniTask.Delay(100);
                    
                    oldTile.SetSortingOrder(100);
                    oldTile.SetMask(SpriteMaskInteraction.None);
                    oldTile.Crush();
                    
                    //create one more
                    ITile newTile =
                        BoardController.TileFactory(targetTile, center.Transform().position, 0);
                    newTile.GameObject().SetActive(true);
                    
                    center.SetFutureTile(newTile);
                    targets.Add(center);
                    crushTileAction?.Invoke(new List<NormalTilePosition>(targets));
                    
                    await UniTask.Delay(100);
                    completedActionCallback?.Invoke();

                    return;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            
            crushTileAction?.Invoke(new List<NormalTilePosition>(targets));
 
            await UniTask.Delay(100);
            // completedActionCallback?.Invoke();
        }

        private List<NormalTilePosition> _GetTargets(NormalTilePosition[,] grid, ETileType tileType = ETileType.None)
        {
            ETileType targetTile = tileType != ETileType.None ? tileType : (ETileType)Random.Range((int)ETileType.Yellow, (int)ETileType.PinWheel);
            List<NormalTilePosition> targets = new();

            foreach (var tilePosition in grid)
            {
                if (tilePosition == null)
                {
                    continue;
                }

                if (tilePosition.IsAvailable())
                {
                    continue;
                }
                
                
                if (targetTile != ETileType.All && tilePosition.CurrentTile.TileType() != targetTile)
                {
                    continue;
                }

                tilePosition.CurrentTile.StopMove();
                tilePosition.ChangePositionState(ETileState.Matching);
                targets.Add(tilePosition);
            }
            
            return targets;
        }
    }
}