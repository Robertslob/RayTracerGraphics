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
        public float refraction;
        public float reflection;
        public float refractionIndex;
        private bool hasPattern;

        public Material(Vector3 color, float refraction, float reflection, bool pattern)
        {
            this.color = color;
            this.refraction = refraction;
            this.reflection = reflection;
            this.hasPattern = pattern;
            this.refractionIndex = 1.3f;
        }

        public Vector3 getpatternColor(Vector3 dest)
        {
            if(hasPattern){                
                int i = ((int)dest.X + ((int)dest.Z)) & 1;                
                return new Vector3(i, i, i);
            }
            else
            {
                return color;
            }
        }
        
    }
}
