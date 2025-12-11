namespace _Scripts.Grid.Gem
{
    public class NormalGemPosition : BaseGemPosition, IGemPosition
    {
        public override EGridPositionType GridPositionType()
        {
            return EGridPositionType.Gem;
        }
    }
}