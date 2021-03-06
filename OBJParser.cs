﻿using Application;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace template
{
    class OBJParser
    {
        //this function is copied from here:
        //http://www.rexcardan.com/2014/10/read-obj-file-in-c-in-just-10-lines-of-code/
        //Not all, but many of, credits go to the guy who really wrote this...
        public static List<Triangle> readOBJ(String file, Vector3 pos, float scale, Material material)
        {
            //setUpCulture();

            String[] lines = File.ReadAllLines(file);
            List<Triangle> triangles = new List<Triangle>();
            Vector3[] vectors = getVectors(lines);
            Console.WriteLine("Loaded vectors: " + vectors.Length);
            Vector3[] normalVectors = getNormalVectors(lines);
            Console.WriteLine("Loaded normals: " + normalVectors.Length);
            return getTriangles(pos, scale, material, lines, vectors, normalVectors);
        }

        //Set up culture for float
        private static void setUpCulture() {
            System.Globalization.CultureInfo customCulter = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulter.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulter;

        }

        private static Vector3[] getVectors(String[] lines)
        {
            Vector3[] verts = lines.Where(l => Regex.IsMatch(l, @"^v(\s+-?\d+(\.?\d+([eE][-+]?\d+)?)?){3,3}$"))
                .Select(l => Regex.Split(l, @"\s+", RegexOptions.None).Skip(1).ToArray()) //Skip v
                .Select(nums => new Vector3(float.Parse(nums[0], CultureInfo.InvariantCulture), float.Parse(nums[1], CultureInfo.InvariantCulture), float.Parse(nums[2], CultureInfo.InvariantCulture)))
                .ToArray();
            return verts;
        }
        private static Vector3[] getNormalVectors(String[] lines)
        {
            Vector3[] verts = lines.Where(l => Regex.IsMatch(l, @"^vn(\s+-?\d+\.?\d+([eE][-+]?\d+)?){3,3}$"))
                .Select(l => Regex.Split(l, @"\s+", RegexOptions.None).Skip(1).ToArray()) //Skip v
                .Select(nums => new Vector3(float.Parse(nums[0], CultureInfo.InvariantCulture), float.Parse(nums[1], CultureInfo.InvariantCulture), float.Parse(nums[2], CultureInfo.InvariantCulture)))
                .ToArray();
            return verts;
        }
        private static List<Triangle> getTriangles(Vector3 position, float scale, Material material, String[] lines, Vector3[] vectors, Vector3[] normals)
        {
            String[][] faces = lines.Where(l => Regex.IsMatch(l, @"^f(\s\d+(\/\d*)?(\/\d*)?){3,}$"))
                .Select(l => Regex.Split(l, @"\s+", RegexOptions.None).Skip(1).ToArray())//Skip f               
                .ToArray();

            List<Triangle> triangles = new List<Triangle>();
            int maxleng = 0;
            foreach (String[] line in faces)
            {                

                var tupl = parseVector(line[0], vectors, normals);
                Vector3 baseVector = tupl.Item1;
                Vector3 baseNormal = tupl.Item2;
                var tupl1 = parseVector(line[1], vectors, normals);
                Vector3 lastVector = tupl1.Item1;
                Vector3 lastNormal = tupl1.Item2;
                //if (line.Length > maxleng) maxleng = line.Length;

                //Console.WriteLine(position + scale * baseVector);
                for (int i = 2; i < line.Length; i++)
                {
                    var tupl2 = parseVector(line[i], vectors, normals);
                    Vector3 vec = tupl2.Item1;
                    Vector3 norm = tupl2.Item2;
                    Vector3 trueNormal = (norm + lastNormal + baseNormal) * 0.33333f;
                    Triangle tri = new Triangle(position + scale * baseVector, position + scale * lastVector, position + scale * vec, trueNormal, material);
                    triangles.Add(tri);
                    
                    lastVector = vec;
                    lastNormal = norm;
                }
            }
            Console.WriteLine("loaded polygons: " + triangles.Count + ", " + maxleng);
            return triangles;
        }
        private static Tuple<Vector3,Vector3> parseVector(String str, Vector3[] vectors, Vector3[] normals)
        {
            String[] p = str.Split('/');
            if (p.Length <= 2) return new Tuple<Vector3, Vector3>(vectors[int.Parse(p[0]) - 1], Vector3.Zero);
            //return new Tuple<Vector3, Vector3>(vectors[int.Parse(p[0]) - 1], normals[int.Parse(p[2]) - 1]);
            return new Tuple<Vector3, Vector3>(vectors[int.Parse(p[0]) - 1], Vector3.Zero);
        }
    }
}
