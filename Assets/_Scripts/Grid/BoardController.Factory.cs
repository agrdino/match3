using System;
using _Scripts.Controller;
using _Scripts.Tile;
using UnityEngine;

namespace _Scripts.Grid
{
    public partial class BoardController
    {
        public static ITile TileFactory(ETileType tileType, Vector3 position, int order)
        {
            ITile newTile = null;
            switch (tileType)
            {
                case ETileType.Yellow:
                case ETileType.Green:
                case ETileType.Red:
                case ETileType.Blue:
                case ETileType.Orange:
                {
                    newTile = NormalTilePooling.Instance.Get();
                    break;
                }
                case ETileType.PinWheel:
                case ETileType.Rocket:
                case ETileType.Boom:
                case ETileType.LightBall:
                {
                    newTile = SpecialTilePooling.Instance.Get();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
                }
            }
            newTile.Transform().position = position;
            newTile.SetUp(tileType, order);
            return newTile;
        }
    }
}