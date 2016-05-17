using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
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
            throw new NotImplementedException();
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
            float d = Vector3.Dot(position - r.Origin, normal) / Vector3.Dot(r.Direction, normal);
            return d;
        }
    }

}
