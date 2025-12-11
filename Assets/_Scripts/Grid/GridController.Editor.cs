using NaughtyAttributes;
using Redcode.Extensions;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace _Scripts.Grid
{
    public partial class GridController
    {
#if UNITY_EDITOR
        [Button]
        private void _CreateGrid()
        {
            _gridPositions.ForEach(x =>
            {
                if (x != null)
                {
                    DestroyImmediate(x.gameObject);
                }
            });
            _gridPositions.Clear();
            int midY = Definition.BOARD_WIDTH / 2;
            int midX = Definition.BOARD_HEIGHT / 2;

            GridPosition gridPosition = null;
            for (int x = -midX; x < midX; x++)
            {
                for (int y = -midY; y < midY; y++)
                {
                    gridPosition = PrefabUtility.InstantiatePrefab(_gridPositionPrefab) as GridPosition;
                    gridPosition.transform.SetParent(transform);
                    gridPosition.transform.localPosition = Vector3.zero.WithX(y).WithY(x);
                    gridPosition.SetCoordinates(new Coordinates(x + midX, y + midY));
                    gridPosition.name = $"{gridPosition.Coordinates.ToString()}";
                    _gridPositions.Add(gridPosition);
                }
            }
        }
#endif

    }
}