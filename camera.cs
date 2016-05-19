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
        // vector Direction?
        public Vector3 p1, p2, p3;
        public Vector3 up, left;
        public float distancePlane = 1.5f;
        public int maxViewingDistance = 9000;

        public Camera(Vector3 p, Vector3 direction)
        {
            position = p + 0.5f * Vector3.UnitY;
            this.direction = direction;
            UpdatePlane();
        }

        public void UpdatePlane()
        {
            // hardcode the screen corners
            up = Vector3.UnitY;
            left = Vector3.Cross(direction, up);
            up = Vector3.Cross(left, direction);
            //linksonder
            p1 = position + direction * distancePlane + left - up;
            //linksboven
            p2 = position + direction * distancePlane + left + up;
            //rechtsboven
            p3 = position + direction * distancePlane - left + up;
        }

        //Get ray from camera to 'pixel' on viewplane.
        public Ray getRay(int x, int y)
        {
            float div = 1 / 512.0f;
            Vector3 PoS = p2 + (x * div) * (p3 - p2) + (y * div) * (p1 - p2); //Kan niet zo he, die hoofdletters, waar staat het voor? pos normaal?
            return new Ray(position, (PoS - position).Normalized());  
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

        public Vector3 mirror(Vector3 normal)
        {
            Vector3 b = Vector3.Dot(Direction, normal) * normal;
            return Direction - 2 * b;
        }
        
    }
}
