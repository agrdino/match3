using _Scripts.Grid;
using _Scripts.Helper;
using UnityEngine;

namespace _Scripts.Controller
{
    public class Config : Singleton<Config>
    {
        #region ----- Component Config -----

        [SerializeField] private GridPosition _gridPosition;

        #endregion

        #region ----- Properties -----

        public GridPosition gridPosition => _gridPosition;

        #endregion
        
    }
}