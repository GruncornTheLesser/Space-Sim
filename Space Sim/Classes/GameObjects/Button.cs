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

namespace GameObjects
{
    /* THINGS TO DO:
     * button press event
     * button hover texture
     * slider button
     * toggle button
     */


    /*
    class Button : RenderObject2D<Vertex2D>
    {
        List<Vector2> HitBoxMesh;
        private delegate void ButtonPress(object sender);
        event ButtonPress ButtonPressEvent;

        public Button(Vector2 Scale, Vector2 Position) : base(0, Scale, Position, Window.SquareMesh, "Button", "Button", "Default") 
        {
            FixToScreenSpace = true;
        } 
        public override void OnMouseDown(MouseButtonEventArgs e) { }
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
                Vector2 T1, T2;
                float D;
                T1 = Polygon[i % Polygon.Count];
                T2 = Polygon[(i + 1) % Polygon.Count];
                D = cross(P, T1) + cross(T1, T2) + cross(T2, P);
                
                if (D > 0) AllNegative = false;
                if (D < 0) AllPositive = false;
            }
            if (AllPositive || AllNegative) return true;
            else return false;
        }
    }
    */
}
