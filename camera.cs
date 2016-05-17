using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK;

namespace template
{
    public class Camera
    {
        Vector3 cameraPosition;
        Vector3 position;
        Vector3 direction;
        // vector Direction?
        Vector3 p1, p2, p3;

        public Camera(Vector3 v)
        {
            cameraPosition = v;
            UpdatePlane();
        }

        public void UpdatePlane()
        {
            // hardcode the screen corners
            Vector3 up = Vector3.UnitZ;
            Vector3 left = Vector3.Cross(direction, up);
            up = Vector3.Cross(left, direction);

            int dist = 123;
            p1 = direction * dist + left - up;
            p2 = direction * dist + left + up;
            p3 = direction * dist - left + up;

        }

    }
}
