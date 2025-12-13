using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts
{
    [Serializable]
    public struct Coordinates
    {
        public int x;
        public int y;

        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"({x} _ {y})";
        }
    }

    [Serializable]
    public class GemAvatarModel
    {
        [FormerlySerializedAs("_gemType")] [SerializeField] private ETileType tileType;
        [SerializeField] private Sprite _avatar;

        public ETileType TileType => tileType;
        public Sprite Avatar => _avatar;
    }
}