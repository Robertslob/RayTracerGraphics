using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application 
{
    public class BVHNode
    {
        public BVHNode left;
        public BVHNode right;
        public AABB bounds;
        public bool isleaf;
        public int first;
        public int count;

        public string toString()
        {
            return "left: " + left.ToString() + ", right: " + right.ToString() + ", count: " + count.ToString() + " en first: " + first.ToString();
        }        
        public static void Divide(BVHNode root, List<int> primitiveIndex, List<Primitive> allprimitives, int ownNodeIndex)
        {
            //if a node has less than 3 primitives it is a leaf            
            uint nodeCounter = 0;
            Queue<BVHNode> toProcess = new Queue<BVHNode>();
            toProcess.Enqueue(root);
            while (toProcess.Count > 0)
            {
                nodeCounter++;
                BVHNode node = toProcess.Dequeue();
                if (node.count <= 8)
                {                    
                    continue;
                }                
                float bestCostFormation = node.bounds.getSurface() * node.count;                
                float pivot;
                Vector3 axis;
                //foreach splitplane that we can imagine with the primitives of this node
                
                Tuple<float, float> x = getBestSplitPlane(primitiveIndex, allprimitives, node, Vector3.UnitX);                
                Tuple<float, float> y = getBestSplitPlane(primitiveIndex, allprimitives, node, Vector3.UnitY);                
                Tuple<float, float> z = getBestSplitPlane(primitiveIndex, allprimitives, node, Vector3.UnitZ);
                
                if (x.Item1 <= y.Item1 && x.Item1 <= z.Item1)
                {
                    bestCostFormation = x.Item1;
                    pivot = x.Item2;
                    axis = Vector3.UnitX;
                }
                else if (y.Item1 <= x.Item1 && y.Item1 <= z.Item1)
                {
                    bestCostFormation = y.Item1;
                    pivot = y.Item2;
                    axis = Vector3.UnitY;
                }
                else
                {
                    bestCostFormation = z.Item1;
                    pivot = z.Item2;
                    axis = Vector3.UnitZ;
                }
                
                //r the split in the count,
                
                //quicksort the index array with knowing our best plane and assign the according value to left and right and give them their primitives
                //switch pivot to first position
                
                int r = quickSortBVH(primitiveIndex, allprimitives, node, pivot, axis);
                Console.WriteLine((node.first) + "< " + (r) + "<" + (node.first + node.count) + "]] best cost: " + bestCostFormation + ", pivot: " + pivot);
               
                
                //subdivide both beginning with the left node
                BVHNode leftNode = new BVHNode();
                leftNode.isleaf = true;
                leftNode.first = node.first;
                leftNode.count = (r - node.first) + 1;
                
                leftNode.bounds.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                leftNode.bounds.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                //we set the right bounds for our left node
                for (int l = leftNode.first; l < leftNode.count + leftNode.first; l++)
                {
                    leftNode.bounds.adjust(allprimitives[primitiveIndex[l]].box);
                }
                node.left = leftNode;                

                //now the right node
                BVHNode rightNode = new BVHNode();
                rightNode.isleaf = true;
                rightNode.first = r + 1;
                rightNode.count = (node.count + node.first) - rightNode.first;
                
                rightNode.bounds.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                rightNode.bounds.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                //we set the right bounds for our right node
                for (int l = (int)rightNode.first; l < rightNode.count + rightNode.first; l++)
                {
                    rightNode.bounds.adjust(allprimitives[primitiveIndex[l]].box);
                }
                //rightNode.bounds = createAABB(rightNode.first, rightNode.first + rightNode.count, allprimitives, primitiveIndex);
                node.right = rightNode;
                
                node.isleaf = false;

                if (leftNode.count > 0) toProcess.Enqueue(leftNode);
                if (rightNode.count > 0) toProcess.Enqueue(rightNode);
            }            
        }

        //Calculate the best splitplane.
        private static Tuple<float, float> getBestSplitPlane(List<int> primitiveIndex, List<Primitive> allPrimitives, BVHNode node, Vector3 axis)
        {
            float minBound = float.PositiveInfinity;
            float maxBound = float.NegativeInfinity;
            float bestCost = float.PositiveInfinity;
            float pivot = 0;
            
            for (int i = node.first; i < node.first + node.count; i++)
            {
                float pos = Vector3.Dot(axis, allPrimitives[primitiveIndex[i]].position);                
                if (pos < minBound) minBound = pos;
                if (pos > maxBound) maxBound = pos;                
            }            
            float increment = (maxBound - minBound) / 8.0f;
            //Using Binning
            for (int i = 1; i < 7; i++)
            {               
                float split = minBound + (float)i * increment;                
                //we make two boundingboxes
                AABB left;
                AABB right;
                left.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                left.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                right.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                right.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                int leftCount = 0;
                int rightCount = 0;

                //we adjust the bounds of these boundingboxes according to whether or not a primitive is left or right of the splitplane
                for (int j = node.first; j < node.first + node.count; j++)
                {
                    Primitive currentPrimitive = allPrimitives[primitiveIndex[j]];
                    //primitves that are exactly on the plane are assigned to the left of the split plane
                    if (Vector3.Dot(axis, currentPrimitive.position) <= split)
                    {
                        leftCount++;
                        left.adjust(currentPrimitive.box);
                    }
                    else
                    {
                        rightCount++;
                        right.adjust(currentPrimitive.box);
                    }
                }

                float costCurrentFormation = leftCount * left.getSurface() + rightCount * right.getSurface();
                //Console.WriteLine(split + " split, " + costCurrentFormation);
                if (costCurrentFormation <= bestCost)
                {
                    bestCost = costCurrentFormation;
                    pivot = split;                    
                }
            }
            return new Tuple<float,float>(bestCost, pivot);
        }

        private static int quickSortBVH(List<int> primitiveIndex, List<Primitive> allprimitives, BVHNode node, float pivot, Vector3 axis)
        {         
            int low = node.first;
            for (int i = node.first + 1; i < (node.first + node.count); i++)
            {
                //if the primitive is on the left side of the best splitplane we found, then it should be behind our pivot
                if (Vector3.Dot(axis, allprimitives[primitiveIndex[i]].position) < pivot)
                {
                    int temp = primitiveIndex[i];
                    primitiveIndex[i] = primitiveIndex[low];
                    primitiveIndex[low] = temp;
                    low++;
                }                
            }
            return low;
        }
    }



    public struct AABB
    {
        public Vector3 minPoint, maxPoint;

        public float intersect(Ray r)
        {
            //pretty costy and should really be stored for a ray
            float invX = 1.0f / r.Direction.X;
            float invY = 1.0f / r.Direction.Y;
            float invZ = 1.0f / r.Direction.Z;

            float minX = (minPoint.X - r.Origin.X) * invX;
            float maxX = (maxPoint.X - r.Origin.X) * invX;
            float minY = (minPoint.Y - r.Origin.Y) * invY;
            float maxY = (maxPoint.Y - r.Origin.Y) * invY;
            float minZ = (minPoint.Z - r.Origin.Z) * invZ;
            float maxZ = (maxPoint.Z - r.Origin.Z) * invZ;

            float tmin = Math.Max(  Math.Max( Math.Min(minX, maxX), Math.Min(minY, maxY)), Math.Min(minZ, maxZ) );
            float tmax = Math.Min(  Math.Min( Math.Max(minX, maxX), Math.Max(minY, maxY)), Math.Max(minZ, maxZ) );

            if (tmax >= tmin) return tmin;
            return 0;
            
        }

        //adjusts this aabb to the union of itself and the parameter box
        public void adjust(AABB box)
        {
            for (int i = 0; i < 3; i++)
            {
                minPoint[i] = Math.Min(box.minPoint[i], minPoint[i]);
                maxPoint[i] = Math.Max(box.maxPoint[i], maxPoint[i]);
            }
        }

        public float getSurface()
        {
            float length = maxPoint.X - minPoint.X;
            float height = maxPoint.Y - minPoint.Y;
            float width = maxPoint.Z - minPoint.Z;
            return 2.0f * ((length * height) + (width * length) + (width * height));
        }

        internal bool inAABB(Vector3 vector)
        {
            for (int i = 0; i < 3; i++)
            {
                if (vector[i] < minPoint[i] || vector[i] > maxPoint[i]) return false;
            }
            return true;
        }
    }

}
