using _Scripts.Controller;
using _Scripts.Tile;

namespace _Scripts.Grid
{
    public partial class BoardController
    {
        private void _TriggerSpecialTile(NormalTilePosition tilePosition)
        {
            if (tilePosition.CurrentTile is not SpecialTile specialGem)
            {
                return;
            }
            
            SpecialTileController.Active(specialGem.TileType(), tilePosition, _tilePositions, targets =>
            {
                _ = _MatchHandler((null, targets), 0, autoFill: false);
            } , () => _FillBoard());
        }
    }
}