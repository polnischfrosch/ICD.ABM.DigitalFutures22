using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Geometry.Voronoi;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using static ICD.ABM.DigitalFutures22.Core.Utilities.MeshUtil;

namespace ICD.ABM.DigitalFutures22.Core.AgentSystem
{
    public partial class DFAgentSystem : AgentSystemBase
    {
        /// <summary>
        /// The list of Voronoi cells associated with each agent.
        /// </summary>
        public List<Cell2> VoronoiCells = new List<Cell2>();
        /// <summary>
        /// The connectivity diagram, i.e. interaction topology, of the agent system.
        /// </summary>
        public Connectivity diagram = null;
        /// <summary>
        /// The field to access the Cartesian environment of this agent system.
        /// </summary>
        public CartesianEnvironment CartesianEnvironment;
        /// <summary>
        /// Boolean toggle to determine if Voronoi diagram should be computed for this system.
        /// </summary>
        public bool ComputeVoronoiCells = false;
        /// <summary>
        /// Boolean toggle to determine if only the connectivity diagram should be computed for this system.
        /// </summary>
        public bool ComputeDelaunayConnectivity = false;


        public SingleBrepEnvironment SingleBrepEnvironment;
        public Mesh Mesh;

        private List<Point3d> uvs;
        private Node2List nodes;

        private Node2List outline;

        public List<Polyline> DualPolylines;

        public DFAgent manipulatedAgent = null;

        public DFAgentSystem(List<DFAgent> agents) //, CartesianEnvironment cartesianEnvironment)
        {
            //this.CartesianEnvironment = cartesianEnvironment;
            this.Agents = new List<AgentBase>();
            //Agents = new List<CartesianAgent>();
            for (int i = 0; i < agents.Count; ++i)
            {
                //agents[i].Id = i;
                agents[i].AgentSystem = this;
                this.Agents.Add((AgentBase)agents[i]);
            }
        }

        public DFAgentSystem() : base()
        {
        }

        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();
        }

        /// <inheritdoc />
        public override void PreExecute()
        {
            if (ComputeVoronoiCells)
            {
                //Node2List nodes = new Node2List();
                //foreach (CartesianAgent agent in this.Agents)
                //    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                //diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                //List<Node2> node2List = new List<Node2>();
                //foreach (Point3d boundaryCorner in this.CartesianEnvironment.BoundaryCorners)
                //    node2List.Add(new Node2(boundaryCorner.X, boundaryCorner.Y));
                //this.VoronoiCells = Grasshopper.Kernel.Geometry.Voronoi.Solver.Solve_Connectivity(nodes, diagram, (IEnumerable<Node2>)node2List);
                ComputePlates();
            }

            if (ComputeDelaunayConnectivity)
            {
                //Node2List nodes = new Node2List();
                //foreach (DFAgent agent in this.Agents)
                //    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                //diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                computeConnectivityMesh();
            }

            foreach (AgentBase agent in this.Agents)
                agent.PreExecute();
        }


        /// <summary>
        /// Method for collecting the geometry that should be displayed.
        /// </summary>+
        /// <returns>Returns null. Needs to be overridden.</returns>
        public override List<object> GetDisplayGeometries()
        {
            List<object> displayGeometry = new List<object>();

            //foreach (DFAgent agent in Agents)
            //    displayGeometry.AddRange(agent.GetDisplayGeometries());

            //displayGeometry.Add(Mesh);

            return displayGeometry;
        }


        /// <summary>
        /// Find all agents that are within the given straight-line distance of the given agent
        /// </summary>
        /// <param name="agent">The agent to search from.</param>
        /// <param name="distance">The search distance.</param>
        /// <returns>Returns a list containing all neighboring agents within the search distance.</returns>
        public List<DFAgent> FindNeighbors(DFAgent agent, double distance)
        {
            List<DFAgent> cartesianAgentList = new List<DFAgent>();
            foreach (DFAgent otherAgent in this.Agents)
            {
                if (agent != otherAgent && agent.Position.DistanceTo(otherAgent.Position) < distance)
                    cartesianAgentList.Add(otherAgent);
            }

            return cartesianAgentList;
        }

        /// <summary>
        /// Find all agents that are topologically connected to a given agent
        /// </summary>
        /// <param name="agent">The agent to search from.</param>
        /// <returns>Returns the list of topologically connected neighboring agents.</returns>
        public List<DFAgent> FindTopologicalNeighbors(DFAgent agent)
        {
            List<DFAgent> cartesianAgentList = new List<DFAgent>();

            List<int> connections = diagram.GetConnections(agent.Id);

            foreach (int index in connections)
            {
                cartesianAgentList.Add((DFAgent)(this.Agents[index]));
            }

            return cartesianAgentList;
        }

        public void ComputePlates()
        {
            Polyline poly;
            PolylineCurve polyCrv;

            List<Curve> crvs = SingleBrepEnvironment.BoundaryCurves2D();
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
            computePlates();
        }

        private void computeConnectivityMesh()
        {
            uvs = new List<Point3d>();
            nodes = new Node2List();
            Mesh = new Mesh();

            foreach (DFAgent agent in Agents)
            {
                Point3d uvCoordinates = SingleBrepEnvironment.UVCoordinates(agent.Position);
                uvs.Add(uvCoordinates);
                //uvs.Add(agent.Position);
                nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                Mesh.Vertices.Add(agent.Position);
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
                Mesh.Faces.AddFace(face.A, face.B, face.C);
            }
        }

        private void computePlates()
        {
            List<Grasshopper.Kernel.Geometry.Voronoi.Cell2> cells = Grasshopper.Kernel.Geometry.Voronoi.Solver.Solve_Connectivity(nodes, diagram, outline);

            DualPolylines = Dual(Mesh, 0, null, null);

            for (int i = 0; i < cells.Count; i++)
            {
                (Agents[i] as DFAgent).PlatePolyline = cells[i].ToPolyline();
            }
        }
    }
}
