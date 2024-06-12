using System.Runtime.CompilerServices;

namespace TaigaGames.Utils
{
    public static class IntEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBool(this int value)
        {
            return value != 0;
        }
    }
}