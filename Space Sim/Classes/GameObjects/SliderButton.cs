using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Shaders;
using DeepCopy;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameObjects
{
    /* THING TO DO:
     * Update Scale on Window resize
     */

    class SliderButton : RenderObject2D
    {
        ClickBox clickbox;
        public float Percentage = 0;

        private int IconTextureHandle;
        private int SliderTextureHandle;

        public SliderButton(Vector2 Position, Vector2 Scale, string Button) : base(Window.SquareMesh, "Default", "SliderButton")
        {
            IconTextureHandle = TextureManager.Get("Textures/Button textures/Button_Slider.png");
            SliderTextureHandle = TextureManager.Get("Textures/Button textures/Button_SliderScale.png");


            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", Window.Get_CamMat));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", EventManager.Get_Time));

            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "Percentage", new DeepCopy<float>(() => Percentage)));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "XYratio", new DeepCopy<float>(() => Scale.X / Scale.Y)));
            
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "IconTexture", new DeepCopy<int>(() => IconTextureHandle)));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "ScaleTexture", new DeepCopy<int>(() => SliderTextureHandle)));



            ShaderProgram.CompileProgram();
            
            Z_index = 3;
            this.Position = Position;
            this.Scale = Scale;
            FixToScreen = true;

            clickbox = new ClickBox(new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) }, MouseButton.Button1, true, () => Transform_Matrix);

            clickbox.Click += () => {
                EventManager.MouseMove += Move_Slider;
            };
            clickbox.UnClick += () => {
                EventManager.MouseMove -= Move_Slider;
            };
        }
        private void Move_Slider(MouseState MS, MouseMoveEventArgs e)
        {
            Vector2 ScreenPos = Window.MouseToScreen(MS.Position);
            Percentage = Math.Clamp((((Window.Get_BaseMat() * Transform_Matrix).Inverted() * new Vector3(ScreenPos.X, ScreenPos.Y, 1)).X + 0.5f), 0, 1);
        }

        public override void OnProcess(float delta) { }
    }
}
