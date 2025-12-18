namespace _Scripts.Grid
{
    public class EmptyTilePosition : BaseTilePosition, ITilePosition
    {
        public override EGridPositionType GridPositionType() => EGridPositionType.None;
        public bool IsAvailable() => false;
        
        public override void ChangePositionState(ETileState newState)
        {
            throw new System.NotImplementedException();
        }

        public void CrushTile()
        {
            throw new System.NotImplementedException();
        }
    }
}