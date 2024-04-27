using UnityEngine;

namespace ALJV
{
    public static class Utils
    {
        // A ReLU activation function
        public static float ReLU(float x)
        {
            return x > 0 ? x : 0;
        }
    }
}
