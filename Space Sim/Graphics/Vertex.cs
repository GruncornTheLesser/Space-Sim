using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
namespace Graphics
{
    struct Vertex
    {
        public const int Size = (2 + 4) * 4;
        
        private Vector2 Position; // 2 floats = 8 bytes
        private Color4 Colour; // 4 floats = 16 bytes

        public Vertex(Vector2 Position, Color4 Colour)
        {
            this.Position = Position;
            this.Colour = Colour;
        }
    }
}
