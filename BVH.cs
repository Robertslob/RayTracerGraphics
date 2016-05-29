using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application 
{
    public struct AABB
    {
        public Vector3 minPoint, maxPoint;

        public bool intersect(Ray r)
        {
            //pretty costy and should really be stored for a ray
            float invX = 1.0f / r.Direction.X;
            float invY = 1.0f / r.Direction.Y;
            float invZ = 1.0f / r.Direction.Z;

            float minX = (minPoint.X - r.Origin.X) * invX;
            float maxX = (maxPoint.X - r.Origin.X) * invX;
            float minY = (minPoint.Y - r.Origin.Y) * invY;
            float maxY = (maxPoint.Y - r.Origin.Y) * invY;
            float minZ = (minPoint.Z - r.Origin.Z) * invZ;
            float maxZ = (maxPoint.Z - r.Origin.Z) * invZ;

            float tmin = Math.Max(  Math.Max( Math.Min(minX, maxX), Math.Min(minY, maxY)), Math.Min(minZ, maxZ) );
            float tmax = Math.Min(  Math.Min( Math.Max(minX, maxX), Math.Max(minY, maxY)), Math.Max(minZ, maxZ) );

            return tmax >= tmin;
        }
    }

}
