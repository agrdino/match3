using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using _Scripts.Tile.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
            
            await TileAnimationController.ChargeAnimation.Play(origin.CurrentTile.GameObject(), targets.Count * 0.1f + 0.15f);

            if (origin.CurrentTile == oldTile)
            {
                origin.CrushTile();
            }
            else
            {
                oldTile.Crush();
            }
            crushTileAction?.Invoke(targets);
 
            await UniTask.Delay(100);
            completedActionCallback?.Invoke();
        }

        public async void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback, bool isSwapped = false)
        {
            ETileType targetTile = target.CurrentTile.TileType();
            origin.CrushTile();
            List<NormalTilePosition> targets = new();
            switch (targetTile)
            {
                case ETileType.PinWheel:
                case ETileType.Rocket:
                case ETileType.Boom:
                {
                    targets = _GetTargets(grid);
                    targets.Add(target);
                    Debug.LogError(targets.Count);
                    TileAnimationController.ChargeAnimation.Play(target.CurrentTile.GameObject(),
                        targets.Count * 0.1f + 0.15f);
                    
                    for (var i = 0; i < targets.Count; i++)
                    {
                        targets[i].CrushTile();
                        ITile newTile =
                            BoardController.TileFactory(targetTile, targets[i].Transform().position, 0);
                        newTile.GameObject().SetActive(true);
                        targets[i].SetFutureGem(newTile);
                        TileAnimationController.ChargeAnimation.Play(newTile.GameObject(),
                            (targets.Count - i) * 0.1f + 0.15f);
                        
                        await UniTask.Delay(100);
                    }

                    await UniTask.Delay(200);
                    break;
                }
                case ETileType.LightBall:
                {
                    target.ChangePositionState(EPositionState.Busy);
                    ITile oldTile = target.CurrentTile;
                    targets = _GetTargets(grid, ETileType.All);
                    for (var i = 0; i < targets.Count; i++)
                    {
                        TileAnimationController.ChargeAnimation.Play(targets[i].CurrentTile.GameObject(),
                            (targets.Count - i) * 0.05f + 0.1f,
                            i * 0.05f);
                    }

                    await TileAnimationController.ChargeAnimation.Play(target.CurrentTile.GameObject(), targets.Count * 0.05f + 0.1f);

                    if (target.CurrentTile == oldTile)
                    {
                        target.CrushTile();
                    }
                    else
                    {
                        oldTile.Crush();
                    }
                    break;
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

                if (tilePosition.PositionState() is EPositionState.Busy)
                {
                    continue;
                }

                if (targetTile != ETileType.All && tilePosition.CurrentTile.TileType() != targetTile)
                {
                    continue;
                }

                tilePosition.ChangePositionState(EPositionState.Busy);
                targets.Add(tilePosition);
            }
            
            return targets;
        }
    }
}