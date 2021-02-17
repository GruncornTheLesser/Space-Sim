using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace GameObjects
{
    class ClickBox
    {
        private Vector2[] HitBox;
        
        public bool fixtoscreen = false;
        private bool FixToScreen
        {
            set
            {
                fixtoscreen = value;
                if (fixtoscreen) Get_Cam_Transform = Window.Get_BaseMat;
                else Get_Cam_Transform = Window.Get_CamMat;
            } 
        }
        private Func<Matrix3> Get_Cam_Transform;
        private Func<Matrix3> Get_Obj_Transform;
        private Matrix3 Camera_Transform
        {
            get => Get_Cam_Transform();
        }
        private Matrix3 Object_Transform
        {
            get => Get_Obj_Transform();
        }

        public bool IsClicked = false;
        public MouseButton ActionButton;

        public Action Click;
        public Action UnClick;


        public ClickBox(Vector2[] HitBox, MouseButton ActionButton, bool fixedtoscreen, Func<Matrix3> Get_Transform_Matrix)
        {
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;
            
            this.Click = () => IsClicked = true;
            this.UnClick = () => IsClicked = false;

            this.Get_Obj_Transform = Get_Transform_Matrix;

            FixToScreen = fixedtoscreen;
            
            this.HitBox = HitBox;

            this.ActionButton = ActionButton;
            
        }
        private void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (PointInHitbox(Window.MouseToScreen(MouseState.Position)) && e.Button == ActionButton) Click();
        }
        private void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (IsClicked && e.Button == ActionButton) UnClick();
        }

        private float cross(Vector2 V1, Vector2 V2) => V1.X * V2.Y - V2.X * V1.Y;
        private bool PointInHitbox(Vector2 Position)
        {
            // assumes concave shape
            // transforms Position to origin
            Vector2 P = ((Camera_Transform * Object_Transform).Inverted() * new Vector3(Position.X, Position.Y, 1)).Xy;

            bool AllPositive = true, AllNegative = true;
            for (int i = 0; i < HitBox.Length; i++)
            {
                Vector2 V1, V2;
                float D;
                V1 = HitBox[i % HitBox.Length]; // first vertex
                V2 = HitBox[(i + 1) % HitBox.Length]; // second vertex
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
