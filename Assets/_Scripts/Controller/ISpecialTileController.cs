using System;
using System.Collections.Generic;
using _Scripts.Grid;

namespace _Scripts.Controller
{
    public interface ISpecialTileController
    {
        public void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback);
    }
}