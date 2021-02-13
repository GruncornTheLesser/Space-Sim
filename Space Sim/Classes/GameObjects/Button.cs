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
     * Change Texture Passing cos it doesnt work with more than 1 texture
     */

    class SliderButton : RenderObject2D
    {
        public float Percentage;

        public SliderButton() : base(Window.SquareMesh, "Default", "Slider")
        {
            ShaderProgram = new ShaderProgram("Default", "SliderButton");
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", TransformCopy));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", Window.CameraCopy));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", EventManager.TimeCopy));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "Slider", new DeepCopy<float>(() => Percentage)));
            ShaderProgram.CompileProgram();
        }

        public override void OnProcess(float delta) { }
    }
    class PressButton : RenderObject2D
    {
        public ClickBox clickbox;
        private int PressedTexture;
        private int UnPressedTexture;
        private int BorderTexture;
        private int InsideTexture;

        public PressButton(Vector2 Position, Vector2 Scale, string Button) : base(Window.SquareMesh, "Default", "PressButton")
        {
            clickbox = new ClickBox(new Vector2[] { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, -1) }, MouseButton.Button1, () => Transform_Matrix);

            InsideTexture = Init_Textures("Button textures/" + Button);
            PressedTexture = Init_Textures("Button textures/Button_Pressed");
            UnPressedTexture = Init_Textures("Button textures/Button_UnPressed");
            BorderTexture = UnPressedTexture;

            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", TransformCopy));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", Window.CameraCopy));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", EventManager.TimeCopy));

            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "BorderTexture", new DeepCopy<int>(() => BorderTexture)));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "InsideTexture", new DeepCopy<int>(() => InsideTexture)));
            

            ShaderProgram.CompileProgram();

            Z_index = 3;

            this.Position = Position;
            this.Scale = Scale;

            FixToScreen = true;

            clickbox.Click += () => { 
                BorderTexture = PressedTexture;
            };
            clickbox.UnClick += () => { 
                BorderTexture = UnPressedTexture;
            };

        }
        public override void OnProcess(float delta) { }

    }


}
