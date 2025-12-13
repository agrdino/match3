using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Tile
{
    public interface ITile
    {
        public Transform Transform();
        public GameObject GameObject();
        public ETileType TileType();

        public event Action onCrushed;

        public void SetUp(ETileType tileType, int row);
        public void Crush();
        public void MoveTo(Vector3 targetPosition, int order, Action onCompleteMoveCallback);
        public UniTask Swap(Vector3 target, Action callback = null);
        public UniTask SwapAndSwapBack(Vector3 target, Action callback = null);

        public void Release();
    }
}