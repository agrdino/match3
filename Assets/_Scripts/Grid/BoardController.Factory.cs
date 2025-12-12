using _Scripts.Controller;
using _Scripts.Gem;
using UnityEngine;

namespace _Scripts.Grid
{
    public partial class BoardController
    {
        public static IGem _GemFactory(EGemType gemType, Vector3 position, int order)
        {
            IGem newGem = GemPooling.Instance.Get();
            newGem.Transform().position = position;
            newGem.SetUp(gemType, order);
            return newGem;
        }
    }
}