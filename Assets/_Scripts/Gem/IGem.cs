using UnityEngine;

namespace _Scripts.Gem
{
    public interface IGem
    {
        public Transform Transform();
        public GameObject GameObject();
        public EGemType GemType();

        public void SetUp(EGemType gemType);
    }
}