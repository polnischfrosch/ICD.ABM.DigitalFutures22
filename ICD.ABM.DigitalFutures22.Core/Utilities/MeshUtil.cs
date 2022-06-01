using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICD.ABM.DigitalFutures22.Core.Utilities
{
    public static class MeshUtil
    {
        public static List<Polyline> Dual(Mesh mesh, int type = 0, Surface surface = null, List<Point3d> pts = null)
        {
            Mesh x = mesh.DuplicateMesh();

            bool hasOnlyTriangleQuadNgons = true;
            for (int i = 0; i < x.Ngons.Count; i++)
                if (x.Ngons[i].BoundaryVertexCount > 4)
                {
                    hasOnlyTriangleQuadNgons = false;
                    break;
                }
            if (hasOnlyTriangleQuadNgons)
                x.Ngons.Clear();

            int Type = Math.Abs(type);

            Mesh dual = new Mesh();
            List<Polyline> polylines = new List<Polyline>();

            if (surface != null)
            {
                Surface s = surface;
                s.SetDomain(0, new Interval(0, 1));
                s.SetDomain(1, new Interval(0, 1));
            }

            if (x.Ngons.Count == 0)
            {
                //Get face center
                int count = 0;

                foreach (MeshFace mf in x.Faces)
                {
                    Point3d barycenter = Point3d.Origin;

                    if (pts != null)
                        if (pts.Count == x.Faces.Count)
                            Type = 2;

                    switch (Type)
                    {

                        case (2):
                            barycenter = pts[count];
                            break;

                        case (1):
                            if (mf.IsQuad)
                            {
                                barycenter = new Polyline(new Point3d[] { x.Vertices[mf.A], x.Vertices[mf.B], x.Vertices[mf.C], x.Vertices[mf.D], x.Vertices[mf.A] }).CenterPoint();
                            }
                            else
                            {
                                barycenter = new Polyline(new Point3d[] { x.Vertices[mf.A], x.Vertices[mf.B], x.Vertices[mf.C], x.Vertices[mf.A] }).CenterPoint();
                            }
                            break;

                        default:

                            barycenter += (x.Vertices[mf.A]);
                            barycenter += (x.Vertices[mf.B]);
                            barycenter += (x.Vertices[mf.C]);

                            if (mf.IsQuad)
                            {
                                barycenter += (x.Vertices[mf.D]);
                                barycenter /= 4;
                            }
                            else
                            {
                                barycenter /= 3;
                            }
                            break;
                    }
                    dual.Vertices.Add(barycenter);
                    count++;
                }

                //Apply dual
                //List<Polyline> polylines = new List<Polyline>();
                bool[] nakedv = x.GetNakedEdgePointStatus();
                Point3d[] midPoints = EdgeMidPoints(x);
                dual.Vertices.AddVertices(midPoints);

                for (int i = 0; i < x.Vertices.Count; i++)
                {

                    if (nakedv[i] && type < 0)
                        continue;

                    int vID = x.TopologyVertices.TopologyVertexIndex(i);
                    int[] vf = x.TopologyVertices.ConnectedFaces(vID);


                    int[] sortedFaces = SortFacesConnectedToTopologyVertex(x, vf);

                    //You can select vertices from dual mesh
                    //Or if you calculate barycenter here, then you will have duplicate points

                    if (vf.Length > 0)
                    {
                        Polyline polyline = new Polyline();
                        List<int> id = new List<int>();

                        for (int j = 0; j < sortedFaces.Length; j++)
                        {
                            polyline.Add(dual.Vertices[sortedFaces[j]]);
                            id.Add(sortedFaces[j]);
                        }

                        if (nakedv[i])
                        {
                            Point3d lastPoint = polyline.Last();
                            List<int> nakedEdges = new List<int>();
                            List<Point3d> nakedEdgesP = new List<Point3d>();
                            List<double> nakedEdgesPDistToLastP = new List<double>();
                            List<int> nakedFaces = new List<int>();

                            int[] edges = x.TopologyVertices.ConnectedEdges(vID);
                            int numberOfNakedV = 0;
                            foreach (int edgeID in edges)
                            {
                                int[] edgeFaces = x.TopologyEdges.GetConnectedFaces(edgeID);
                                if (edgeFaces.Length == 1)
                                {
                                    numberOfNakedV++;
                                    nakedFaces.Add(edgeFaces[0]);
                                    nakedEdges.Add(edgeID);
                                    nakedEdgesP.Add(midPoints[edgeID]);
                                    // nakedEdgesPDistToLastP.Add(lastPoint.DistanceToSquared(midPoints[edgeID]));
                                }
                            }

                            if (nakedFaces.Count == 2)
                            {
                                if (id.Last() == nakedFaces[0])
                                {
                                    polyline.Add(nakedEdgesP[0]);

                                    if (vf.Length <= 10)
                                        polyline.Add(x.Vertices[vID]);

                                    polyline.Add(nakedEdgesP[1]);

                                    //id.Add(nakedFaces[0]);
                                    //id.Add(nakedFaces[1]);
                                }
                                else
                                {
                                    polyline.Add(nakedEdgesP[1]);
                                    if (vf.Length <= 10)
                                        polyline.Add(x.Vertices[vID]);

                                    polyline.Add(nakedEdgesP[0]);
                                    //id.Add(nakedFaces[1]);
                                    //id.Add(nakedFaces[0]);
                                }
                            }
                        }
                        polyline.Add(polyline[0]);
                        polylines.Add(polyline);
                    }
                }
            }
            return polylines;
        }

        public static int[] SortFacesConnectedToTopologyVertex(Mesh mesh, int[] vf)
        {
            int n = vf.Length;
            //Print(n.ToString());

            //Can be only one face connected to naked edge or two
            //These cases does not form  mesh face
            if (n <= 2)
                return vf;

            var vfList = vf.ToList();

            //Start with one of naked faces
            for (int i = 0; i < n; i++)
            {

                int[] e = mesh.TopologyEdges.GetEdgesForFace(vf[i]);
                for (int j = 0; j < e.Length; j++)
                {

                    int[] ef = mesh.TopologyEdges.GetConnectedFaces(e[j]);

                    if (ef.Length == 1)
                    {
                        int naked = vf[i];
                        vfList.RemoveAt(i);
                        vfList.Insert(0, naked);
                        break;
                    }
                }
            }

            vf = vfList.ToArray();

            //Output
            int[] sortedFaceID = new int[n];
            sortedFaceID[0] = vf[0];

            //Face vertices
            //flattened array 0   1  2  3  4
            //                19 25 26 29 34
            int[][] faceE = new int[n][];
            for (int i = 0; i < n; i++)
                faceE[i] = mesh.TopologyEdges.GetEdgesForFace(vf[i]);

            //Visited list
            List<int> notvisited = Enumerable.Range(1, n - 1).ToList();

            int counter = 1;
            int lastId = 0;

            //n-1 because last step is done below
            for (int i = 1; i < n - 1; i++)
            {
                int k = 0;
                foreach (int j in notvisited)
                {

                    var intersection = Enumerable.Intersect(faceE[lastId], faceE[j]);

                    if (intersection.Count() != 0)
                    {
                        sortedFaceID[counter++] = vf[j];
                        lastId = j;
                        notvisited.RemoveAt(k);
                        break;
                    }
                    k++;

                }
            }

            //the last item, may not have shared edges for cases of naked edges
            sortedFaceID[n - 1] = vf[notvisited[0]];


            return sortedFaceID;

        }

        public static Point3d[] EdgeMidPoints(Mesh mesh)
        {
            Point3d[] pts = new Point3d[mesh.TopologyEdges.Count];

            for (int i = 0; i < mesh.TopologyEdges.Count; i++)
                pts[i] = mesh.TopologyEdges.EdgeLine(i).PointAt(0.5);

            return pts;
        }
    }
}
