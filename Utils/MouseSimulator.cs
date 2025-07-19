using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public static class PointExtensions
{
    public static SharpDX.Vector2 ToVector2(this Point point)
    {
        return new SharpDX.Vector2(point.X, point.Y);
    }
}
public static class MouseSimulator
{
    private const int MOUSEEVENTF_MOVE = 0x0001;
    private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const int MOUSEEVENTF_LEFTUP = 0x0004;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const int MOUSEEVENTF_RIGHTUP = 0x0010;

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public static void MoveMouseBezier(Point endPoint, int duration)
    {
        duration += 1000;
        Point startPoint = GetCurrentCursorPosition();
        Random rand = new Random();
        int width = Math.Abs(endPoint.X - startPoint.X);
        int height = Math.Abs(endPoint.Y - startPoint.Y);
        Point controlPoint1 = new Point(
            startPoint.X + (int)(rand.NextDouble() * width),
            startPoint.Y + (int)(rand.NextDouble() * height));
        Point controlPoint2 = new Point(
            startPoint.X + (int)(rand.NextDouble() * width),
            startPoint.Y + (int)(rand.NextDouble() * height));

        DateTime startTime = DateTime.Now;
        TimeSpan elapsedTime;
        double t;

        while ((elapsedTime = DateTime.Now - startTime).TotalMilliseconds < duration)
        {
            t = elapsedTime.TotalMilliseconds / duration;
            Point p = GetBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);

            p.Offset((int)GetGaussianRandom(rand), (int)GetGaussianRandom(rand));

            SetCursorPos(p.X, p.Y);
            Thread.Sleep(1); // Задержка между шагами
        }

        SetCursorPos(endPoint.X, endPoint.Y);
    }

    public static void MoveMouseBezier(SharpDX.Vector2 endPoint, int duration)
    {
        Point point = new Point((int)endPoint.X, (int)endPoint.Y);
        MoveMouseBezier(point, duration);
    }

    public static void LeftClick(Point position, int minDelay = 50, int maxDelay = 150)
    {
        MoveMouse(position);
        LeftDown();
        Thread.Sleep(GetRandomDelay(minDelay, maxDelay));
        LeftUp();
    }

    public static void RightClick(Point position, int minDelay = 50, int maxDelay = 150)
    {
        MoveMouse(position);
        RightDown();
        Thread.Sleep(GetRandomDelay(minDelay, maxDelay));
        RightUp();
    }


    public static void RightClick(int minDelay = 25, int maxDelay = 100)
    {        
        RightDown();
        Thread.Sleep(GetRandomDelay(minDelay, maxDelay));
        RightUp();
    }

    public static void LeftDown()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
    }

    public static void LeftUp()
    {
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    public static void RightDown()
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
    }

    public static void RightUp()
    {
        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
    }

    public static void MoveMouse(Point position)
    {
        SetCursorPos(position.X, position.Y);
    }

    public static Point GetCurrentCursorPosition()
    {
        if (!GetCursorPos(out POINT point))
        {
            throw new Exception("Failed to get cursor position.");
        }
        return new Point(point.X, point.Y);
    }

    private static Point GetBezierPoint(double t, Point p0, Point p1, Point p2, Point p3)
    {
        double u = 1 - t;
        double tt = t * t;
        double uu = u * u;
        double uuu = uu * u;
        double ttt = tt * t;

        Point p = new Point(
            (int)(uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X),
            (int)(uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y));

        return p;
    }

    private static double GetGaussianRandom(Random rand)
    {
        double u1 = rand.NextDouble();
        double u2 = rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return randStdNormal * 0.1; // Масштабируем для меньшего шума
    }

    public static int GetRandomDelay(int minDelay, int maxDelay)
    {
        Random rand = new Random();
        return rand.Next(minDelay, maxDelay);
    }
}