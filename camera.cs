using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace template
{
    public class Camera
    {
        Vector cameraPosition;
        float upLeft, upRight, downLeft, downRight;
        // vector Direction?

        public Camera(Vector v)
        {
            cameraPosition = v;
            UpdatePlane();
        }

        public void UpdatePlane()
        {
            // hardcode the screen corners
        }
    }
}
