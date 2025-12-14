using _Scripts.Controller;

namespace _Scripts.Grid
{
    public partial class BoardController
    {
        private void _TriggerSpecialTile(NormalTilePosition origin)
        {
            SpecialTileHandler.Active(origin.CurrentTile.TileType(), origin, _tilePositions, targets =>
            {
                _ = _MatchHandler((null, targets), 0, bySpecial: true, autoFill: false);
            } , () => _FillBoard());
        }
        
        private void _MergeSpecialTile(NormalTilePosition origin, NormalTilePosition target)
        {
            SpecialTileHandler.Merge(origin, target, _tilePositions, targets =>
            {
                _ = _MatchHandler((null, targets), 0, bySpecial: true, autoFill: false);
            } , () => _FillBoard());
        }
    }
}