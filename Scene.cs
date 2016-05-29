using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    class Scene
    {
        public List<Primitive> allPrimitives = new List<Primitive>();
        public List<Light> allLights = new List<Light>();
        public AABB sceneBox;
        private Plane floor;

        public Scene()
        {
            //all the primitives that are present in our scene
            allLights.Add(new Light(new Vector3(-5, 1, -5), new Vector3(150, 150, 150)));
            allLights.Add(new Light(new Vector3(5, 10, 5), new Vector3(50, 50, 50)));
            allLights.Add(new Light(new Vector3(5, 1, -5), new Vector3(50, 50, 50)));
            //allPrimitives.Add(new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material(new Vector3(0.2f, 0.2f, 0.3f), 0f, 1f, 0.5f, true)));            
            allPrimitives.Add(new Sphere(new Vector3(0, 1, 0), 1, new Material(new Vector3(1, 0, 1), 1f, 1.3f, 0.0f, false)));         
            allPrimitives.Add(new Sphere(new Vector3(-2, 1, 0), 1, new Material(new Vector3(0.5f, 0, 0), 0.0f, 1, 1f, false)));
            //allPrimitives.Add(new Sphere(new Vector3(2, 1, 0), 1, new Material(new Vector3(0, 0, 0.8f), 0.0f, 0.0f, true)));            
            allPrimitives.Add(new Sphere(new Vector3(2, 1, 0), 1, new Material("../../assets/2.jpg", 0.2f)));

            // Test-Triangle
            allPrimitives.Add(new Triangle(new Vector3(4, 1, 1), new Vector3(3, 2, 1), new Vector3(3.5f, 0, 0), new Material(new Vector3(0.0f, 0.5f, 0.2f), 0.1f, 0.3f, 0.05f, false)));

            floor = (new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material("../../assets/1.jpg", 0.0f)));            


            buildSceneAABB();            
        }

        public bool sceneboxIntersection(Ray r)
        {
            return sceneBox.intersect(r);
        }

        private void buildSceneAABB()
        {
            sceneBox.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            sceneBox.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (Primitive primitive in allPrimitives)
            {
                //set minimum and maximum of the primitive each dimension at a time
                for (int i = 0; i < 3; i++)
                {
                    sceneBox.minPoint[i] = Math.Min(primitive.box.minPoint[i], sceneBox.minPoint[i]);

                    sceneBox.maxPoint[i] = Math.Max(primitive.box.maxPoint[i], sceneBox.maxPoint[i]);
                }
            }
        }

        public Intersection intersectScene(Ray ray)
        {
            //very bad way to make this work... but it works.
            float closestDistance = 1000000;
            Primitive closestPrimitive = null;
            float currentDistance;

            if(sceneboxIntersection(ray)){
                foreach (Primitive primitive in allPrimitives)
                {
                    currentDistance = primitive.intersects(ray);
                    //we loop over all primitives and return the intersect of the closest one which we intersect
                    if (closestDistance > currentDistance && currentDistance > 0)
                    {
                        closestDistance = currentDistance;
                        closestPrimitive = primitive;
                    }
                }
            }            

            //we eventually check if we hit the floor
            currentDistance = floor.intersects(ray);
            if (closestDistance > currentDistance && currentDistance > 0)
            {
                closestPrimitive = floor;
                closestDistance = currentDistance;
            }

            //returns the closest primitive if there is an intersection, else it returns null
            return new Intersection(closestDistance, closestPrimitive);
        }

        


    }

    //contains the result of an intersection
    class Intersection{

        public float intersectionDistance;
        //I'm not sure this is what they mean in the assignment..
        public Primitive intersectedPrimitive;
        //perhaps also the normal of the point of intersection should be stored here according to the assignment

        public Intersection(float distance, Primitive primitive)
        {
            this.intersectionDistance = distance;
            this.intersectedPrimitive = primitive;
        }
        
    }

}
