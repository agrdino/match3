using UnityEngine;

namespace _Scripts.Grid
{
    public abstract class BaseTilePosition
    {
        protected Transform _transform;
        protected GameObject _gameObject;
        protected EGridPositionType _gridPositionType;
        protected Coordinates _coordinates;
        
        public Transform Transform() => _transform;
        public GameObject GameObject() => _gameObject;
        public Coordinates Coordinates() => _coordinates;

        public virtual EGridPositionType GridPositionType() => _gridPositionType;
        public virtual ETileState TileState() => ETileState.None;

        public void CreateTilePosition(Coordinates coordinates, GameObject gameObject, Transform transform)
        {
            _transform = transform;
            _gameObject = gameObject;
            _coordinates = coordinates;
        }

        public abstract void ChangePositionState(ETileState newState);
    }
}