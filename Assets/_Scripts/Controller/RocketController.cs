using System;
using System.Collections.Generic;
using _Scripts.Grid;
using Cysharp.Threading.Tasks;

namespace _Scripts.Controller
{
    public class RocketController : ISpecialTileController
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            //force vertical
            int x = origin.Coordinates().x;
            int y = origin.Coordinates().y;
            
            origin.CrushTile();

            for (int i = 0; i < Definition.BOARD_HEIGHT; i++)
            {
                await UniTask.Delay(100);
                int check = y - i;
                List<NormalTilePosition> target = new();
                if (BoardController.IsInBounds(x, check))
                {
                    if (grid[x, check] != null && !grid[x, check].IsAvailable()) 
                    {
                        target.Add(grid[x, check]);
                    }
                }

                check = y + i;
                if (BoardController.IsInBounds(x, check))
                {
                    if (grid[x, check] != null && !grid[x, check].IsAvailable()) 
                    {
                        target.Add(grid[x, check]);
                    }
                }
                crushTileAction?.Invoke(target);
            }
            await UniTask.Delay(100);
            completedActionCallback?.Invoke();
        }
    }
}