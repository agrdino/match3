using _Scripts.Controller;
using _Scripts.Gem;
using UnityEngine;

namespace _Scripts.Grid
{
    public partial class GridController
    {
        public static IGem _GemFactory(EGemType gemType, Vector3 position)
        {
            IGem newGem = GemPooling.Instance.Get();
            newGem.Transform().position = position;
            newGem.SetUp(gemType);
            return newGem;
        }
    }
}