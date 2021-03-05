using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Graphics.Shaders;
using Graphics;

namespace GameObjects
{
    class SliderButton : RenderObject2D
    {
        ClickBox clickbox;
        
        public float Percentage
        {
            get => percentage;
            set {
                percentage = value;
                Set_Percentage(percentage);
            }
        }
        private float percentage;
        public Action<float> Set_Percentage = (percent) => { };

        private readonly string IconTexture;
        private readonly string SliderTexture;

        

        public SliderButton(RenderWindow RenderWindow, Vector2 Position, Vector2 Scale) : base(RenderWindow, SquareMesh, "Default", "SliderButton")
        {
            IconTexture = "Textures/Button textures/Button_Slider.png";
            SliderTexture = "Textures/Button textures/Button_SliderScale.png";

            // pass in uniforms
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", () => RenderWindow.EventManager.Program_Time));

            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "Percentage", new DeepCopy<float>(() => Percentage)));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "XYratio", new DeepCopy<float>(() => Scale.X / Scale.Y)));
            
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "IconTexture", () => TextureManager.Get(IconTexture)));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "ScaleTexture", () => TextureManager.Get(SliderTexture)));

            // compile shader
            ShaderProgram.CompileProgram();
            
            Z_index = 5; 
            this.Position = Position;
            this.Scale = Scale;
            FixToScreen = true;

            clickbox = new ClickBox(RenderWindow, new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) }, MouseButton.Button1, true, () => Transform_Matrix);

            clickbox.Click += () => {
                RenderWindow.EventManager.MouseMove += Move_Slider;
            };
            clickbox.UnClick += () => {
                RenderWindow.EventManager.MouseMove -= Move_Slider;
            };            
        }
        private void Move_Slider(MouseState MS, MouseMoveEventArgs e)
        {
            Vector2 ScreenPos = RenderWindow.MouseToScreen(MS.Position);
            Percentage = Math.Clamp((((RenderWindow.Camera.BaseMatrix * Transform_Matrix).Inverted() * new Vector3(ScreenPos.X, ScreenPos.Y, 1)).X + 0.5f), 0, 1);
        }
        
    }
}
