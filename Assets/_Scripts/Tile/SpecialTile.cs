using _Scripts.Controller;

namespace _Scripts.Tile
{
    public class SpecialTile : BaseTile, ITile
    {
        public override void Crush()
        {
            base.Crush();
            SpecialTilePooling.Instance.Release(this);
        }
    }
}