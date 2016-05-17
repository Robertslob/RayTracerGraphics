using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK;
using Application;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Application
{
    public class Camera
    {

        public Vector3 position;
        public Vector3 direction;
        // vector Direction?
        public Vector3 p1, p2, p3;
        public Vector3 up, left;
        public int distancePlane = 10;
        public int maxViewingDistance = 9000;

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
            p1 = position + direction * distancePlane + left - up;
            p2 = position + direction * distancePlane + left + up;
            p3 = position + direction * distancePlane - left + up;
        }

        
        public void debugOutput()
        {

            Console.WriteLine("[pos: '" + position + "', dir: '" + direction + "'");

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m = Matrix4.CreateScale(1 / 32.0f);
            GL.LoadMatrix(ref m);
            GL.Color3(0.8f, 0.3f, 0.3f);



           

            //Draw the camera
            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex2((position.Xz + left.Xz));
            GL.Vertex2((position.Xz + direction.Xz));
            GL.Vertex2((position.Xz - left.Xz));
            GL.End();
            //Draw camera end

            Vector2 sv1 = (position.Xz - p1.Xz) * -30;
            Vector2 sv2 = (position.Xz - p3.Xz) * -30;

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(position.Xz);
            GL.Vertex2((position.Xz + sv1));

            GL.Vertex2(position.Xz);
            GL.Vertex2((position.Xz + sv2));

            GL.Color3(0.4f, 1.0f, 0.4f);
            GL.Vertex2(p1.Xz);
            GL.Vertex2(p3.Xz);

            
            
            GL.End();

            Sphere p = new Sphere(Vector3.UnitZ * 8f, 5f, new Material());
            p.debugOutput();
            Sphere sphere = new Sphere(new Vector3(20, 0, 30), 29f, new Material());
            sphere.debugOutput();

            Vector3 viewPlaneXZ = (p3 - p2).Normalized();
            
            GL.Color3(0.5f, 0.5f, 0.5f);
            
            for (int i = 0; i < 30; i++)
            {
                Vector3 rd = ((p2 + (viewPlaneXZ * ((i) / 15.0f))) - position);

                rd.Y = 0;
                rd.Normalize();
                
                Ray r = new Ray(position, rd);
                float dist = Math.Min(p.intersects(r), sphere.intersects(r));                
                
                GL.Begin(PrimitiveType.Lines);

                if (dist > 0)
                {
                    GL.Vertex2(position.Xz);
                    GL.Vertex2(position.Xz + rd.Xz * dist);
                }
                
                GL.End();
            }
        }        
    }
    

    public class Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public int depth; //Voor weerkaatsing enzo.

        public Ray(Vector3 position, Vector3 direction)
        {
            this.Origin = position;
            this.Direction = direction;
        }
        
    }
}
