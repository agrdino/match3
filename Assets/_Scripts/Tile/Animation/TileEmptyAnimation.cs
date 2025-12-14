using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Tile.Animation
{
    public class TileEmptyAnimation : ITileAnimation
    {
        public UniTask Play(GameObject gameObject, float duration = 0.2f, float delay = 0)
        {
            return UniTask.CompletedTask;
        }
    }
}