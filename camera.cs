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
