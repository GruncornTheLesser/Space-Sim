using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Shaders;

namespace GameObjects
{
    /* THINGS TO DO:
     * button press event
     * button hover texture
     * slider button
     * toggle button
     */


   
    class Button : RenderObject2D<Vertex2D>
    {
        public delegate void ButtonPress(object sender);
        public ButtonPress ButtonPressed;

        public Button(Vector2 Scale, Vector2 Position, DeepCopy<Matrix3> CameraDeepCopy, DeepCopy<float> TimeCopy) : base(0, Scale, Position, Window.SquareMesh, CameraDeepCopy, TimeCopy, "Button", "Button", "Default") 
        {
            
        } 
        
        public override void Process(float delta) { }
        
        public override void OnMouseDown(MouseButtonEventArgs e) 
        {
            // NEEDS MOUSE POSITION
        }
        public override void OnMouseUp(MouseButtonEventArgs e) { }
        public override void OnMouseMove(MouseMoveEventArgs e) { }


        // could seperate off into a static geometry class
        static private float cross(Vector2 V1, Vector2 V2) => V1.X * V2.Y - V2.X* V1.Y;
        static public bool PointInPolygon(List<Vector2> Polygon, Vector2 P) 
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
