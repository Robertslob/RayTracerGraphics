using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            //allPrimitives.Add(new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material(new Vector3(0.2f, 0.2f, 0.3f), 0f, 1f, 0.5f, true)));            
            //warning this takes like 5 min to load....!!!!!!
            

            allPrimitives.Add(new Sphere(new Vector3(9, 1, 0), 1, new Material(new Vector3(1, 0, 1), 1f, 1.3f, 0.0f, false)));         
            allPrimitives.Add(new Sphere(new Vector3(6, 1, 0), 1, new Material(new Vector3(0.5f, 0, 0), 0.0f, 1, 1f, false)));
            allPrimitives.Add(new Sphere(new Vector3(3, 1, 0), 1, new Material(new Vector3(1, 0, 1), 1f, 1.3f, 0.0f, false)));
            allPrimitives.Add(new Sphere(new Vector3(0, 1, 0), 1, new Material(new Vector3(1, 0, 1), 1f, 1.3f, 0.0f, false)));
            allPrimitives.Add(new Sphere(new Vector3(-5, 1, 0), 1, new Material(new Vector3(1, 0, 1), 1f, 1.3f, 0.0f, false)));                     
            allPrimitives.Add(new Sphere(new Vector3(-2, 1, 0), 1, new Material("../../assets/2.jpg", 0.2f)));

            //warning this takes like 5 min to load....!!!!!!
            //buildAsset("../../assets/bunny.obj", new Vector3(2, 0, 2));
            // Test-Triangle
            //allPrimitives.Add(new Triangle(new Vector3(8, 1, 1), new Vector3(7, 1, 1), new Vector3(7.5f, 0, 0), new Material(new Vector3(0.5f, 0.5f, 0.2f), 0.1f, 0.3f, 0.5f, false)));

            floor = (new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Material("../../assets/1.jpg", 0.0f)));            
            
            buildBVH();
            Console.WriteLine("build BVH");
        }

        //this function is copied from here:
        //http://www.rexcardan.com/2014/10/read-obj-file-in-c-in-just-10-lines-of-code/
        //all credits go to the guy who really wrote this...
        public void buildAsset(string objectLocation, Vector3 objectPosition)
        {

            var lines = File.ReadAllLines(objectLocation);
            //List of double[]. Each entry of the list contains 3D vertex x,y,z in double array form
            var verts = lines.Where(l => Regex.IsMatch(l, @"^v(\s+-?\d+\.?\d+([eE][-+]?\d+)?){3,3}$"))
                .Select(l => Regex.Split(l, @"\s+", RegexOptions.None).Skip(1).ToArray()) //Skip v
                .Select(nums => new Vector3(float.Parse(nums[0]), float.Parse(nums[1]), float.Parse(nums[2])))
                .ToList();

            //List of int[]. Each entry of the list contains zero based index of vertex reference
            //Obj format is 1 based index. This is converting into C# zero based, so on write out you need to convert back.
            var faces = lines.Where(l => Regex.IsMatch(l, @"^f(\s\d+(\/+\d+)?){3,3}$"))
                .Select(l => Regex.Split(l, @"\s+", RegexOptions.None).Skip(1).ToArray())//Skip f
                .Select(i => i.Select(a => Regex.Match(a, @"\d+", RegexOptions.None).Value).ToArray())
                .Select(nums => new int[] { int.Parse(nums[0]) - 1, int.Parse(nums[1]) - 1, int.Parse(nums[2]) - 1 })
                .ToList();
            Console.WriteLine(faces.Count.ToString());


            Material material = new Material("../../assets/1.jpg", 0.0f);
            foreach (int[] face in faces)
            {
                //Console.WriteLine((objectPosition + 0.000001f * verts[face[0]]));
                //allPrimitives.Add(new Triangle(objectPosition + 0.000001f * verts[face[0]], objectPosition + 0.000001f * verts[face[1]], objectPosition + 0.000001f * verts[face[2]], new Material(new Vector3(0.3f, 0.5f, 0.2f), 0, 1.3f, 0.0f, false)));
                allPrimitives.Add(new Triangle(objectPosition + 0.000001f * verts[face[0]], objectPosition + 0.000001f * verts[face[1]], objectPosition + 0.000001f * verts[face[2]], material));
            }
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
                if (parentNode.bounds.intersect(ray) > 0)
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
                        if (closestPrimitive != null)
                        {
                            break;
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

        public float intersectionDistance;
        public Primitive intersectedPrimitive;

        public Intersection(float distance, Primitive primitive)
        {
            this.intersectionDistance = distance;
            this.intersectedPrimitive = primitive;
        }
        
    }

}
