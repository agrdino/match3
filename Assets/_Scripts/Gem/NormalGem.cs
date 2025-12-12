using System;
using _Scripts.Controller;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Gem
{
    public class NormalGem : BaseGem, IGem
    {
        public void Crush()
        {
            gameObject.SetActive(false);
            GemPooling.Instance.Release(this);
        }

        public async UniTask Swap(Vector3 target, Action callback = null)
        {
            await transform.DOMove(target, 0.25f).ToUniTask();
            callback?.Invoke();
        }

        public async UniTask SwapAndSwapBack(Vector3 target, Action callback = null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(target, 0.15f));
            sequence.Append(transform.DOMove(transform.position, 0.15f));
            await sequence.ToUniTask();
            callback?.Invoke();
        }
    }
}