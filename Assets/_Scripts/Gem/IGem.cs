using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Gem
{
    public interface IGem
    {
        public Transform Transform();
        public GameObject GameObject();
        public EGemType GemType();

        public void SetUp(EGemType gemType, int row);
        public void Crush();
        public void MoveTo(Vector3 targetPosition, int order, Action onCompleteMoveCallback);
        public UniTask Swap(Vector3 target, Action callback = null);
        public UniTask SwapAndSwapBack(Vector3 target, Action callback = null);

        public void Release();
    }
}