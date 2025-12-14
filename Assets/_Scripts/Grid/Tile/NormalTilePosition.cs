using _Scripts.Tile;
using NaughtyAttributes;
using UnityEngine;

namespace _Scripts.Grid
{
    public class NormalTilePosition : BaseTilePosition, ITilePosition
    {
        #region ----- Variable -----

        protected ITile _currentTile;

        #endregion

        #region ----- Properties -----

        public override EGridPositionType GridPositionType() => EGridPositionType.Gem;

        public ITile CurrentTile => _currentTile;

        public bool IsAvailable() => _currentTile == null;

        #endregion

        #region ----- Public Function -----

        public void CrushTile()
        {
            if (_currentTile == null)
            {
                Debug.Log("Destroy bởi thứ khác");
                return;
            }
            _currentTile.Crush();
            ReleaseTile();
        }
        
        public void ReleaseTile()
        {
            _currentTile = null;
        }
        
        public void SetFutureGem(ITile tile, bool force = false)
        {
            ChangePositionState(force ? EPositionState.Free : EPositionState.Busy);
            _currentTile = tile;
        }

        public void CompleteReceivedTile()
        {
            ChangePositionState(EPositionState.Free);
        }
        

        #endregion
    }
}