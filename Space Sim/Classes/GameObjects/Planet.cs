using System;
using System.Collections.Generic;
using System.Text;
using Graphics;
using OpenTK.Mathematics;
namespace GameObjects
{
    class Planet : RenderObject2D<Vertex2D>
    {
        public Planet(Vector2 Scale, float eccentricity, string Texture, string VertexShader, string FragmentShader) : base(0, Scale, new Vector2(), Window.SquareMesh, Texture, VertexShader, FragmentShader) { }
    }
}
