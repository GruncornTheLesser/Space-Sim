using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using GameObjects;
using Graphics;
using Graphics.Shaders;
using System;
using System.IO;

namespace Space_Sim
{
    static class Program
    {
        static void Main()
        {
            
            var GWS = new GameWindowSettings();
            var NWS = new NativeWindowSettings();


            
            NWS.Size = new Vector2i(800, 800);

            using (SpaceSimWindow Sim = new SpaceSimWindow(GWS, NWS))
            {
                Sim.Run();
            }
        }
    }
}

/*
            using (RenderWindow R = new RenderWindow(GWS, NWS))
            {

                R.Camera = new Camera(R.EventManager, R.Size, 1, 300, 0.2f); // for moving camera

                // render tests

                // test 1
                //RenderObject2D R1 = new TestObj(R, new Vector2(-125, 0), "1", 1);
                //RenderObject2D R2 = new TestObj(R, new Vector2(-75, 0), "2", 2);
                //RenderObject2D R3 = new TestObj(R, new Vector2(-25, 0), "3", 3);
                //RenderObject2D R4 = new TestObj(R, new Vector2(25, 0), "4", 4);
                //RenderObject2D R5 = new TestObj(R, new Vector2(75, 0), "5", 5);

                // test 2, 3
                //R3.Visible = false;
                //R3.Visible = true;

                // test 4
                //Vertex2D[] VA = R1.VertexArray;

                //R1.VertexArray = VA;

                // test 4, 5, 6
                //Vertex2D[] VA = new Vertex2D[5]; // 4
                //Vertex2D[] VA = new Vertex2D[5]; // 5
                //Vertex2D[] VA = new Vertex2D[0]; // 6

                // for (int i = 0; i < VA.Length; i++) VA[i] = R1.VertexArray[i];

                //VA[1] = new Vertex2D(-2, -2, 0, 0, 1, 1, 1, 1); // 4
                //R1.VertexArray = VA;

                //test 7

                //R.EventManager.System_Process += (delta) => R.Title =
                //$"MousePos: " +
                //$"{MathF.Round(R.MouseState.Position.X, 2)}," +
                //$"{MathF.Round(R.MouseState.Position.Y, 2)} " +

                //$"MouseScreenPos: " +
                //$"{MathF.Round(R.MouseToScreen(R.MouseState.Position).X, 2)}," +
                //$"{MathF.Round(R.MouseToScreen(R.MouseState.Position).Y, 2)} " +

                //$"MouseWorldPos: " +
                //$"{MathF.Round(R.MouseToWorld(R.MouseState.Position).X, 2)}," +
                //$"{MathF.Round(R.MouseToWorld(R.MouseState.Position).Y, 2)} " +

                //$"FPS: {1 / delta : 0} ";

                //R5.FixToScreen = true;
                //R5.Position = new Vector2(0, 0);
                //R5.Scale = new Vector2(0.5f, 0.5f);


                // UI tests
                // test 1 & 2
                
                //ClickBox CB1 = new ClickBox(R, () => R1.Transform_Matrix, R1.FixToScreen);
                //CB1.Click += () =>
                //{
                    //string text = File.ReadAllText($"Test/Log file.txt");
                    //text += $"Clicked R1 at {R.MouseToWorld(R.MouseState.Position)}{Environment.NewLine}";
                    //File.WriteAllText($"Test/Log file.txt", text);                   
                //};
                
                //ClickBox CB2 = new ClickBox(R, () => R3.Transform_Matrix, R3.FixToScreen);
                //CB2.Click += () =>
                //{
                    //string text = File.ReadAllText($"Test/Log file.txt");
                    //text += $"Clicked R3 at {R.MouseToWorld(R.MouseState.Position)}{Environment.NewLine}";
                    //File.WriteAllText($"Test/Log file.txt", text);

                //};
                
                // test 3
                //R.EventManager.System_Process += (delta) => R.Title = $"Zoom: {R.Camera.Zoom}, Scale: {R.Camera.Scale}";

                // test 4
                //SliderButton SB = new SliderButton(R, Vector2.Zero, new Vector2(2.0f, 0.1f));

                // test 5
                //PressButton B1 = new PressButton(R, new Vector2(0, 0), new Vector2(0.5f, 0.5f), "Button_test");
                //PressButton B2 = new PressButton(R, new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), "Button_test");

                // test 6

                //ClickBox CB1 = new ClickBox(R, () => R1.Transform_Matrix, R1.FixToScreen);
                //CB1.Click += () =>
                //{
                //    string text = File.ReadAllText($"Test/Log file.txt");
                //    string click = "double";
                //    if (CB1.Time_Since_Last_Call > 0.2) click = "single";
                //     text += $"{click} click at Pos: {R.MouseToWorld(R.MouseState.Position)}, Time: {R.EventManager.System_Time}{Environment.NewLine}";
                //    File.WriteAllText($"Test/Log file.txt", text);
                //};

                //R.Run();
            }
            
        }


    }
    internal class TestObj : RenderObject2D
    {
        public TestObj(RenderWindow R, Vector2 Position, string path, int Z_index) : base(R, SquareMesh, "Default", "Default")
        {
            this.Position = Position;
            this.Scale = new Vector2(100f, 100f);
            this.Z_index = Z_index;

            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", () => TextureManager.Get($"Test/{path}.png")));
            ShaderProgram.CompileProgram();
        }
    }
}
*/


