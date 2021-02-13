using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameObjects
{
    class ClickBox
    {
        private Vector2[] HitBox;
        public bool IsClicked = false;
        public MouseButton ActionButton;

        public Action Click;
        public Action UnClick;

        private Func<Matrix3> Get_Transform_Matrix;
        public ClickBox(Vector2[] HitBox, MouseButton ActionButton, Func<Matrix3> Get_Transform_Matrix)
        {
            this.Click = () => IsClicked = true;
            this.UnClick = () => IsClicked = false;

            this.ActionButton = ActionButton;
            this.HitBox = HitBox;
            this.Get_Transform_Matrix = Get_Transform_Matrix;
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;
        }
        public void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (PointInPolygon(Window.MouseToScreen(MouseState.Position)) && e.Button == ActionButton) Click();
        }
        public void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (IsClicked && e.Button == ActionButton) UnClick();
        }

        private float cross(Vector2 V1, Vector2 V2) => V1.X * V2.Y - V2.X * V1.Y;
        private bool PointInPolygon(Vector2 P)
        {

            // assumes concave shape
            bool AllPositive = true, AllNegative = true;
            for (int i = 0; i < HitBox.Length; i++)
            {
                Vector2 V1, V2;
                float D;
                V1 = (Get_Transform_Matrix() * new Vector3(HitBox[i % HitBox.Length].X, HitBox[i % HitBox.Length].Y, 1)).Xy; // first vertex
                V2 = (Get_Transform_Matrix() * new Vector3(HitBox[(i + 1) % HitBox.Length].X, HitBox[(i + 1) % HitBox.Length].Y, 1)).Xy; // second vertex
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
