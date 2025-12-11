namespace _Scripts.Grid.Gem
{
    public abstract class BaseGemPosition
    {
        protected EGridPositionType _gridPositionType;

        public virtual EGridPositionType GridPositionType() => _gridPositionType;
    }
}