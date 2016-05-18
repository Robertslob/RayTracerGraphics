﻿using OpenTK;
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
            allLights.Add(new Light(new Vector3(0, 0, 0), new Vector3(5, 5, 5)));
            allPrimitives.Add(new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material(new Vector3(0.2f, 0.2f, 0.3f), 0.0f, 0.4f)));
            allPrimitives.Add(new Sphere(new Vector3(0, 1, 0), 1, new Material(new Vector3(0, 1, 0), 0.5f, 0.98f)));
            allPrimitives.Add(new Sphere(new Vector3(-3, 1, 0), 1, new Material(new Vector3(1, 0, 0), 0.5f, 0.0f)));
            allPrimitives.Add(new Sphere(new Vector3(3, 1, 0), 1, new Material(new Vector3(0, 0, 1), 0.5f, 0.0f)));
        }

        public Intersection intersectScene(Ray ray)
        {
            //very bad way to make this work...
            float closestDistance = 1000000;
            Primitive closestPrimitive = null;
            float currentDistance;

            foreach(Primitive primitive in allPrimitives){
                currentDistance = primitive.intersects(ray);
                //we loop over all primitives and return the intersect of the closest one which we intersect
                if (closestDistance > currentDistance && currentDistance > 0)
                {
                    closestDistance = currentDistance;
                    closestPrimitive = primitive;
                }
            }
            //returns the closest primitive if there is an intersection, else it returns null
            return new Intersection(closestDistance, closestPrimitive);
        }

        public Vector3 calculateillumination(Vector3 point, Primitive primitive)
        {
            Vector3 illumination = Vector3.Zero;
            //foreach light we check if there is nothing in the way to our destination point 
            foreach (Light light in allLights)
            {
                Vector3 dir = point - light.location;
                float length = (float)Math.Sqrt((Vector3.Dot(dir, dir)));
                Vector3 normDir = dir / length;
                }
            }
            return false;
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
