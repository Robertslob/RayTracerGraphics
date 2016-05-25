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
            this.refractionIndex = 0.8f;
        }

        public Vector3 getpatternColor(Vector3 dest)
        {
            if(hasPattern){
                //prevent rounding of int problem at 0 (-1 < x < 1 are all considered even)
                if (dest.X < 0)
                {
                    dest.X -= 1;
                }
                if (dest.Z < 0)
                {
                    dest.Z -= 1;
                }

                //checkboard pattern
                if ((int)dest.X % 2 == 0)
                {
                    if ((int)dest.Z % 2 == 0)
                    {
                        return (new Vector3(1, 1, 1) - this.color);
                    }
                    else
                    {
                        return color;
                    }
                }
                else
                {
                    if ((int)dest.Z % 2 == 0)
                    {
                        return color;
                    }
                    else
                    {
                        return (new Vector3(1, 1, 1) - this.color);                        
                    }
                }
            }
            else
            {
                return color;
            }
        }
        
    }
}
