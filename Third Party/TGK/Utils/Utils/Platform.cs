
#if UNITY_WEBGL
using GamePush;
#endif

namespace TaigaGames.Utils
{
    public static class Platform
    {
        public static bool IsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#elif UNITY_WEBGL
            return GP_Device.IsMobile();
#endif
            return false;
        }

        public static bool IsDesktop()
        {
            return !IsMobile();
        }
    }
}