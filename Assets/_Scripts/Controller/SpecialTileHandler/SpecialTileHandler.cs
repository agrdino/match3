using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;

namespace _Scripts.Controller
{
    public static class SpecialTileHandler
    {
        public static readonly ISpecialTileHandler RocketHandler = new RocketHandler();
        public static readonly ISpecialTileHandler PinWheelHandler = new PinWheelHandler();
        public static readonly ISpecialTileHandler BoomHandler = new BoomHandler();
        public static readonly ISpecialTileHandler lightBallHandler = new LightBallHandler();

        public static void Active(ETileType tileType, NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            switch (tileType)
            {
                case ETileType.PinWheel:
                {
                    PinWheelHandler.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                case ETileType.Rocket:
                {
                    RocketHandler.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                case ETileType.Boom:
                {
                    BoomHandler.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                case ETileType.LightBall:
                {
                    lightBallHandler.Active(origin, grid, crushTileAction, completedActionCallback);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
                }
            }
        }
        
        public static void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback, bool isSwapped = false)
        {
            if (origin.CurrentTile is not SpecialTile originTile)
            {
                throw new ArgumentException();
            }
            
            switch (originTile.TileType())
            {
                case ETileType.PinWheel:
                {
                    PinWheelHandler.Merge(origin, target, grid, crushTileAction, completedActionCallback, isSwapped);
                    break;
                }
                case ETileType.Rocket:
                {
                    RocketHandler.Merge(origin, target, grid, crushTileAction, completedActionCallback, isSwapped);
                    break;
                }
                case ETileType.Boom:
                {
                    BoomHandler.Merge(origin, target, grid, crushTileAction, completedActionCallback, isSwapped);
                    break;
                }
                case ETileType.LightBall:
                {
                    lightBallHandler.Merge(origin, target, grid, crushTileAction, completedActionCallback, isSwapped);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(ETileType), originTile.TileType(), null);
                }
            }
        }
    }
}