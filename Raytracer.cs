using OpenTK;
using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Drawing;


namespace Application {



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
                    Ray currentray = camera.getRay(x, y);
                    Vector3 color = PrimaryRay(currentray);
                    
                    screen.pixels[y * 1024 + x] = CreateColor((int)(color.X * 255), (int)(color.Y * 255),(int)( color.Z * 255));
                }
            }
            //draw the debug                    
	    }

        private Vector3 PrimaryRay(Ray r, int depth = 5)
        {
            Intersection intersected = scene.intersectScene(r);
            Vector3 color = new Vector3(0, 0, 0);

            Primitive primitive = intersected.intersectedPrimitive;
            if (primitive != null)
            {
                Material material = primitive.material;
                color = material.color;

                //with the dest we can calulate the luminicity (hoe je dat ook schrijft) of a pixel by shadowraying
                Vector3 dest = r.Origin + r.Direction * intersected.intersectionDistance;      
          
                //shadowray the fuck out of the dest
                //a very important thing to note is that right now it is much easier to start at our lights and from there on try to see if the object is visible. if you do it the other way around
                //you wil have to write more code probably because you have to make sure that "until" a certain point there is no intersection while the other way around you only have to check
                //if the object that is intersected is the primitive of dest. I think.....
                
                if (material.reflection > 0 && depth > 0)
                {
                    Vector3 reflV = r.mirror(primitive.getNormal(dest));
                    //the 0.01f is a small delta to prevent the reflection hitting the object that reflected the ray again
                    Ray nray = new Ray(dest + reflV * 0.01f, reflV);
                    //combination of the color of the object and the color our reflectionray returns
                    color = color * (1 - material.reflection) + PrimaryRay(nray, depth - 1) * material.reflection;
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

        private void debugRay(Vector3 pos, Ray ray, int depth)
        {
            Intersection intersected = scene.intersectScene(ray);

            Vector3 dest = (pos + ray.Direction * intersected.intersectionDistance);            
            GL.Vertex2(pos.Xz);
            GL.Vertex2(dest.Xz);

            
            if (intersected.intersectedPrimitive != null && depth > 0)
            {
                if (intersected.intersectedPrimitive.material.reflection > 0)
                {
                    Vector3 m = ray.mirror(intersected.intersectedPrimitive.getNormal(dest));
                    Ray nray = new Ray(dest + m * 5.0f, m);
                    debugRay(dest, nray, depth - 1);
                }
            }
             
        }



    }
    

} 