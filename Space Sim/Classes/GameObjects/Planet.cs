using System;
using System.Collections.Generic;
using System.Text;
using Graphics;
using OpenTK.Mathematics;
using Shaders;
namespace GameObjects
{
    
    class Planet : RenderObject2D<Vertex2D>
    {
        public Planet(Vector2 Scale, Vector2 StartPosition, DeepCopy<Matrix3> CameraCopy, DeepCopy<float> TimeCopy, string Texture, string VertexShader, string FragmentShader)
            : base(0, Scale, StartPosition, Window.SquareMesh, CameraCopy, TimeCopy, Texture, VertexShader, FragmentShader) 
        { 
            
        }
        public override void Process(float delta)
        {
            
        }
    }
    
}
