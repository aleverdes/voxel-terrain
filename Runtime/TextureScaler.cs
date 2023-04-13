using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    public static class TextureScaler
    {
        /// <summary>
        ///     Returns a scaled copy of given texture.
        /// </summary>
        /// <param name="src">Source texture to scale</param>
        /// <param name="width">Destination texture width</param>
        /// <param name="height">Destination texture height</param>
        /// <param name="mode">Filtering mode</param>
        public static Texture2D Scale(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new(0, 0, width, height);
            ScaleTexture(src, width, height, mode);

            Texture2D result = new(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }
 
        /// <summary>
        ///     Scales the texture data of the given texture.
        /// </summary>
        /// <param name="tex">Texure to scale</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="mode">Filtering mode</param>
        public static void Rescale(this Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new(0, 0, width, height);
            ScaleTexture(tex, width, height, mode);
 
            tex.Reinitialize(width, height);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true);
        }

        private static void ScaleTexture(Texture2D src, int width, int height, FilterMode fmode)
        {
            src.filterMode = fmode;
            src.Apply(true);

            RenderTexture rtt = new(width, height, 32);

            Graphics.SetRenderTarget(rtt);

            GL.LoadPixelMatrix(0, 1, 1, 0);
            GL.Clear(true, true, Color.clear);
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }
}