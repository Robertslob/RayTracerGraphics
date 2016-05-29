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

        //Assuming allPrimitives is ordered by their x location.
        public static void subdivide(BVHNode root, List<int> primitiveIndex, List<Primitive> allprimitives, int ownNodeIndex)
        {
            //if a node has less than 3 primitives it is a leaf            
            uint nodeCounter = 0;
            Queue<BVHNode> toProcess = new Queue<BVHNode>();
            toProcess.Enqueue(root);
            while (toProcess.Count > 0)
            {
                nodeCounter++;
                BVHNode node = toProcess.Dequeue();
                if (node.count <= 7)
                {                    
                    continue;
                }

                Console.WriteLine(node.count);
                float bestCostFormation = node.bounds.getSurface() * node.count;
                int bestPrimitiveOfPlane = 0;
                bestPrimitiveOfPlane = node.first + (node.count >> 1);
                //foreach splitplane that we can imagine with the primitives of this node
                
                bool x = getBestSplitPlane(primitiveIndex, allprimitives, node, ref bestCostFormation, ref bestPrimitiveOfPlane, Vector3.UnitX);
                bool y = getBestSplitPlane(primitiveIndex, allprimitives, node, ref bestCostFormation, ref bestPrimitiveOfPlane, Vector3.UnitY);
                bool z = getBestSplitPlane(primitiveIndex, allprimitives, node, ref bestCostFormation, ref bestPrimitiveOfPlane, Vector3.UnitZ);

                int r;
                //quicksort the index array with knowing our best plane and assign the according value to left and right and give them their primitives
                //switch pivot to first position
                if (z)
                {
                    r = quickSortBVH(primitiveIndex, allprimitives, node, bestPrimitiveOfPlane, Vector3.UnitZ);
                }
                else if (y)
                {
                    r = quickSortBVH(primitiveIndex, allprimitives, node, bestPrimitiveOfPlane, Vector3.UnitY);
                }
                else 
                {
                    r = quickSortBVH(primitiveIndex, allprimitives, node, bestPrimitiveOfPlane, Vector3.UnitX);
                }
                
                //int r = quickSortBVH(primitiveIndex, allprimitives, node, bestPrimitiveOfPlane);

                //subdivide both beginning with the left node
                BVHNode leftNode = new BVHNode();
                leftNode.isleaf = true;
                leftNode.first = node.first;
                leftNode.count = (r - node.first) + 1;

                leftNode.bounds.minPoint = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                leftNode.bounds.maxPoint = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

                //we set the right bounds for our left node
                for (int l = (int)leftNode.first; l < leftNode.count + leftNode.first; l++)
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

                node.right = rightNode;
                
                node.isleaf = false;

                if (leftNode.count > 0) toProcess.Enqueue(leftNode);
                if (rightNode.count > 0) toProcess.Enqueue(rightNode);
            }            
        }

        private static bool getBestSplitPlane(List<int> primitiveIndex, List<Primitive> allprimitives, BVHNode node, ref float bestCostFormation, ref int bestPrimitiveOfPlane, Vector3 axis)
        {
            bool better = false;
            for (int i = node.first; i < node.first + node.count; i++)
            {
                //we get the x value of a splitplane
                float split = Vector3.Dot(axis, allprimitives[primitiveIndex[(int)i]].position);

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
                    Primitive currentPrimitive = allprimitives[primitiveIndex[(int)j]];
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

                if (costCurrentFormation < bestCostFormation)
                {
                    bestCostFormation = costCurrentFormation;
                    bestPrimitiveOfPlane = (int)i;
                    better = true;
                }
            }
            return better;
        }

        private static int quickSortBVH(List<int> primitiveIndex, List<Primitive> allprimitives, BVHNode node, int bestPrimitiveOfPlane, Vector3 axis)
        {
            int temp = primitiveIndex[bestPrimitiveOfPlane];
            primitiveIndex[bestPrimitiveOfPlane] = primitiveIndex[(int)node.first]; 
            primitiveIndex[(int)node.first] = temp;
            int r = (int)node.first; 
            int s = (int)(node.first + node.count) - 1;
            float bestSplitX = Vector3.Dot(axis, allprimitives[primitiveIndex[r]].position);

            while (r < s)
            {
                //if the primitive is on the left side of the best splitplane we found, then it should be behind our pivot
                if (Vector3.Dot(axis,allprimitives[primitiveIndex[r + 1]].position) <= bestSplitX)
                {
                    temp = primitiveIndex[r]; primitiveIndex[r] = primitiveIndex[r + 1]; primitiveIndex[r + 1] = temp;
                    r++;
                }
                else
                {
                    temp = primitiveIndex[r + 1]; primitiveIndex[r + 1] = primitiveIndex[s]; primitiveIndex[s] = temp;
                    s--;
                }
            }
            return r;
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
            return -1;
            
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
    }

}
