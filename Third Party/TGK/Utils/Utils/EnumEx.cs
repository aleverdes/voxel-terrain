using System;
namespace TaigaGames.Utils
{
    public static class EnumUtils
    {
        public static T[] GetEnumArray<T>() where T : Enum
        {
            return (T[]) Enum.GetValues(typeof(T));
        }
    }
}
