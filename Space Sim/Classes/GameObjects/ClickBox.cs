using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace GameObjects
{
    /// <summary>
    /// A box defined by n vertices which checks for clicks inside this box
    /// </summary>
    class ClickBox
    {
        private Vector2[] HitBox;
        
        
        public bool FixToScreen
        {
            set
            {
                fixtoscreen = value;
                if (fixtoscreen) Get_Cam_Transform = () => RenderWindow.Camera.BaseMatrix;
                else Get_Cam_Transform = () => RenderWindow.Camera.Transform_Matrix;
            } 
        }
        private bool fixtoscreen = false;
        private Matrix3 Camera_Transform
        {
            get => Get_Cam_Transform();
        }
        private Func<Matrix3> Get_Cam_Transform;
        private Matrix3 Object_Transform
        {
            get => Get_Obj_Transform();
        }
        private Func<Matrix3> Get_Obj_Transform;


        private float Time_of_Last_Call = float.MinValue;
        public float Time_Since_Last_Call = float.MaxValue;
        public bool IsClicked = false;
        public MouseButton ActionButton;

        public Action Click;
        public Action UnClick;
        

        public ClickBox(Vector2[] HitBox, MouseButton ActionButton, bool fixedtoscreen, Func<Matrix3> Get_Transform_Matrix)
        {
            // attach events
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;

            this.Get_Obj_Transform = Get_Transform_Matrix;
            this.FixToScreen = fixedtoscreen;
            this.HitBox = HitBox;
            this.ActionButton = ActionButton;

            this.Click = () =>
            {
                Time_Since_Last_Call = EventManager.System_Time - Time_of_Last_Call;
                Time_of_Last_Call = EventManager.System_Time;
                IsClicked = true;
            };
            this.UnClick = () => IsClicked = false;

        }
        public ClickBox(Func<Matrix3> Get_Transform_Matrix, bool FixToScreen)
        {
            // attach events
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;

            this.Get_Obj_Transform = Get_Transform_Matrix;
            this.FixToScreen = FixToScreen;
            this.HitBox = new Vector2[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) };
            this.ActionButton = MouseButton.Button1;

            this.Click = () =>
            {
                Time_Since_Last_Call = EventManager.System_Time - Time_of_Last_Call;
                Time_of_Last_Call = EventManager.System_Time;
                IsClicked = true;
            };
            this.UnClick = () => IsClicked = false;
        }
        private void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (PointInHitbox(RenderWindow.MouseToScreen(MouseState.Position)) && e.Button == ActionButton) Click();
        }
        private void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (IsClicked && e.Button == ActionButton) UnClick();
        }

        private static float cross(Vector2 V1, Vector2 V2) => V1.X * V2.Y - V2.X * V1.Y;
        private bool PointInHitbox(Vector2 Position)
        {
            // assumes concave shape
            // untransforms Position to origin
            Vector2 P = ((Camera_Transform * Object_Transform).Inverted() * new Vector3(Position.X, Position.Y, 1)).Xy;

            // checks if point is inside hitbox
            bool AllPositive = true, AllNegative = true;
            for (int i = 0; i < HitBox.Length; i++)
            {
                Vector2 V1 = HitBox[i % HitBox.Length]; // first vertex
                Vector2 V2 = HitBox[(i + 1) % HitBox.Length]; // second vertex

                float D = cross(P, V1) + cross(V1, V2) + cross(V2, P); // the determinate of the triangle V1, V2, V3

                if (D > 0) AllNegative = false;
                if (D < 0) AllPositive = false;
            }
            // depending on the winding of the triangle clockwise or anti clockwise
            // point is inside the triangle if all determinates are positive or all are negative
            // http://totologic.blogspot.com/2014/01/accurate-point-in-triangle-test.html -> 3rd method
            if (AllPositive || AllNegative) return true;
            else return false;
        }
    }
}
