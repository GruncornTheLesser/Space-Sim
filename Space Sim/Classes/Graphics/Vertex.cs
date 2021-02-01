using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
namespace Graphics
{
    /* Ive tried to set it up so the program can use both 3d and 2d.
     * 
     * This means instead of having seperate objects for different vertex types
     * you pass in the custom vertex you want to use and write a corresponding shader.
     * 
     * This means that the shaders are written for a particular type vertex.
     */

    public struct Vertex2D
    {
        
        public Vector2 TextureUV; // 2 floats = 8 bytes
        public Color4 VertColour; // 4 floats = 16 bytes
        public Vector2 VertUV; // 2 floats = 8 bytes
        public Vertex2D(Vector2 Position, Vector2 TextureUV, Color4 Colour)
        {
            this.VertUV = Position;
            this.TextureUV = TextureUV;
            this.VertColour = Colour;
            
        }
        public Vertex2D(float PositionX, float PositionY, float TextureU, float TextureV, float R, float G, float B, float A)
        {
            this.VertUV = new Vector2(PositionX, PositionY);
            this.TextureUV = new Vector2(TextureU, TextureV);
            this.VertColour = new Color4(R, G, B, A);
        }
    }
    
    // no dont. just dont. no 3D. 
    public struct Vertex3D
    {
        private Vector3 Position; // 3 floats = 12 bytes
        private Vector2 TextureUV; // 2 floats = 8 bytes
        private Color4 Colour; // 4 floats = 16 bytes

        public Vertex3D(Vector3 Position, Vector2 TextureUV, Color4 Colour)
        {
            this.Position = Position;
            this.TextureUV = TextureUV;
            this.Colour = Colour;
        }
        public Vertex3D(float PositionX, float PositionY, float PositionZ, float TextureU, float TextureV, float R, float G, float B, float A)
        {
            this.Position = new Vector3(PositionX, PositionY, PositionZ);
            this.TextureUV = new Vector2(TextureU, TextureV);
            this.Colour = new Color4(R, G, B, A);
        }
    }
}
