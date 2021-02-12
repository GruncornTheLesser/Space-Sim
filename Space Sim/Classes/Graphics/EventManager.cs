using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Input;
using OpenTK.Mathematics;
using Graphics;
using DeepCopy;

namespace  Graphics
{
    public static class EventManager
    {
        internal static float Time = 0;
        internal static readonly DeepCopy<float> TimeCopy = new DeepCopy<float>(() => Time);

        internal static Action<MouseState, MouseButtonEventArgs> MouseDown = (M, e) => { };
        internal static Action<MouseState, MouseButtonEventArgs> MouseUp = (M, e) => { };
        internal static Action<MouseState, MouseWheelEventArgs> MouseWheel = (M, e) => { };
        internal static Action<MouseState, MouseMoveEventArgs> MouseMove = (M, e) => { };
        internal static Action<float> Process = (delta) => Time += delta;
    }
}
