using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application 
{
    public struct Material
    {
        public Vector3 color;
        float refraction;
        float reflection;

        public Material(Vector3 color, float refraction, float reflection)
        {
            this.color = color;
            this.refraction = refraction;
            this.reflection = reflection;
        }
        
    }
}
