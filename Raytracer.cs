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
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    screen.pixels[y * 1024 + x] = 255;
                }
            }

            //draw the debug
            debugOutput();            
	    }

        public void debugOutput()
        {
            //this should be made procedural to the screensize
            GL.Viewport(512, 0, 512, 512);
            //we dont want to texture our lines
            GL.Disable(EnableCap.Texture2D);

            //Console.WriteLine("[pos: '" + camera.position + "', dir: '" + camera.direction + "'");

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m = Matrix4.CreateScale(1 / 16.0f);
            GL.LoadMatrix(ref m); 
            GL.Color3(0.8f, 0.3f, 0.3f);

            //Draw the camera -- must be a point according to the assignment
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(camera.position.Xz);
            GL.End();

            //Draw camera end
            Vector2 sv1 = (camera.position.Xz - camera.p1.Xz) * -30;
            Vector2 sv2 = (camera.position.Xz - camera.p3.Xz) * -30;

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(camera.position.Xz);
            GL.Vertex2((camera.position.Xz + sv1));

            GL.Vertex2(camera.position.Xz);
            GL.Vertex2((camera.position.Xz + sv2));

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
            GL.Viewport(0, 0, 1024, 512);

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