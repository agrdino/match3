using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Tile.Animation
{
    public class TileCrushAnimation : ITileAnimation
    {
        public async UniTask Play(GameObject gameObject, float duration = 0.2f, float delay = 0)
        {
            await gameObject.transform.DOScale(0.5f * Vector3.one, duration);
            gameObject.transform.localScale = Vector3.one;
        }
    }
}