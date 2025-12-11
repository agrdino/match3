namespace _Scripts.Grid.Gem
{
    public class EmptyGemPosition : BaseGemPosition, IGemPosition
    {
        public override EGridPositionType GridPositionType()
        {
            return EGridPositionType.None;
        }
    }
}