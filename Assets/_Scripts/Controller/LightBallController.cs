using System;
using System.Collections.Generic;
using _Scripts.Grid;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace _Scripts.Controller
{
    public class LightBallController : ISpecialTileController
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid, Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback)
        {
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
                
                targets.Add(tilePosition);
            }
            
            crushTileAction?.Invoke(targets);
            await UniTask.Delay(100);
            completedActionCallback?.Invoke();
        }
    }
}