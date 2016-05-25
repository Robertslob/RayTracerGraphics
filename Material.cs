using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application 
{
    public class Material
    {
        public Vector3 color;
        public float refraction;
        public float reflection;
        public float refractionIndex;
        private bool hasPattern;        
        private Surface surface;
        private static Vector3[] hdr = new Vector3[1];

        public Material(Vector3 color, float refraction, float refractionIndex, float reflection, bool pattern)
        {
            this.color = color;
            this.refraction = refraction;
            this.reflection = reflection;
            this.hasPattern = pattern;
            this.refractionIndex = refractionIndex;  
        }

        public Material(String file, float reflection)
        {
            this.reflection = reflection;
            this.hasPattern = true;
            surface = new Surface(file);
        }

        public static void loadHdr(String file, int width)
        {
            if (hdr.Length > 1) return;
            BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open));
            hdr = new Vector3[width * width];
            for (int i = 0; i < hdr.Length; i++)
            {                
                hdr[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());                
            }            
            reader.Close();
        }

        public Vector3 getSphereColor(Vector3 dest, Vector3 sphere)
        {
            if (hasPattern)
            {
                Vector3 d = (dest - sphere).Normalized();

                float f = 1 / (float)Math.PI;
                float u = 0.5f + (float)Math.Atan2(d.Z, d.X) * 0.5f * f;
                float v = 0.5f - (float)Math.Asin(d.Y) * f;

                int x = (int)(u * surface.width);
                int y = (int)(v * surface.height);
                int c = surface.pixels[x + y * surface.width];

                int r = (c >> 16) & 0xff;
                int g = (c >> 8) & 0xff;
                int b = (c) & 0xff;

                float div = 1/255.0f;
                return new Vector3(r * div, g * div, b * div);
                //return getHemiSphereColor(dest, sphere);
            }
            else
            {
                return color;
            }
        }

        public static Vector3 getHemiSphereColor(Vector3 dest, Vector3 sphere)
        {            
                Vector3 d = (dest - sphere).Normalized();

                float f = 1 / (float)Math.PI;
                //float u = 0.5f + (float)Math.Atan2(d.Z, d.X) * 0.5f * f;
                //float v = 0.5f + (float)Math.Asin(d.Y) * f;

                float length = d.Xy.Length;
                if (length == 0) length = 1;
                float r = f * (float)Math.Acos(d.Z) / length;
                float u = 1 + d.X * r;
                float v = 1 + d.Y * r;

                int x = (int)(u * 750) % 1500;
                int y = (int)(v * 750) % 1500;

                return hdr[x + y * 1500];            
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
