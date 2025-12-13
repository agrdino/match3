using System;
using System.Collections.Generic;
using _Scripts.Grid;

namespace _Scripts.Controller
{
    public static class SpecialTileController
    {
        public static ISpecialTileController RocketController = new RocketController();
        public static ISpecialTileController PinWheelController = new PinWheelController();
        public static ISpecialTileController BoomController = new BoomController();
        public static ISpecialTileController LightBallController = new LightBallController();

        public static void Active(ETileType tileType, NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            switch (tileType)
            {
                case ETileType.PinWheel:
                {
                    PinWheelController.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                case ETileType.Rocket:
                {
                    RocketController.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                case ETileType.Boom:
                {
                    BoomController.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                case ETileType.LightBall:
                {
                    LightBallController.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
                }
            }
        }
    }
}