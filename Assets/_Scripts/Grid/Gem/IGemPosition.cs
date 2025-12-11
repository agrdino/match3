using UnityEngine;

namespace _Scripts.Grid.Gem
{
    public interface IGemPosition
    {
        public Transform Transform();
        public GameObject GameObject();
        
        public EGridPositionType GridPositionType();
        public bool IsAvailable();
        public EGridPositionState GridPositionState();
        public void CreateGemPosition(GameObject gameObject, Transform transform);
        public void ChangePositionState(EGridPositionState newState);
    }
}