using UnityEngine;

namespace _Scripts.Helper
{
    public static class GameHelper
    {
        public static float MoveTimeCalculate(this Vector3 origin, Vector3 target)
        {
            float distance = Vector3.Distance(origin, target);
            return distance * Definition.MOVE_TIME_PER_UNIT;
        }
    }
}