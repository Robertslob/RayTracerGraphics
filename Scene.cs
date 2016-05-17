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

        public Scene()
        {
            //all the primitives that are present in our scene
            allPrimitives.Add( new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material()) );
            allPrimitives.Add( new Sphere(new Vector3(0, 0, 0), 1, new Material()));
            allPrimitives.Add( new Sphere(new Vector3(-3, 0, 0), 1, new Material()));
            allPrimitives.Add( new Sphere(new Vector3(3, 0, 0), 1, new Material()));
        }

        public intersection intersectScene(Ray ray)
        {
            //very bad way to make this work...
            float closestDistance = 1000000;
            Primitive closestPrimitive = new Sphere(new Vector3(0,0,0), 0.0f, new Material());

            foreach(Primitive primitive in allPrimitives){
                float currentDistance = primitive.intersects(ray);
                //we loop over all primitives and return the intersect of the closest one which we intersect
                if (closestDistance > currentDistance)
                {
                    closestDistance = currentDistance;
                    closestPrimitive = primitive;
                }
            }
            //returns the closest primitive if there is an intersection, else it returns a self made intersection...
            return new intersection(closestDistance, closestPrimitive);
        }


    }

    //contains the result of an intersection
    class intersection{
        float intersectionDistance;
        //I'm not sure this is what they mean in the assignment..
        Primitive intersectedPrimitive;
        //perhaps also the normal of the point of intersection should be stored here according to the assignment

        public intersection(float distance, Primitive primitive)
        {
            this.intersectionDistance = distance;
            this.intersectedPrimitive = primitive;
        }
        
    }

}
