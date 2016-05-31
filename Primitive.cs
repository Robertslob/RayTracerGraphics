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
        public AABB box;

        public Primitive(Material material, Vector3 position)
        {
            this.material = material;
            this.position = position;
        }        

        //Returns lengte van ray r;
        public abstract float intersects(Ray r);
        //Returns normal
        public abstract Vector3 getNormal(Vector3 positionOnPrimitive);
        //Causes the debugOutput
        public abstract void debugOutput();
    }

    public class Sphere : Primitive
    {       
        public float radius;

        public Sphere(Vector3 position, float radius, Material material)
            : base(material, position)
        {            
            this.radius = radius;
            box.minPoint = position - new Vector3(radius, radius, radius);
            box.maxPoint = position + new Vector3(radius, radius, radius);
        }

        public override float intersects(Ray r)
        {         
            float b = 2*(Vector3.Dot(r.Direction, (r.Origin - position)));
            float c = Vector3.Dot((r.Origin - position),(r.Origin - position)) - radius * radius;

            if (b * b - 4 * c > 0)
            {
                float cs = (float)Math.Sqrt((double)(b * b - 4 * c));
                float distance1 = -b + cs;
                float distance2 = -b - cs;
                distance2 = (distance2 < 0) ? distance1 : distance2;
                
                return Math.Min(distance1, distance2) / 2.0f;
            }
            else if (b * b - 4 * c == 0)
            {
                return -b / 2.0f;
            }
            else
            {
                return int.MaxValue;
            }
        }

        // An early out, not used anymore because it doesn't work with refraction
        /*
        public bool canIntersect(Ray r)
        {
            Vector3 c = position - r.Origin;
            float t = Vector3.Dot(c, r.Direction);
            Vector3 q = c - t * r.Direction;
            float p2 = Vector3.Dot(q, q);
            if (p2 > radius)
                return false;
            return true;
        }*/

        public override void debugOutput()
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color3(material.color);
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
            this.normal = normal.Normalized();
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

    public class Triangle : Primitive
    {
        Vector3 position2, position3;
        Vector3 normal;

        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 norm1, Vector3 norm2, Vector3 norm3, Material material)
            : base(material, pos1)
        {
            position2 = pos2;
            position3 = pos3;
            normal = (norm1 + norm2 + norm3) * 0.333333f;
        }
        public Triangle(Vector3 position, Vector3 pos2, Vector3 pos3, Material material)
            : base(material, position)
        {
            position2 = pos2;
            position3 = pos3;
            normal = Vector3.Cross((position2 - position), (position3 - position));
            normal.Normalize();


            box.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            box.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            //set minimum and maximum of the primitive each dimension at a time
            for (int i = 0; i < 3; i++)
            {
                box.minPoint[i] = Math.Min(position[i], box.minPoint[i]);
                box.minPoint[i] = Math.Min(position2[i], box.minPoint[i]);
                box.minPoint[i] = Math.Min(position3[i], box.minPoint[i]);

                box.maxPoint[i] = Math.Max(position[i], box.maxPoint[i]);
                box.maxPoint[i] = Math.Max(position2[i], box.maxPoint[i]);
                box.maxPoint[i] = Math.Max(position3[i], box.maxPoint[i]);
            }
        }

        // Draw the 3 edges of the Triangle
        public override void debugOutput()
        {
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(material.color);
            GL.Vertex2(position.Xz);
            GL.Vertex2(position2.Xz);
            GL.Vertex2(position3.Xz);
            GL.Vertex2(position.Xz);
      
            GL.End();
        }

        // Möller–Trumbore https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
        public override float intersects(Ray r)
        {
            float EPSILON = 0.000001f;

            Vector3 e1 = position2 - position;
            Vector3 e2 = position3 - position;

            Vector3 P = Vector3.Cross(r.Direction, e2);
            float det = Vector3.Dot(e1, P);

            if (det > -EPSILON && det < EPSILON)
                return 0.0f;
            float invDet = 1.0f / det;

            Vector3 T = r.Origin - position;

            float u = Vector3.Dot(T, P) * invDet;
            if (u < 0.0f || u > 1.0f) return 0.0f;

            Vector3 Q = Vector3.Cross(T, e1);

            float v = Vector3.Dot(r.Direction, Q) * invDet;
            if (v < 0.0f || v + u > 1.0f) return 0.0f;

            float t = Vector3.Dot(e2, Q) * invDet;

            if (t > EPSILON)
                return t;

            return 0;
        }

        public override Vector3 getNormal(Vector3 positionOnPrimitive)
        {            
            return -normal;
        }

        public Tuple<float, float> getUV(Ray r)
        {
            Vector3 e1 = position2 - position;
            Vector3 e2 = position3 - position;

            Vector3 P = Vector3.Cross(r.Direction, e2);
            float det = Vector3.Dot(e1, P);            
            float invDet = 1.0f / det;

            Vector3 T = r.Origin - position;
            Vector3 Q = Vector3.Cross(T, e1);

            float u = Vector3.Dot(T, P) * invDet;          
            float v = Vector3.Dot(r.Direction, Q) * invDet;

            return new Tuple<float, float>(u, v);
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
