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

        public Material(string file, float reflection)
        {
            this.reflection = reflection;
            this.hasPattern = true;
            this.color = new Vector3(1, 1, 1);
            surface = new Surface(file);
        }

        // Loads the HDR
        public static void loadHdr(string file, int width)
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
        
        public static Vector3 getHemiSphereColor(Vector3 dest, Vector3 sphere)
        {
            Vector3 d = (dest - sphere).Normalized();

            float f = 1 / (float)Math.PI;

            float length = d.Xy.Length;
            if (length == 0) length = 1;
            float r = f * (float)Math.Acos(d.Z) / length;
            float u = 1 + d.X * r;
            float v = 1 + d.Y * r;

            int x = (int)(u * 750) % 1500;
            int y = (int)(v * 750) % 1500;

            return hdr[x + y * 1500];
        }

        // Returns the color of a pattern
        public Vector3 getpatternColor(Vector3 dest, Primitive primitive)
        {
            if (hasPattern && surface == null)
            {
                int i = ((int)dest.X + ((int)dest.Z)) & 1;
                return new Vector3(i, i, i);

            }
            else if (surface != null)
            {
                if (primitive.GetType() == typeof(Sphere))
                {
                    return getSphereColor(dest, primitive.position);
                }
                else if(primitive.GetType() == typeof(Triangle)) {
                    var uv = ((Triangle)primitive).getUV(new Ray(dest + Vector3.UnitY, -Vector3.UnitY));
                    float uc = uv.Item1;
                    float vc = uv.Item2;
                    int x = (int)(uc * surface.width) % surface.width;
                    int y = (int)(vc * surface.height) % surface.height;
                    x = (x < 0) ? surface.width + x : x;
                    y = (y < 0) ? surface.height + y : y;

                    int c = surface.pixels[x + y * surface.width];

                    int r = (c >> 16) & 0xff;
                    int g = (c >> 8) & 0xff;
                    int b = (c) & 0xff;
                    float div = 1 / 255.0f;
                    return new Vector3(r * div, g * div, b * div);
                }
                else
                {
                    Vector3 n = primitive.getNormal(dest);
                    Vector3 u = new Vector3(n.Y, -n.X, 0).Normalized();
                    Vector3 v = Vector3.Cross(n, u);
                    float uc = Vector3.Dot(u, dest);
                    float vc = Vector3.Dot(v, dest);

                    int x = (int)(uc*surface.width) % surface.width;
                    int y = (int)(vc*surface.height) % surface.height;
                    x = (x < 0) ? surface.width + x : x;
                    y = (y < 0) ? surface.height + y : y;

                    int c = surface.pixels[x + y * surface.width];

                    int r = (c >> 16) & 0xff;
                    int g = (c >> 8) & 0xff;
                    int b = (c) & 0xff;
                    float div = 1 / 255.0f;
                    return new Vector3(r * div, g * div, b * div);
                }
            }
            else
            {
                return color;
            }
        }

        //Returns the color of the sphere, which is easy, unless it has a pattern
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

                float div = 1 / 255.0f;
                return new Vector3(r * div, g * div, b * div);
            }
            else
            {
                return color;
            }
        }
    }
}
