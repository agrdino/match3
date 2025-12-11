using UnityEngine;

namespace _Scripts.Grid.Gem
{
    public abstract class BaseGemPosition
    {
        protected Transform _transform;
        protected GameObject _gameObject;
        protected EGridPositionType _gridPositionType;
        protected EGridPositionState _gridPositionState;

        public Transform Transform() => _transform;
        public GameObject GameObject() => _gameObject;

        public virtual EGridPositionType GridPositionType() => _gridPositionType;
        public virtual EGridPositionState GridPositionState() => _gridPositionState;

        public void CreateGemPosition(GameObject gameObject, Transform transform)
        {
            _transform = transform;
            _gameObject = gameObject;
        }
        public void ChangePositionState(EGridPositionState newState)
        {
            _gridPositionState = newState;
        }
    }
}