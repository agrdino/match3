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
        public ETileState TileState();
        public void CreateTilePosition(Coordinates coordinates, GameObject gameObject, Transform transform);
        public void CrushTile();
    }
}