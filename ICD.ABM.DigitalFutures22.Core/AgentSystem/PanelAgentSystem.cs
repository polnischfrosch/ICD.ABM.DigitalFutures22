using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.Environments;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Core.AgentSystem
{
    public class PanelAgentSystem : AgentSystemBase
    {
        public RailEnvironment RailEnvironment = null;

        public Mesh SystemMesh;
        public Mesh StartMesh;
        public bool[] isNaked;

        private List<Point3d> uvs;
        private Node2List nodes;
        private Node2List outline;

        /// <summary>
        /// The connectivity diagram, i.e. interaction topology, of the agent system.
        /// </summary>
        public Connectivity diagram = null;

        public PanelAgentSystem() : base()
        {
        }

        public PanelAgentSystem(List<PanelAgent> agents, RailEnvironment railEnvironment)
        {
            RailEnvironment = railEnvironment;
            Agents = new List<AgentBase>();

            for (int i = 0; i < agents.Count; ++i)
            {
                agents[i].Id = i;
                agents[i].AgentSystem = this;
                agents[i].UV = agents[i].startUV = new Point2d(RailEnvironment.UVCoordinates(agents[i].Position).X, RailEnvironment.UVCoordinates(agents[i].Position).Y);
                Agents.Add(agents[i]);
            }

            // Step 2: Compute the interaction network, i.e. the mesh
            this.SystemMesh = this.StartMesh = computeConnectivityMesh(); // compute the mesh only once at initialization
            isNaked = SystemMesh.GetNakedEdgePointStatus();

            RhinoApp.WriteLine("A new agent system has been created");
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void PreExecute()
        {
            //base.PreExecute();
            this.SystemMesh = computeConnectivityMesh();
            ComputeCells();
            FindNeighborsOnRail();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void PostExecute()
        {
            base.PostExecute();
        }

        public override List<object> GetDisplayGeometries()
        {
            List<object> displayGeometry = new List<object>
            {
                SystemMesh
            };

            foreach (PanelAgent agent in Agents)
                displayGeometry.AddRange(agent.GetDisplayGeometries());

            return displayGeometry;
        }

        /// <summary>
        /// Find all agents that are within the given straight-line distance of the given agent
        /// </summary>
        /// <param name="agent">The agent to search from.</param>
        /// <param name="distance">The search distance.</param>
        /// <returns>Returns a list containing all neighboring agents within the search distance.</returns>
        public List<PanelAgent> FindNeighbors(PanelAgent agent, double distance)
        {
            List<PanelAgent> panelAgentList = new List<PanelAgent>();
            foreach (PanelAgent otherAgent in this.Agents)
            {
                if (agent != otherAgent && agent.UV.DistanceTo(otherAgent.UV) < distance)
                    panelAgentList.Add(otherAgent);
            }

            return panelAgentList;
        }

        /// <summary>
        /// Find all agents that are topologically connected to a given agent
        /// </summary>
        /// <param name="agent">The agent to search from.</param>
        /// <returns>Returns the list of topologically connected neighboring agents.</returns>
        public List<PanelAgent> FindTopologicalNeighbors(PanelAgent agent)
        {
            List<PanelAgent> panelAgentList = new List<PanelAgent>();

            List<int> connections = diagram.GetConnections(agent.Id);

            foreach (int index in connections)
            {
                panelAgentList.Add((PanelAgent)(this.Agents[index]));
            }

            return panelAgentList;
        }

        private Mesh computeConnectivityMesh()
        {
            uvs = new List<Point3d>();
            nodes = new Node2List();
            SystemMesh = new Mesh();

            foreach (PanelAgent agent in Agents)
            {
                Point3d uvCoordinates = RailEnvironment.UVCoordinates(agent.Position);
                uvs.Add(uvCoordinates);
                //uvs.Add(agent.Position);
                nodes.Append(new Node2(agent.UV.X, agent.UV.Y));
                SystemMesh.Vertices.Add(RailEnvironment.BrepObject.Surfaces[0].PointAt(agent.UV.X, agent.UV.Y));
            }

            //List<Grasshopper.Kernel.Geometry.Delaunay.Face> faces =
            //    Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(
            //        new Node2List(uvs),
            //        Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(
            //    new Node2List(uvs),
            //    Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);

            List<Grasshopper.Kernel.Geometry.Delaunay.Face> faces =
                Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(
                    nodes,
                    Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(
                nodes,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);

            foreach (var face in faces)
            {
                SystemMesh.Faces.AddFace(face.A, face.B, face.C);
            }

            return SystemMesh;
        }

        private void computeVoronoi()
        {
            List<Grasshopper.Kernel.Geometry.Voronoi.Cell2> cells = Grasshopper.Kernel.Geometry.Voronoi.Solver.Solve_Connectivity(nodes, diagram, outline);

            for (int i = 0; i < cells.Count; i++)
            {
                List<Point3d> cell3dPts = new List<Point3d>();

                foreach (Point3d pt in cells[i].ToPolyline())
                {
                    cell3dPts.Add(RailEnvironment.BrepObject.Surfaces[0].PointAt(pt.X, pt.Y));
                }
                Polyline cell3d = new Polyline(cell3dPts);

                (Agents[i] as PanelAgent).Cell = cell3d;
            }
        }

        public void ComputeCells()
        {
            Polyline poly;
            PolylineCurve polyCrv;

            List<Curve> crvs = RailEnvironment.BoundaryCurves2D();
            Curve[] joinedCrvs = Curve.JoinCurves(crvs);

            polyCrv = joinedCrvs[0].ToPolyline(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                Rhino.RhinoDoc.ActiveDoc.ModelAngleToleranceRadians,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                100000);
            polyCrv.TryGetPolyline(out poly);

            if (!polyCrv.TryGetPolyline(out poly))
            {
                throw new Exception("Failed to get the polyline");
            }
            outline = new Node2List(poly);

            computeConnectivityMesh();
            computeVoronoi();
        }

        public void FindNeighborsOnRail()
        {
            foreach (PanelAgent agent in Agents)
            {
                List<PanelAgent> topoNeighbors = FindTopologicalNeighbors(agent);

                List<PanelAgent> neighborsOnRail = new List<PanelAgent>();

                foreach (PanelAgent otherAgent in topoNeighbors)
                {
                    LineCurve ln = new LineCurve(agent.UV, otherAgent.UV);

                    // TO DO: get railcurves from rail brep to avoid having both inputs
                    foreach (Curve railC in RailEnvironment.RailCurves)
                    {
                        Curve crv = RailEnvironment.BrepObject.Surfaces[0].Pullback(railC, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                        var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(crv, ln, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                        if (events.Count < 1)
                        {
                            neighborsOnRail.Add(otherAgent);
                        }
                    }
                }
                agent.NeighborsOnRail = neighborsOnRail;
            }
        }

        ///// <summary>
        ///// Computes the Delaunay mesh that correponds to the agent positions.
        ///// </summary>
        ///// <returns>Mesh</returns>
        //private Mesh ComputeMesh()
        //{
        //    Node2List nodes = new Node2List();
        //    foreach (PanelAgent agent in this.Agents)
        //        nodes.Append(new Node2(agent.UV.X, agent.UV.Y));

        //    List<Face> faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(nodes, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
        //    Mesh mesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, ref faces);

        //    mesh.Normals.ComputeNormals();
        //    mesh.Compact();
        //    mesh.Faces.CullDegenerateFaces();

        //    return mesh;
        //}
    }
}
