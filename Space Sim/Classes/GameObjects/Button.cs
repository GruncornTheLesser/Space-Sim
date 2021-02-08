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
        private bool Pressed = false;

        public Button() : base() 
        {
            
        } 
        
        public override void OnProcess(float delta) { }

        public override void OnMouseDown(MouseState MouseState) 
        {
        }
        public override void OnMouseUp(MouseState MouseState) { }
        public override void OnMouseMove(MouseState MouseState) { }
        public override void OnMouseWheel(MouseState MouseState) { }


        // could seperate off into a static geometry class
        static private float cross(Vector2 V1, Vector2 V2) => V1.X * V2.Y - V2.X* V1.Y;
        static private bool PointInPolygon(List<Vector2> Polygon, Vector2 P) 
        {
            // assumes concave shape
            bool AllPositive = true, AllNegative = true;
            for (int i = 0; i < Polygon.Count; i++)
            {
                Vector2 V1, V2;
                float D;
                V1 = Polygon[i % Polygon.Count]; // first vertex
                V2 = Polygon[(i + 1) % Polygon.Count]; // second vertex
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
