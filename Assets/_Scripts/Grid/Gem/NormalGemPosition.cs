using _Scripts.Gem;

namespace _Scripts.Grid.Gem
{
    public class NormalGemPosition : BaseGemPosition, IGemPosition
    {
        #region ----- Variable -----

        protected IGem _currentGem;

        #endregion

        #region ----- Properties -----

        public override EGridPositionType GridPositionType() => EGridPositionType.Gem;

        public IGem CurrentGem => _currentGem;

        public bool IsAvailable() => _currentGem == null;

        #endregion

        #region ----- Public Function -----
        
        public void ReleaseGem()
        {
            _currentGem = null;
        }
        
        public void SetFutureGem(IGem gem)
        {
            _currentGem = gem;
        }

        public void CompleteReceivedGem()
        {
            ChangePositionState(EGridPositionState.Free);
        }
        

        #endregion
        
    }
}