using System;
using System.Collections.Generic;
using _Scripts.Grid;
using Cysharp.Threading.Tasks;

namespace _Scripts.Controller
{
    public class BoomController : ISpecialTileController
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback)
        {
            //force vertical
            int x = origin.Coordinates().x;
            int y = origin.Coordinates().y;

            List<NormalTilePosition> targets = new();
            
            origin.CrushTile();
            
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (!BoardController.IsInBounds(x + i, y + j))
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
                    
                    targets.Add(grid[x + i, y + j]);
                }
            }
            
            crushTileAction?.Invoke(targets);
            await UniTask.Delay(100);
            completedActionCallback?.Invoke();
        }
    }
}