using _Scripts.Controller;

namespace _Scripts.Tile
{
    public class NormalTile : BaseTile, ITile
    {
        public override void Crush()
        {
            base.Crush();
            NormalTilePooling.Instance.Release(this);
        }
    }
}