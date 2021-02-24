using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Shaders;

namespace GameObjects
{
    /* Thing To Do:
     * Update by distance travelled?
     */
    /// <summary>
    /// Trail is render object that takes a transform and draws a line from the previous position to the current position.
    /// </summary>
    class Trail : RenderObject2D
    {
        private Queue<Vertex2D> VerticeQueue;
        private Func<Vertex2D> GetVertex;
        private readonly int UpdateTime = 100;

        /// <summary>
        /// Constructs a trail
        /// </summary>
        /// <param name="Capacity">the number of vertices this object uses. Higher capacity makes longer lines.</param>
        /// <param name="UpdateTime">the number of milliseconds between position updates. Smaller update times increases smoothness. 0 processes every frame.</param>
        /// <param name="GetNewPosition">A delegate to get the transform position of the object this trails behind.</param>
        public Trail(int Capacity, int UpdateTime, Func<Vector2> GetNewPosition, Color4 Colour) : base(new Vertex2D[] { }, "Line", "Line")
        {
            this.GetVertex = () => new Vertex2D(GetNewPosition(), Vector2.Zero, Color4.WhiteSmoke);
            this.UpdateTime = UpdateTime;
            this.Z_index = -1;

            RenderingType = PrimitiveType.LineStrip; // makes the render object render as a line
            PolygonMode = PolygonMode.Line; // area to fill so unneccessary to fill

            // initiates queue with starting position of planets
            Vertex2D StartVertex = GetVertex();
            VerticeQueue = new Queue<Vertex2D>(Capacity);
            for (int i = 0; i < Capacity; i++) VerticeQueue.Enqueue(StartVertex);

            // line shader has only 1 uniform -> the camera matrix.
            // a trainsform matrix isnt used because the line transform has no bearing on the vertex positions.
            // the vertex positions have already been transformed.
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.CompileProgram();
            
            if (UpdateTime == 0) EventManager.Program_Process += UpdateQueueImmediate; // update every frame(as fast as possible)
            else EventManager.Program_Process += UpdateQueueLoop; // start loop
        }
        public void UpdateQueueImmediate(float delta)
        {
            VerticeQueue.Dequeue(); // remove old position.
            VerticeQueue.Enqueue(GetVertex()); // add new position

            VertexArray = VerticeQueue.ToArray(); // update vertex array
        }
        /// <summary>
        /// changes the vertex array to render a new line shape
        /// </summary>
        /// <param name="delta">time passed on this update. Has no bearing on trail update.</param>
        private void UpdateQueueLoop(float delta)
        {
            VerticeQueue.Dequeue(); // remove old position.
            VerticeQueue.Enqueue(GetVertex()); // add new position

            VertexArray = VerticeQueue.ToArray(); // update vertex array

            EventManager.Program_Process -= UpdateQueueLoop; // remove from regular update
            Task.Delay(UpdateTime).ContinueWith(ID => EventManager.Program_Process += UpdateQueueLoop); // wait Update Time the add Update queue back to Process
        }
    }
}
