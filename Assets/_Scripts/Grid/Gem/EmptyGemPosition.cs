namespace _Scripts.Grid.Gem
{
    public class EmptyGemPosition : BaseGemPosition, IGemPosition
    {
        public override EGridPositionType GridPositionType() => EGridPositionType.None;

        public override EPositionState PositionState() => EPositionState.Busy;
        
        public bool IsAvailable() => false;

        public void CrushGem()
        {
            throw new System.NotImplementedException();
        }

    }
}