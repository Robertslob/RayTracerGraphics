using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application 
{
    public abstract class Primitive
    {
        public Material material;
        public Vector3 position;

        public Primitive(Material material, Vector3 position)
        {
            this.material = material;
            this.position = position;
        }

        //Returns lengte van ray r;
        public abstract float intersects(Ray r);
        public abstract Vector3 getNormal(Vector3 positionOnPrimitive);
        public abstract void debugOutput();
    }

    public class Sphere : Primitive
    {       
        public float radius;

        public Sphere(Vector3 position, float radius, Material material)
            : base(material, position)
        {            
            this.radius = radius;
        }

        public override float intersects(Ray r)
        {            
            float b = 2*(Vector3.Dot(r.Direction, (r.Origin - position)));
            float c = Vector3.Dot((r.Origin - position),(r.Origin - position)) - radius * radius;

            if (b * b - 4 * c > 0)
            {
                
                float cs = (float)Math.Sqrt((double)(b * b - 4*c));                
                float distance1 = -b + cs;
                float distance2 = -b - cs;
                return Math.Min(distance1, distance2) / 2;
            }
            else if (b * b - 4 * c == 0)
            {
                return -b / 2;
            }
            else
            {
                return int.MaxValue;
            }
        }

        public override void debugOutput()
        {
            GL.Begin(PrimitiveType.LineLoop);
            for (int i = 0; i <= 300; i++)
            {
                float angle = (float)(2 * Math.PI * i) / 300.0f;
                float x = (float)Math.Cos(angle) * radius;
                float y = (float)Math.Sin(angle) * radius;
                GL.Vertex2(x + position.X, y + position.Z);
            }
            GL.End();
        }
        public override Vector3 getNormal(Vector3 positionOnPrimitive)
        {
            return (positionOnPrimitive-position).Normalized();
        }
    }

    public class Plane : Primitive
    {
        Vector3 normal;
        public Plane(Vector3 normal, Vector3 position, Material material)
            : base(material,position)
        {
            this.normal = normal;
        }

        public override float intersects(Ray r)
        {
            float dotproduct = Vector3.Dot(r.Direction, normal);

            if (dotproduct != 0) {
                return Vector3.Dot(position - r.Origin, normal) / dotproduct;
            }
            return int.MaxValue;
        }

        public override void debugOutput()
        {            
        }
        public override Vector3 getNormal(Vector3 positionOnPrimitive)
        {
            return normal;
        }
    }

    //whitted-style ray tracer only has point light (in its basic form)
    public class Light
    {
        public Vector3 location;
        //color should be stored using floats according to the assignment 
        public Vector3 intensity;

        public Light(Vector3 location, Vector3 intensity)
        {
            this.location = location;
            this.intensity = intensity;
        }
    }

}
