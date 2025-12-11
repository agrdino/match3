using System;

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
}