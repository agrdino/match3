namespace _Scripts.Grid.Gem
{
    public class EmptyGemPosition : BaseGemPosition, IGemPosition
    {
        public override EGridPositionType GridPositionType() => EGridPositionType.None;

        public override EGridPositionState GridPositionState() => EGridPositionState.Busy;

        public bool IsAvailable() => false;
    }
}