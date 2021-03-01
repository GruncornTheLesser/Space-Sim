using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Graphics;
using Graphics.Shaders;

namespace GameObjects
{
    /* Thing To Do:
     * Button Dont Work on window rescale
     * create button parent class
     */
    sealed class PressButton : RenderObject2D
    {
        public Action Down;
        public Action Release;

        private ClickBox clickbox;

        private string PressedTexture;
        private string UnPressedTexture;
        private string BorderTexture;
        private string InsideTexture;

        public PressButton(Vector2 Position, Vector2 Scale, string Button) : base(SquareMesh, "Default", "PressButton")
        {
            // create textures
            InsideTexture = "Textures/Button textures/" + Button + ".png";
            PressedTexture = "Textures/Button textures/Button_Pressed.png";
            UnPressedTexture = "Textures/Button textures/Button_UnPressed.png";

            BorderTexture = UnPressedTexture;

            // pass in default shader parameters
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", () => Transform_Matrix));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.BaseMatrix)); // gets replaced when fixed to screen
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", () => EventManager.Program_Time));

            // pass in specific shader parameters
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "BorderTexture", () => TextureManager.Get(BorderTexture)));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "InsideTexture", () => TextureManager.Get(InsideTexture)));
            
            // compile shaders
            ShaderProgram.CompileProgram();

            // so it appears in front.
            Z_index = 5;
            // so it isnt moved by camera
            FixToScreen = true;

            // set transform
            this.Position = Position;
            this.Scale = Scale;

            // initiate clickbox
            clickbox = new ClickBox(new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) }, MouseButton.Button1, true, () => Transform_Matrix);
            
            Down = () => { }; // do nothing by default can be changed in derived class or outside object
            Release = () => { };
            
            clickbox.Click += OnClick; // set texture and call Down/release
            clickbox.UnClick += OnUnClick;




        }
        private void OnClick()
        {
            BorderTexture = PressedTexture;
            Down();
        }
        private void OnUnClick()
        {
            BorderTexture = UnPressedTexture;
            Release();
        }
    }
}
