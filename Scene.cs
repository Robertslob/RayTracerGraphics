using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using template;

namespace Application
{
    class Scene
    {
        public List<Primitive> allPrimitives = new List<Primitive>();
        public List<Light> allLights = new List<Light>();
        private Plane floor;

        //for the acceleration structure        
        public List<int> primitiveIndexes = new List<int>();
        BVHNode root;
        

        public Scene()
        {
            //all the primitives that are present in our scene
            allLights.Add(new Light(new Vector3(0, 1, -5), new Vector3(150, 150, 150)));
            allLights.Add(new Light(new Vector3(10, 10, 5), new Vector3(50, 50, 50)));
            allLights.Add(new Light(new Vector3(10, 1, -5), new Vector3(50, 50, 50)));            
           /* allPrimitives.Add(new Sphere(new Vector3(3, 1, 0), 1, new Material(new Vector3(1, 0, 1), 0f, 1.3f, 0.0f, false)));
            allPrimitives.Add(new Sphere(new Vector3(0, 1, -1), 1, new Material(new Vector3(1, 0, 1), 0, 1.3f, 0.0f, false)));
            allPrimitives.Add(new Sphere(new Vector3(-5, 1, 0), 1, new Material(new Vector3(1, 0, 1), 0, 1.3f, 0.0f, false)));                     
            allPrimitives.Add(new Sphere(new Vector3(-2, 1, 0), 1, new Material("../../assets/2.jpg", 0.0f)));*/

            //warning this takes like 5 min to load....!!!!!!
            Material material = new Material(new Vector3(1.0f, 0.5f, 1.0f), 0, 0, 0f, false);
            allPrimitives.AddRange(OBJParser.readOBJ("../../assets/freg.obj", new Vector3(0, 1.5f, 0), 0.005f, material));

            // Test-Triangle
            //allPrimitives.Add(new Triangle(new Vector3(8, 1, 1), new Vector3(7, 1, 1), new Vector3(7.5f, 0, 0), new Material(new Vector3(0.5f, 0.5f, 0.2f), 0.1f, 0.3f, 0.5f, false)));

            floor = (new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material("../../assets/1.jpg", 0.0f)));
            Console.WriteLine("Building BVH");
            buildBVH();

            
            Console.WriteLine("build BVH");
        }

        public void buildBVH()
        {
            root = new BVHNode();
            root.bounds.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            root.bounds.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            allPrimitives.Sort((Primitive p1, Primitive p2) => {
                return p1.position.X.CompareTo(p2.position.X);
            });
          
            //we set the right bounds for our root and also fill our primitive indexarray with unsorted primitive indexes
            for (int j = 0; j < allPrimitives.Count(); j++)
            {
                root.bounds.adjust(allPrimitives[j].box);

                //identity keyvalue we will sort while subdividing
                primitiveIndexes.Add(j);
            }

            //just in case our whole scene consists of 3 primitives
            root.isleaf = true;
            root.first = 0;
            root.count = primitiveIndexes.Count();
            
            BVHNode.subdivide(root, primitiveIndexes, allPrimitives, 0);
        }

        public Intersection intersectScene(Ray ray)
        {
            Stack<BVHNode> searchQueue = new Stack<BVHNode>();
            //add the root in the searchqueue
            searchQueue.Push(root);

            // We set closestdistance to be very latge, and look for an object closer
            float closestDistance = 1000000;
            Primitive closestPrimitive = null;
            float currentDistance;

            //if we have more nodes to search            
            while (searchQueue.Count != 0)
            {                
                BVHNode parentNode = searchQueue.Pop();
                //we only continue if a ray intersects with the parents aabb
                float parentIntersect = parentNode.bounds.intersect(ray);
                if (parentIntersect > 0 && parentIntersect < closestDistance)
                {
                    //if it is not a leaf we just enqueue its children so we can analyze them later on
                    if (!parentNode.isleaf)
                    {
                        float leftT = parentNode.left.bounds.intersect(ray);
                        float rightT = parentNode.right.bounds.intersect(ray);
                        if (leftT > rightT) //The left side is intersected first, so start looking at the left side first.
                        {                            
                            if (leftT > 0) searchQueue.Push(parentNode.left);
                            if (rightT > 0) searchQueue.Push(parentNode.right);
                        }
                        else //The right side is intersected first, so start looking at the right side first.
                        {
                            if (rightT > 0) searchQueue.Push(parentNode.right);
                            if (leftT > 0) searchQueue.Push(parentNode.left);
                        }
                    }
                    //if it is a leaf we have to check every primitive that it owns
                    else
                    {                        
                        for (int n = parentNode.first; n < parentNode.first + parentNode.count; n++)
                        {
                            currentDistance = allPrimitives[primitiveIndexes[n]].intersects(ray);
                            //we loop over all primitives and return the intersect of the closest one which we intersect
                            if (closestDistance > currentDistance && currentDistance > 0)
                            {
                                closestDistance = currentDistance;
                                closestPrimitive = allPrimitives[primitiveIndexes[n]];
                            }
                        }
                    }
                }                
            }            
            //eventually we also check if we hit the floor
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

    //contains our necessary result of an intersection
    class Intersection{

        public float distance;
        public Primitive primitive;

        public Intersection(float distance, Primitive primitive)
        {
            this.distance = distance;
            this.primitive = primitive;
        }
        
    }

}
