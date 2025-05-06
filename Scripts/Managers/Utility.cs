using System.Collections.Generic;
using UnityEngine;

namespace ZeekSpace
{
    public static class Utility
    {
        public static void Shuffle<T>(List<T> list)
        {
            System.Random random = new System.Random();
            int n = list.Count;

            if (n == 0)
            {
                Debug.LogError("Cannot shuffle an empty list!");
                return;
            }

            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (list[j], list[i]) = (list[i], list[j]);
            }
            Debug.Log("Finished Shuffling!");
        }
    }
}