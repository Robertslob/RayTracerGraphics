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

        public static int WIDTH = 512 >> 3;
        public static int MAXDEPTH = 5;

        int CreateColor(int red, int green, int blue)
        {
            return (Math.Min(red, 255) << 16) + (Math.Min(green, 255) << 8) + Math.Min(blue, 255);
        }

        public Camera Init()
        {
            Material.loadHdr("../../assets/stpeters_probe.float", 1500);
            scene = new Scene();
            camera = new Camera(new Vector3(2.5f, 2.5f, 2.5f), new Vector3(-.612f, -.415f, -0.674f));
            camera.UpdatePlane();
            return camera;
        }

        // this guy is called once every frame and draws everything on the screen -- CORE OF THE RAYTRACER!
        //int a = 0;
        public void Render()
        {
            /*a++;
            scene.allPrimitives[1].position.X = (float)Math.Sin((double)a / 20.0d) * 6;
            scene.allPrimitives[1].position.Z = (float)Math.Cos((double)a / 20.0d) * 6;
            scene.allPrimitives[1].material.color.X = 0.5f + (float)Math.Sin((double)a / 10.0d) * 0.5f;
            scene.allPrimitives[1].material.color.Y = 0.5f + (float)Math.Sin((double)a / 10.0d) * 0.5f;
            scene.allPrimitives[1].material.color.Z = 0.5f + (float)Math.Sin((double)a / 20.0d) * 0.5f;*/
            screen.Clear(0);
            //raytrace color for each pixel (hardcoded screen resolution!)
            
            Parallel.For(0, WIDTH, (y) =>
            //for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Ray currentray = camera.getRay(x, y);
                    Vector3 color = PrimaryRay(currentray, MAXDEPTH);

                    screen.pixels[y * WIDTH + x] = CreateColor((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
                }
            }
            );
        }

        private Vector3 PrimaryRay(Ray r, int depth)
        {
            Intersection intersected = scene.intersectScene(r);
            Vector3 color = Material.getHemiSphereColor(r.Origin + r.Direction * intersected.intersectionDistance, Vector3.Zero);
            //Vector3 color = new Vector3(0.4f, 0.3f, 0.7f);
            if (depth == 0) return Vector3.Zero;
            Primitive primitive = intersected.intersectedPrimitive;
            if (primitive != null)
            {
                Material material = primitive.material;

                //with the dest we can calulate the luminicity (hoe je dat ook schrijft) of a pixel by shadowraying
                Vector3 dest = r.Origin + r.Direction * intersected.intersectionDistance;

                Vector3 illumination = calculateillumination(dest, primitive);
                color = material.getpatternColor(dest, primitive) * illumination;

                //Get reflection and refrection vectors.
                Vector3 reflectionColor = Vector3.Zero;
                Vector3 refractionColor = Vector3.Zero;

                if (material.reflection > 0 && depth > 0)
                {
                    Vector3 reflV = r.mirror(primitive.getNormal(dest));
                    //the 0.01f is a small delta to prevent the reflection hitting the object that reflected the ray again
                    Ray nray = new Ray(dest + reflV * 0.01f, reflV);
                    nray.refractionIndex = r.refractionIndex; //Set the refraction index of the ray.
                    //combination of the color of the object and the color our reflectionray returns
                    reflectionColor = PrimaryRay(nray, depth - 1);
                    //reflectionColor = material.reflection * reflectionColor + (1 - material.reflection) * color;
                    color = material.reflection * reflectionColor + (1 - material.reflection) * color;
                }

                //If there is refraction...
                if (material.refraction > 0 && depth > 0)
                {
                    Ray refractionRay = refract(primitive, r, dest);//calculate refraction ray.
                    if (refractionRay != null)
                    {
                        if (material.reflection == 0) //If we have no reflection calculated...
                        {
                            refractionColor = PrimaryRay(refractionRay, depth - 1);
                        }
                        else
                        {
                            float Rf = (float)Math.Pow((r.refractionIndex - material.refractionIndex) / (r.refractionIndex + material.refractionIndex), 2);
                            float fresnel = Rf + (1 - Rf) * (float)Math.Pow(1 - Math.Abs(Vector3.Dot(primitive.getNormal(dest), -r.Direction)), 5);//calculate fresnel index
                            refractionColor = fresnel * reflectionColor + (1 -fresnel) * PrimaryRay(refractionRay, depth - 1);
                        }
                    }
                    else
                    {
                        //If there is no refraction, then there is reflection!
                        refractionColor = reflectionColor;
                    }
                    color = (material.refraction * refractionColor + (1 - material.refraction) * color);//set color to refraction.                    
                }               
                
            }
            
            return color;
        }

        public Ray refract(Primitive primitive, Ray ray, Vector3 dest)
        {
            //cheat, if the current refraction of the ray is not 1, it means it is in something.
            float refrIndex = (ray.refractionIndex != 1.0f) ? refrIndex = 1 : refrIndex = primitive.material.refractionIndex;
            Vector3 normal = primitive.getNormal(dest);
            //calculate n1/n2
            float n12 = ray.refractionIndex / refrIndex;
            //calculate cos(theta)

            float cosTheta = Vector3.Dot(-ray.Direction, normal);
            if (cosTheta < 0)
            {
                normal = -normal;
                cosTheta = -cosTheta;
            }
            
            //check if the refraction hasn't reached its critical point.
            float k = 1 - ((n12 * n12) * (1 - (cosTheta * cosTheta)));
            if (k < 0) return null;

            //refract.
            Vector3 dir = n12 * ray.Direction + normal * (n12 * cosTheta - (float)Math.Sqrt(k));

            dir.Normalize();
            Ray nray = new Ray(dest + 0.012f * dir, dir);
            nray.refractionIndex = refrIndex;
            
            return nray;
        }

        public Vector3 calculateillumination(Vector3 point, Primitive primitive)
        {
            Vector3 illumination = Vector3.Zero;
            //foreach light we check if there is nothing in the way to our destination point 
            foreach (Light light in scene.allLights)
            {
                Vector3 dir = point - light.location;
                Vector3 pointNormal = primitive.getNormal(point);
                //only if a light source is not behind the point that needs shading
                if (Vector3.Dot(dir, pointNormal) < 0 && Vector3.Dot(dir, dir) < (WIDTH << 4))
                {
                    //only if the distance of the light is not too far away                    
                    Vector3 normDir = dir.Normalized();
                    if (!shadowRay(point, light, normDir, dir.LengthSquared))
                    {                        
                        float dotpr = Vector3.Dot(-normDir, pointNormal);
                        if (dotpr > 0)
                            illumination += (1 / (dir.LengthSquared)) * light.intensity * dotpr;
                    }
                }
            }
            return illumination;
        }

        // Return true als er iets tussen de lichtbron en het punt zit, false anders
        public bool shadowRay(Vector3 point, Light light, Vector3 normDir, float length)
        {

            Ray r = new Ray(light.location, normDir);

            //float DistancetoPoint = length*.99f;
            float currentDistance;

            foreach (Primitive primitive in scene.allPrimitives)
            {
                currentDistance = primitive.intersects(r);
                if (currentDistance * currentDistance < length * 0.999f && currentDistance > 0)
                    return true;
            }
            return false;
        }

        public void debugOutput()
        {
            //this should be made procedural to the screensize           

            //we dont want to texture our lines
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m = Matrix4.CreateScale(1 / 10.0f);
            GL.LoadMatrix(ref m);


            //Draw the camera -- must be a point according to the assignment
            GL.Color3(0.8f, 0.2f, 0.2f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(camera.position.Xz);
            GL.End();

            //Draw camera end
            Vector2 sv1 = (camera.p2.Xz - camera.position.Xz) * 30.0f;
            Vector2 sv2 = (camera.p3.Xz - camera.position.Xz) * 30.0f;

            GL.Color3(0.8f, 0.5f, 0.5f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(camera.position.Xz);
            GL.Vertex2((camera.position.Xz + sv1));

            GL.Vertex2(camera.position.Xz);
            GL.Vertex2((camera.position.Xz + sv2));

            GL.Color3(0.2f, 1.0f, 0.7f);

            //draw the rays of the 255 row with an interval of 64

            for (int x = 0; x <= WIDTH; x += 64)
            {
                Ray currentray = camera.getRay(x, WIDTH >> 1);
                debugRay(camera.position, currentray, MAXDEPTH);
            }

            GL.Color3(0.4f, 1.0f, 0.4f);
            GL.Vertex2(camera.p2.Xz);
            GL.Vertex2(camera.p3.Xz);
            GL.End();

            foreach (Primitive primitive in scene.allPrimitives)
            {
                primitive.debugOutput();
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
                    nray.refractionIndex = ray.refractionIndex;
                    debugRay(dest, nray, depth - 1);
                }
                Material material = intersected.intersectedPrimitive.material;
                if (material.refraction > 0 && depth > 0)
                {
                    Ray nray = refract(intersected.intersectedPrimitive, ray, dest);
                    if (nray != null)
                    {
                        debugRay(dest, nray, depth - 1);
                    }
                    else
                    {

                        Vector3 m = ray.mirror(intersected.intersectedPrimitive.getNormal(dest));
                        nray = new Ray(dest + m * 0.05f, m);
                        debugRay(dest, nray, depth - 1);
                    }

                }
            }
        }

        private void debugshadowRay(Vector3 pos)
        {
            GL.Color3(1.0f, 1.0f, 0.4f);
            foreach (Light light in scene.allLights)
            {
                Vector3 dir = pos - light.location;
                Vector3 normDir = dir.Normalized();
                Ray r = new Ray(light.location, normDir);
                bool isblocked = false;

                //float DistancetoPoint = length*.99f;
                float currentDistance;

                foreach (Primitive primitive in scene.allPrimitives)
                {
                    currentDistance = primitive.intersects(r);
                    if (currentDistance * currentDistance < dir.LengthSquared * 0.999f && currentDistance > 0)
                    {
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
    }
} 