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
        private Plane floor;

        //for the acceleration structure
        public List<BVHNode> nodePool = new List<BVHNode>();
        public List<int> primitiveIndexes = new List<int>();
        BVHNode root;
        

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
            
            buildBVH();
        }

        public void buildBVH()
        {
            root.bounds.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            root.bounds.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
          
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
            root.count = (uint)primitiveIndexes.Count();

            nodePool.Add(root);
            //subdivide the root to create the whole tree structure.
            root.subdivide(nodePool, primitiveIndexes, allPrimitives);
        }

        public Intersection intersectScene(Ray ray)
        {
            Queue<int> searchQueue = new Queue<int>();
            //add the root in the searchqueue
            searchQueue.Enqueue(0);

            //very bad way to make this work... but it works.
            float closestDistance = 1000000;
            Primitive closestPrimitive = null;
            float currentDistance;

            //if we have more nodes to search
            while (searchQueue.Count != 0)
            {
                int parentInt = searchQueue.Dequeue();
                BVHNode parentNode = nodePool[parentInt];
                //we only continue if a ray intersects with the parents aabb
                if (parentNode.bounds.intersect(ray))
                {
                    //if it is not a leaf we just enqueue its children so we can analyze them later on
                    if (!parentNode.isleaf)
                    {
                        searchQueue.Enqueue((int)parentNode.left);
                        searchQueue.Enqueue((int)parentNode.right);
                    }
                    //if it is a leaf we have to check every primitive that it owns
                    else
                    {
                        for (int n = (int)parentNode.first; n < parentNode.first + parentNode.count; n++)
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
