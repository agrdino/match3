namespace _Scripts.Tile.Animation
{
    public static class TileAnimationController
    {
        public static readonly ITileAnimation EmptyAnimation = new TileEmptyAnimation();
        public static readonly ITileAnimation CrushAnimation = new TileCrushAnimation();
        public static readonly ITileAnimation ChargeAnimation = new TileChargeAnimation();
        public static readonly ITileAnimation ZoomAnimation = new TileZoomAnimation();
    }
}