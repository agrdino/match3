using UnityEngine;

namespace _Scripts.Grid
{
    public interface ITilePosition
    {
        public Transform Transform();
        public GameObject GameObject();
        public Coordinates Coordinates();
        
        public EGridPositionType GridPositionType();
        public bool IsAvailable();
        public EPositionState PositionState();
        public void CreateTilePosition(Coordinates coordinates, GameObject gameObject, Transform transform);
        public void ChangePositionState(EPositionState newState);
        public void CrushTile();
    }
}