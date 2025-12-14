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
            List<NormalTilePosition> targets = new();
            ETileType targetTile = (ETileType)Random.Range((int)ETileType.Yellow, (int)ETileType.PinWheel);
            
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

                if (tilePosition.CurrentTile.TileType() != targetTile)
                {
                    continue;
                }

                tilePosition.ChangePositionState(EPositionState.Busy);
                targets.Add(tilePosition);
            }
            
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

        public void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback, bool isSwapped = false)
        {
            throw new NotImplementedException();
        }
    }
}