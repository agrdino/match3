using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Tile.Animation
{
    public class TileZoomAnimation : ITileAnimation
    {
        public async UniTask Play(GameObject gameObject, float duration = 0.2f, float delay = 0)
        {
            await gameObject.transform.DOScale(3 * Vector3.one, duration);
            await UniTask.Delay(500);
            gameObject.transform.localScale = Vector3.one;
        }
    }
}