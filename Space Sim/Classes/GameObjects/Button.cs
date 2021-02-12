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
    /* THINGS TO DO:
     * Needs access to world coordinates maybe use static class?
     */
    public delegate void ButtonPress(object sender, Vector2 MousePosition, MouseButtonEventArgs e);


    class Button : RenderObject2D
    {
        
        private Vector2[] HitBox = {
            new Vector2(-1, 1),
            new Vector2(1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1)
            };

        private int PressedTexure;
        private int UnPressedTexture;
        private int CurrentTexture;

        private bool button_down = false;
        public bool Button_Down
        {
            get => button_down;
            private set
            {
                button_down = value;
                if (button_down) CurrentTexture = PressedTexure;
                else CurrentTexture = UnPressedTexture;
            }
        }


        public Action ButtonPress;
        public Action ButtonRelease;
        public MouseButton ActionButton = MouseButton.Button1;

        public Button() : base() 
        {
            PressedTexure = Init_Textures("Button textures/Button_Pressed");
            UnPressedTexture = Init_Textures("Button textures/Button_UnPressed");
            CurrentTexture = UnPressedTexture;

            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", TransformCopy));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", Window.CameraCopy));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", EventManager.TimeCopy));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", new DeepCopy<int>(() => CurrentTexture)));

            ShaderProgram.CompileProgram();

            ButtonPress = () => Button_Down = true;
            ButtonRelease = () => Button_Down = false;

            Z_index = 10;
            Scale = new Vector2(0.1f, 0.1f);
            Position = new Vector2(0.9f, 0.9f);
            FixToScreen = true;

            for(int i = 0; i < HitBox.Length; i++)
            {
                HitBox[i] = (Transform_Matrix * new Vector3(HitBox[i].X, HitBox[i].Y, 1)).Xy;
            }

            
        } 
        
        public override void OnProcess(float delta) { }

        public override void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e) 
        {
            if (PointInPolygon(Window.MouseToScreen(MouseState.Position), HitBox) && e.Button == ActionButton) 
                ButtonPress();
        }
        public override void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e) 
        {
            if (button_down && e.Button == ActionButton) ButtonRelease();
        }
        public override void OnMouseMove(MouseState MouseState, MouseMoveEventArgs e) { }
        public override void OnMouseWheel(MouseState MouseState, MouseWheelEventArgs e) { }


        // could seperate off into a static geometry class
        static private float cross(Vector2 V1, Vector2 V2) => V1.X * V2.Y - V2.X* V1.Y;
        static private bool PointInPolygon(Vector2 P, Vector2[] Polygon) 
        {
            // assumes concave shape
            bool AllPositive = true, AllNegative = true;
            for (int i = 0; i < Polygon.Length; i++)
            {
                Vector2 V1, V2;
                float D;
                V1 = Polygon[i % Polygon.Length]; // first vertex
                V2 = Polygon[(i + 1) % Polygon.Length]; // second vertex
                // V3 = point
                D = cross(P, V1) + cross(V1, V2) + cross(V2, P); // the determinate of the triangle V1, V2, V3
                
                if (D > 0) AllNegative = false;
                if (D < 0) AllPositive = false;
            }
            // depending on the winding of the triangle clockwise or anti clockwise
            // point is inside the triangle if all determinates are positive or all are negative
            if (AllPositive || AllNegative) return true;
            else return false;
        }
    }
    
}
