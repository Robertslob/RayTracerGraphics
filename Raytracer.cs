using OpenTK;
using System;
using System.IO;
using OpenTK.Graphics.OpenGL;


namespace Application {



    //owns the scene, camera and surface. has the render method which is called by the application every frame
    class Raytracer
    {
	    public Surface screen;
        public Camera camera;
        public Scene scene;

        int CreateColor(int red, int green, int blue)
        {
            return (red << 16) + (green << 8) + blue;
        }         

   
	    public Camera Init()
	    {
            scene = new Scene();
            camera = new Camera(new Vector3(0, 0, -10), Vector3.UnitZ);
            camera.UpdatePlane();
            return camera;
	    }

        // this guy is called once every frame and draws everything on the screen -- CORE OF THE RAYTRACER!
	    public void Render()
	    {
            screen.Clear(0);
            //raytrace color for each pixel (hardcoded screen resolution!)
            /*for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    Ray currentray = camera.getRay(x, y, 512, 512);
                    Console.WriteLine("the " + x.ToString() + "th ray of row " + y.ToString() + " has direction: " + currentray.Direction.ToString());
                    Intersection intersected = scene.intersectScene(currentray);
                    Vector3 color = intersected.intersectedPrimitive.material.color;;
                    screen.pixels[y * 1024 + x] = CreateColor((int)color.X, (int)color.Y, (int)color.Z);
                }
            }*/
            //draw the debug
            debugOutput();            
	    }

        public void debugOutput()
        {
            //this should be made procedural to the screensize

            GL.PushAttrib(AttribMask.ViewportBit); //Push current viewport attributes to a sta ck
            GL.Viewport(0, 0, 512, 512); //Create a new viewport bottom left for the debug output.

            //we dont want to texture our lines
            GL.Disable(EnableCap.Texture2D);            

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m = Matrix4.CreateScale(1 / 32.0f);
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
                for (int x = 0; x <= 512; x += 512)
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
            //GL.Viewport(0, 0, 1024, 512);
        }

        private void debugRay(Vector3 pos, Ray ray, int depth)
        {
            Intersection intersected = scene.intersectScene(ray);

            Vector3 dest = (pos + ray.Direction * intersected.intersectionDistance);            
            GL.Vertex2(pos.Xz);
            GL.Vertex2(dest.Xz);
            
            if (intersected.intersectedPrimitive != null && depth > 0)
            {
                Vector3 m = ray.mirror(intersected.intersectedPrimitive.getNormal(dest));                
                Ray nray = new Ray(dest + m * 5f, m);
                debugRay(dest, nray, depth - 1);
            }
        }



    }
    /* ZINO's DEBUG DEEL DAT MISSCHIEN HANDIG KAN ZIJN LATER
            Sphere p = new Sphere(Vector3.UnitZ * 8f, 5f, new Material());
            p.debugOutput();
            Sphere sphere = new Sphere(new Vector3(20, 0, 30), 29f, new Material());
            sphere.debugOutput();

            Vector3 viewPlaneXZ = (camera.p3 - camera.p2).Normalized();

            GL.Color3(0.5f, 0.5f, 0.5f);

            for (int i = 0; i < 30; i++)
            {
                Vector3 rd = ((camera.p2 + (viewPlaneXZ * ((i) / 15.0f))) - camera.position);

                rd.Y = 0;
                rd.Normalize();

                Ray r = new Ray(camera.position, rd);
                float dist = Math.Min(p.intersects(r), sphere.intersects(r));

                GL.Begin(PrimitiveType.Lines);

                if (dist > 0)
                {
                    GL.Vertex2(camera.position.Xz);
                    GL.Vertex2(camera.position.Xz + rd.Xz * dist);
                }

                GL.End();
            }
             */

} 