using System;
using System.Collections.Generic;
using _Scripts.Grid;

namespace _Scripts.Controller
{
    public interface ISpecialTileHandler
    {
        public void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback);
        
        public void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback, bool isSwapped = false);
    }
}