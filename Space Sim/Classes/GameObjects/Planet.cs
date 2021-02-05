using System;
using System.Collections.Generic;
using System.Text;
using Graphics;
using OpenTK.Mathematics;
using Shaders;
using DeepCopy;
namespace GameObjects
{
    enum PlanetType
    {
        Sun,
        Mercury,
        Venus,
        Earth,
        Mars,
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Random
    }

    struct PlanetFields
    {
        public float mass;
        public Vector2 velocity;
    }
    class Planet : RenderObject2D<Vertex2D>
    {
        public readonly float Mass;
        public readonly Vector2 velocity;
        private List<PlanetFields> PlanetData
        {
            get => GetPlanetData();
        }
        Func<List<PlanetFields>> GetPlanetData;

        public Planet(Vector2 Scale, Vector2 StartPosition, DeepCopy<Matrix3> CameraCopy, DeepCopy<float> TimeCopy, string Texture, string VertexShader, string FragmentShader)
            : base(0, Scale, StartPosition, Window.SquareMesh, CameraCopy, TimeCopy, Texture, VertexShader, FragmentShader) 
        {
        }
        public override void Process(float delta)
        {
            
        }
    }
    
}
