using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class RandomColor
    {
        public static Color Get()
        {
            Vector3 v = new Vector3(Random.value, Random.value, Random.value).normalized;
            return new Color(v.x, v.y, v.z);
        }
    }
}
