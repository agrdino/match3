using System;
using _Scripts.Controller;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Tile
{
    public class NormalTile : BaseTile, ITile
    {
        public override void Crush()
        {
            base.Crush();
            NormalTilePooling.Instance.Release(this);
        }
    }
}