using NaughtyAttributes;
using UnityEngine;

namespace _Scripts.Grid
{
    public class GridPosition : MonoBehaviour
    {
        #region ----- Component Config -----

        [ReadOnly] [SerializeField] private Coordinates _coordinates;

        #endregion

        #region ----- Properties -----

        public Coordinates Coordinates => _coordinates;

        #endregion
        
        #region ----- Public Function -----

        public void SetCoordinates(Coordinates coordinates)
        {
            _coordinates = coordinates;
        }

        #endregion
    }
}