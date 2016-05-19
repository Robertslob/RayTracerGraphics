using OpenTK;
using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Threading.Tasks;


namespace Application
{

    //owns the scene, camera and surface. has the render method which is called by the application every frame
    class Raytracer
    {
        public Surface screen;
        public Camera camera;
        public Scene scene;

        int CreateColor(int red, int green, int blue)
        {
            return (Math.Min(red, 255) << 16) + (Math.Min(green, 255) << 8) + Math.Min(blue, 255);
        }


        public Camera Init()
        {
            scene = new Scene();
            camera = new Camera(new Vector3(10, 3.5f, 11), new Vector3(-0.612f, -0.415f, -0.674f));
            camera.UpdatePlane();
            return camera;
        }

        // this guy is called once every frame and draws everything on the screen -- CORE OF THE RAYTRACER!
        int a = 0;
        public void Render()
        {
            a++;
            scene.allPrimitives[1].position.X = (float)Math.Sin((double)a / 20.0d) * 6;
            scene.allPrimitives[1].position.Z = (float)Math.Cos((double)a / 20.0d) * 6;
            scene.allPrimitives[1].material.color.X = 0.5f + (float)Math.Sin((double)a / 10.0d) * 0.5f;
            scene.allPrimitives[1].material.color.Y = 0.5f + (float)Math.Sin((double)a / 10.0d) * 0.5f;
            scene.allPrimitives[1].material.color.Z = 0.5f + (float)Math.Sin((double)a / 20.0d) * 0.5f;
            screen.Clear(0);
            //raytrace color for each pixel (hardcoded screen resolution!)

            Parallel.For(0, 512, (y) =>
            //for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    Ray currentray = camera.getRay(x, y);
                    Vector3 color = PrimaryRay(currentray);

                    screen.pixels[y * 1024 + x] = CreateColor((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
                }
            }
            );
        }

        private Vector3 PrimaryRay(Ray r, int depth = 5)
        {
            Intersection intersected = scene.intersectScene(r);
            Vector3 color = new Vector3(.4f, 0.4f, 1);

            Primitive primitive = intersected.intersectedPrimitive;
            if (primitive != null)
            {
                Material material = primitive.material;
                

                //with the dest we can calulate the luminicity (hoe je dat ook schrijft) of a pixel by shadowraying
                Vector3 dest = r.Origin + r.Direction * intersected.intersectionDistance;

                Vector3 illumination = scene.calculateillumination(dest, primitive);
                color = material.getpatternColor(dest) * illumination;            
                

                if (material.reflection > 0 && depth > 0)
                {
                    Vector3 reflV = r.mirror(primitive.getNormal(dest));
                    //the 0.01f is a small delta to prevent the reflection hitting the object that reflected the ray again
                    Ray nray = new Ray(dest + reflV * 0.01f, reflV);
                    //combination of the color of the object and the color our reflectionray returns
                    color = (color * (1 - material.reflection) + PrimaryRay(nray, depth - 1) * material.reflection);
                }
            }

            return color;
        }

        public void debugOutput()
        {
            //this should be made procedural to the screensize

            GL.PushAttrib(AttribMask.ViewportBit); //Push current viewport attributes to a sta ck
            GL.Viewport(512, 0, 512, 512); //Create a new viewport bottom left for the debug output.

            //we dont want to texture our lines
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m = Matrix4.CreateScale(1 / 16.0f);
            GL.LoadMatrix(ref m);


            //Draw the camera -- must be a point according to the assignment
            GL.Color3(0.8f, 0.2f, 0.2f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(camera.position.Xz);
            GL.End();

            //Draw camera end
            Vector2 sv1 = (camera.position.Xz - camera.p1.Xz) * -30;
            Vector2 sv2 = (camera.position.Xz - camera.p3.Xz) * -30;

            GL.Color3(0.8f, 0.5f, 0.5f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(camera.position.Xz);
            GL.Vertex2((camera.position.Xz + sv1));

            GL.Vertex2(camera.position.Xz);
            GL.Vertex2((camera.position.Xz + sv2));

            GL.Color3(0.2f, 1.0f, 0.7f);

            //draw the rays of the 255 row with an interval of 64
            for (int y = 255; y < 256; y++)
            {
                for (int x = 0; x <= 512; x += 64)
                {
                    Ray currentray = camera.getRay(x, y);
                    debugRay(camera.position, currentray, 10);
                }
            }

            GL.Color3(0.4f, 1.0f, 0.4f);
            GL.Vertex2(camera.p1.Xz);
            GL.Vertex2(camera.p3.Xz);
            GL.End();

            foreach (Primitive primitive in scene.allPrimitives)
            {
                primitive.debugOutput();
            }

            //we want to texture stuff again and restor our viewport
            GL.Enable(EnableCap.Texture2D);
            GL.PopAttrib();//Reset to the old viewport.
            GL.Viewport(0, 0, 1024, 512);
        }

        private void debugshadowRay(Vector3 pos){
            GL.Color3(1.0f, 1.0f, 0.4f);
            foreach (Light light in scene.allLights){
                Vector3 dir = pos - light.location;                
                Vector3 normDir = dir.Normalized();
                Ray r = new Ray(light.location, normDir);
                bool isblocked = false;

                //float DistancetoPoint = length*.99f;
                float currentDistance;

                foreach (Primitive primitive in scene.allPrimitives)
                {                    
                    currentDistance = primitive.intersects(r);
                    if (currentDistance * currentDistance < dir.LengthSquared * 0.999f && currentDistance > 0){
                        GL.Vertex2(light.location.Xz);
                        GL.Vertex2(light.location.Xz + currentDistance * normDir.Xz);
                        isblocked = true;
                        break;
                    }
                }
                if (!isblocked)
                {
                    GL.Vertex2(light.location.Xz);
                    GL.Vertex2(pos.Xz);
                }
            }            
        }

        private void debugRay(Vector3 pos, Ray ray, int depth)
        {
            Intersection intersected = scene.intersectScene(ray);

            Vector3 dest = (pos + ray.Direction * intersected.intersectionDistance);
            GL.Color3(0.4f, 1.0f, 0.4f);

            GL.Vertex2(pos.Xz);
            GL.Vertex2(dest.Xz);

            

            if (intersected.intersectedPrimitive != null && depth > 0)
            {
                debugshadowRay(dest);
                if (intersected.intersectedPrimitive.material.reflection > 0)
                {
                    Vector3 m = ray.mirror(intersected.intersectedPrimitive.getNormal(dest));
                    Ray nray = new Ray(dest + m * 0.05f, m);
                    debugRay(dest, nray, depth - 1);
                }
            }
        }
    }
} 