using _Scripts.Tile;
using UnityEngine;

namespace _Scripts.Grid
{
    public class NormalTilePosition : BaseTilePosition, ITilePosition
    {
        #region ----- Variable -----

        protected ITile _currentTile;

        #endregion

        #region ----- Properties -----

        public override EGridPositionType GridPositionType() => EGridPositionType.Tile;
        public override ETileState TileState() => _currentTile.TileState();

        public ITile CurrentTile => _currentTile;

        public bool IsAvailable() => _currentTile == null;

        #endregion

        #region ----- Public Function -----
        
        public override void ChangePositionState(ETileState newState)
        {
            _currentTile.ChangeState(newState);
        }

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
        
        public void SetFutureTile(ITile tile, bool force = false)
        {
            _currentTile = tile;
            ChangePositionState(ETileState.Free);
        }

        public void CompleteReceivedTile()
        {
            _currentTile.StopMove();
        }
        

        #endregion
    }
}