using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Application;

namespace Application
{
    public class Camera
    {
        public Vector3 position;
        public Vector3 direction;
        public Vector3 p1, p2, p3;
        public Vector3 up, left;
        public float distancePlane = 1.5f;
        public int maxViewingDistance = 9000;
        // We can maken the field of view smaller or larger with this factor
        public float viewDegree = 1.0f;             

        public Camera(Vector3 p, Vector3 direction)
        {
            position = p + 0.5f * Vector3.UnitY;
            this.direction = direction;
            UpdatePlane();
        }

        public void UpdatePlane()
        {
            up = Vector3.UnitY;
            left = Vector3.Cross(direction, up);
            up = Vector3.Cross(left, direction);

            left.Normalize();
            up.Normalize();
            //linksonder
            p1 = position + (direction * distancePlane) + left * viewDegree - up;
            //linksboven
            p2 = position + (direction * distancePlane) + left * viewDegree + up;
            //rechtsboven
            p3 = position + (direction * distancePlane) - left * viewDegree + up;
        }

        //Get ray from camera to 'pixel' on viewplane.
        float div = 1 / (float)Raytracer.WIDTH;
        public Ray getRay(int x, int y) 
        {
            
            Vector3 pos = p2 + (x * div) * (p3 - p2) + (y * div) * (p1 - p2);
            return new Ray(position, (pos - position).Normalized());  
        }
    }
    
    // The ray Class
    public class Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public int depth;
        public float refractionIndex = 1.0f;

        public Ray(Vector3 position, Vector3 direction)
        {
            this.Origin = position;
            this.Direction = direction;
        }

        public Vector3 mirror(Vector3 normal)
        {
            Vector3 b = Vector3.Dot(Direction, normal) * normal;
            return Direction - 2 * b;
        }
        
    }
}
