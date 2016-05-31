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
            allPrimitives.Add(new Sphere(new Vector3(3, 1, 0), 1, new Material(new Vector3(1, 0, 1), 0f, 1.3f, 0.5f, false)));
            allPrimitives.Add(new Sphere(new Vector3(0, 1, -1), 1, new Material(new Vector3(1, 0, 1), 1.0f, 1.3f, 0.01f, false)));
            allPrimitives.Add(new Sphere(new Vector3(-5, 1, 0), 1, new Material(new Vector3(1, 0, 1), 0, 1.3f, 0.0f, false)));                     
            allPrimitives.Add(new Sphere(new Vector3(-2, 1, 0), 1, new Material("../../assets/2.jpg", 0.0f)));

            Material material = new Material("../../assets/1.jpg", 0.2f);
            Material material2 =new Material(new Vector3(1.0f, 0.5f, 1.0f), 0, 0, 0.2f, false);
            
            //we have implemented the possibility to construct .obj files by using triangles. Because the raytracer
            //is not implemented on the gpu, the fps while having a lot of primitives can be extremely low
            //also having a scene with a large amount of primitives can really take a long time to start due to the 
            //raytracer building the bvh, which is neither on the gpu nor multithreaded.
            //Scale can be different on different PCs, why????
            allPrimitives.AddRange(OBJParser.readOBJ("../../assets/stalin1.obj", new Vector3(0, 1.5f, 3), 0.00000001f, material2));
            //allPrimitives.AddRange(OBJParser.readOBJ("../../assets/bunny.obj", new Vector3(2, 0.5f, 0), 0.000001f, material));
            //allPrimitives.AddRange(OBJParser.readOBJ("../../assets/dragon.obj", new Vector3(0, 1.5f, 3), 0.00005f, material2));
            //allPrimitives.AddRange(OBJParser.readOBJ("../../assets/mino.obj", new Vector3(0, 1.5f, 3), 0.001f, material2));

            floor = (new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material("../../assets/1.jpg", 0.0f)));
            
            Console.WriteLine("Building BVH");
            buildBVH();            
            Console.WriteLine("Build BVH");
        }

        //builds the whole bvh
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

            //a node is always a leaf until proven innocent (russian-law-bvh)
            root.isleaf = true;
            root.first = 0;
            root.count = primitiveIndexes.Count();
            
            //grow our root into a beautifull tree
            BVHNode.Divide(root, primitiveIndexes, allPrimitives, 0);
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
                bool inAABB = parentNode.bounds.inAABB(ray.Origin);
                if (inAABB) parentIntersect = 0;
                if ((parentIntersect != 0 || inAABB) && parentIntersect < closestDistance)
                {
                    //if it is not a leaf we just enqueue its children so we can analyze them later on
                    if (!parentNode.isleaf)
                    {
                        float leftT = parentNode.left.bounds.intersect(ray);
                        float rightT = parentNode.right.bounds.intersect(ray);
                        if (leftT > rightT) //The left side is intersected first, so start looking at the left side first.
                        {                            
                            if (leftT != 0) searchQueue.Push(parentNode.left);
                            if (rightT != 0) searchQueue.Push(parentNode.right);
                        }
                        else //The right side is intersected first, so start looking at the right side first.
                        {
                            if (rightT != 0) searchQueue.Push(parentNode.right);
                            if (leftT != 0) searchQueue.Push(parentNode.left);
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
