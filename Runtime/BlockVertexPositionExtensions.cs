namespace AleVerDes.VoxelTerrain
{
    public static class BlockVertexPositionExtensions
    {
        public static bool IsTop(this BlockVertexPosition vertexPosition)
        {
            return vertexPosition is BlockVertexPosition.TopBackLeft or BlockVertexPosition.TopBackRight or BlockVertexPosition.TopForwardLeft or BlockVertexPosition.TopForwardRight;
        }
        
        public static bool IsBottom(this BlockVertexPosition vertexPosition)
        {
            return vertexPosition is BlockVertexPosition.BottomBackLeft or BlockVertexPosition.BottomBackRight or BlockVertexPosition.BottomForwardLeft or BlockVertexPosition.BottomForwardRight;
        }
        
        public static bool IsLeft(this BlockVertexPosition vertexPosition)
        {
            return vertexPosition is BlockVertexPosition.TopBackLeft or BlockVertexPosition.TopForwardLeft or BlockVertexPosition.BottomBackLeft or BlockVertexPosition.BottomForwardLeft;
        }
        
        public static bool IsRight(this BlockVertexPosition vertexPosition)
        {
            return vertexPosition is BlockVertexPosition.TopBackRight or BlockVertexPosition.TopForwardRight or BlockVertexPosition.BottomBackRight or BlockVertexPosition.BottomForwardRight;
        }
        
        public static bool IsForward(this BlockVertexPosition vertexPosition)
        {
            return vertexPosition is BlockVertexPosition.TopForwardLeft or BlockVertexPosition.TopForwardRight or BlockVertexPosition.BottomForwardLeft or BlockVertexPosition.BottomForwardRight;
        }
        
        public static bool IsBack(this BlockVertexPosition vertexPosition)
        {
            return vertexPosition is BlockVertexPosition.TopBackLeft or BlockVertexPosition.TopBackRight or BlockVertexPosition.BottomBackLeft or BlockVertexPosition.BottomBackRight;
        }
    }
}