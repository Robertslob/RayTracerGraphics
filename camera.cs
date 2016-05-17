using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK;
using Template;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace template
{
    public class Camera
    {

        Vector3 position;
        Vector3 direction;
        // vector Direction?
        Vector3 p1, p2, p3;
        Vector3 up, left;
        int distancePlane = 10;

        public Camera(Vector3 p, Vector3 direction)
        {
            position = p;
            this.direction = direction;
            UpdatePlane();
        }

        public void UpdatePlane()
        {
            // hardcode the screen corners
            up = Vector3.UnitY;
            left = Vector3.Cross(direction, up);
            up = Vector3.Cross(left, direction);

            
            p1 = direction * distancePlane + left - up;
            p2 = direction * distancePlane + left + up;
            p3 = direction * distancePlane - left + up;
        }

        public void cameraMovement()
        {
            var state = OpenTK.Input.Keyboard.GetState();
            Matrix4 m = Matrix4.CreateRotationY(0.1f);
            Matrix4 m2 = Matrix4.CreateRotationY(-0.1f);
            if (state[Key.Q])
            {
                direction = Vector3.Transform(direction, m);
            }
            if (state[Key.E])
            {
                direction = Vector3.Transform(direction, m2);
            }
            if (state[Key.R])
            {
                distancePlane++;
            }
            if (state[Key.F])
            {

                distancePlane--;
                if (distancePlane < 1) distancePlane = 1;
            }
            direction.Normalize();
            UpdatePlane();
        }
        public void debugOutput()
        {
            cameraMovement();
            Console.WriteLine("[pos: '" + position + "', dir: '" + direction + "'");
            GL.Color3(0.8f, 0.3f, 0.3f);

            float r = 1 / 32.0f;
            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex2((position.Xz + left.Xz) * r);
            GL.Vertex2((position.Xz + direction.Xz) * r);
            GL.Vertex2((position.Xz - left.Xz) * r);
            GL.End();

            Vector2 svx = (position.Xz - p1.Xz) * -30;
            Vector2 svy = (position.Xz - p3.Xz) * -30;
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(position.Xz * r);
            GL.Vertex2((position.Xz * r+ svx));
            GL.Vertex2(position.Xz * r);
            GL.Vertex2((position.Xz * r + svy));
            GL.Color3(0.4f, 1.0f, 0.4f);
            GL.Vertex2(p1.Xz * r);
            GL.Vertex2(p3.Xz * r);
            GL.End();

        }
    }
}
