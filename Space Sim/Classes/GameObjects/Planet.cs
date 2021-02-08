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
    class Planet : RenderObject2D
    {
        public readonly float Mass;
        public readonly Vector2 velocity;
        private List<PlanetFields> PlanetData
        {
            get => GetPlanetData();
        }
        Func<List<PlanetFields>> GetPlanetData;

        public Planet(string Texture, string VertexShader, string FragmentShader) : base(Window.SquareMesh, Texture, VertexShader, FragmentShader) 
        {
        }
        public override void OnProcess(float delta)
        {
            
        }
    }
    
}
