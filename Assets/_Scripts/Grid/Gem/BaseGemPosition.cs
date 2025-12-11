using UnityEngine;

namespace _Scripts.Grid.Gem
{
    public abstract class BaseGemPosition
    {
        protected Transform _transform;
        protected GameObject _gameObject;
        protected EGridPositionType _gridPositionType;
        protected EPositionState _positionState;
        protected Coordinates _coordinates;
        
        public Transform Transform() => _transform;
        public GameObject GameObject() => _gameObject;
        public Coordinates Coordinates() => _coordinates;

        public virtual EGridPositionType GridPositionType() => _gridPositionType;
        public virtual EPositionState PositionState() => _positionState;

        public void CreateGemPosition(Coordinates coordinates, GameObject gameObject, Transform transform)
        {
            _transform = transform;
            _gameObject = gameObject;
            _coordinates = coordinates;
        }
        
        public void ChangePositionState(EPositionState newState)
        {
            _positionState = newState;
        }
    }
}