using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TravkinGames.Utils
{
    public static class ArrayEx
    {
        public static T[] Shuffle<T> (this T[] array)
        {
            for (int t = 0; t < array.Length; t++ )
            {
                var r = Random.Range(t, array.Length);
                (array[t], array[r]) = (array[r], array[t]);
            }

            return array;
        }
        
        public static T[] Shuffle<T> (this T[] array, int seed)
        {
            var rng = new System.Random(seed);
            var length = array.Length;
            while (length > 1)
            {
                length--;
                var k = rng.Next(length + 1);
                (array[k], array[length]) = (array[length], array[k]);
            }

            return array;
        }

        public static T GetRandomElement<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T GetRandomElement<T>(this IList<T> array)
        {
            return array[Random.Range(0, array.Count)];
        }

        public static T GetRandomElement<T>(this IEnumerable<T> set)
        {
            return set.ToList().GetRandomElement();
        }
    }
}
