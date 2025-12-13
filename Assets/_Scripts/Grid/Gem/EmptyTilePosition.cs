namespace _Scripts.Grid
{
    public class EmptyTilePosition : BaseTilePosition, ITilePosition
    {
        public override EGridPositionType GridPositionType() => EGridPositionType.None;

        public override EPositionState PositionState() => EPositionState.Busy;
        
        public bool IsAvailable() => false;

        public void CrushTile()
        {
            throw new System.NotImplementedException();
        }

    }
}