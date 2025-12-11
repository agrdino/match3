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

        public void SetUp(EGemType gemType);
        public void Crush();
        public UniTask Swap(Vector3 target, Action callback = null);
        public UniTask SwapAndSwapBack(Vector3 target, Action callback = null);
    }
}