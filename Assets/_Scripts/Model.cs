using System;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private EGemType _gemType;
        [SerializeField] private Sprite _avatar;

        public EGemType GemType => _gemType;
        public Sprite Avatar => _avatar;
    }
}