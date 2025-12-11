using UnityEngine;

namespace _Scripts.Grid.Gem
{
    public interface IGemPosition
    {
        public Transform Transform();
        public GameObject GameObject();
        public Coordinates Coordinates();
        
        public EGridPositionType GridPositionType();
        public bool IsAvailable();
        public EPositionState PositionState();
        public void CreateGemPosition(Coordinates coordinates, GameObject gameObject, Transform transform);
        public void ChangePositionState(EPositionState newState);
        public void CrushGem();
    }
}