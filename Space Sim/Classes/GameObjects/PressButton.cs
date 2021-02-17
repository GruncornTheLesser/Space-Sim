using DeepCopy;
using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameObjects
{
    /* Thing To Do:
     * Button Dont Work on window rescale
     * create button parent class
     */
    class PressButton : RenderObject2D
    {
        public Action Down;
        public Action Release;

        protected ClickBox clickbox;
        private int PressedTexture;
        private int UnPressedTexture;
        private int BorderTexture;
        private int InsideTexture;

        public PressButton(Vector2 Position, Vector2 Scale, string Button) : base(Window.SquareMesh, "Default", "PressButton")
        {
            InsideTexture = TextureManager.Get("Textures/Button textures/" + Button + ".png");
            PressedTexture = TextureManager.Get("Textures/Button textures/Button_Pressed.png");
            UnPressedTexture = TextureManager.Get("Textures/Button textures/Button_UnPressed.png");

            BorderTexture = UnPressedTexture;

            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", () => Transform_Matrix));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", Window.Get_CamMat)); // gets replaced when fixed to screen
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", EventManager.Get_Time));

            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "BorderTexture", new DeepCopy<int>(() => BorderTexture)));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "InsideTexture", new DeepCopy<int>(() => InsideTexture)));


            ShaderProgram.CompileProgram();

            Z_index = 3;

            this.Position = Position;
            this.Scale = Scale;

            FixToScreen = true;

            Down = () => { };
            Release = () => { };

            clickbox = new ClickBox(new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) }, MouseButton.Button1, true, () => Transform_Matrix);

            clickbox.Click += () =>
            {
                BorderTexture = PressedTexture;
                Down();
            };
            clickbox.UnClick += () =>
            {
                BorderTexture = UnPressedTexture;
                Release();
            };

        }
        public override void OnProcess(float delta) { }

    }
}
