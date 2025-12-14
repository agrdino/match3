using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Tile.Animation
{
    public class TileChargeAnimation : ITileAnimation
    {
        public async UniTask Play(GameObject gameObject, float duration = 0.2f, float delay = 0)
        {
            gameObject.transform.DOShakePosition(0.1f , 0.1f, 1, 0.1f).SetDelay(delay).SetLoops(-1);
            await UniTask.Delay((int)(duration * 1000));
            gameObject.transform.DOKill();
        }
    }
}