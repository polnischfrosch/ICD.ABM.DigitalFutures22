using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Geometry.Voronoi;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICD.ABM.DigitalFutures22.Core.AgentSystem
{
    public class DFAgentSystem : AgentSystemBase
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

        public DFAgent manipulatedAgent = null;
        public SingleBrepEnvironment SingleBrepEnvironment;


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
                Node2List nodes = new Node2List();
                foreach (CartesianAgent agent in this.Agents)
                    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                List<Node2> node2List = new List<Node2>();
                foreach (Point3d boundaryCorner in this.CartesianEnvironment.BoundaryCorners)
                    node2List.Add(new Node2(boundaryCorner.X, boundaryCorner.Y));
                this.VoronoiCells = Grasshopper.Kernel.Geometry.Voronoi.Solver.Solve_Connectivity(nodes, diagram, (IEnumerable<Node2>)node2List);
            }

            if (ComputeDelaunayConnectivity)
            {
                Node2List nodes = new Node2List();
                foreach (DFAgent agent in this.Agents)
                    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
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

            foreach (DFAgent agent in Agents)
                displayGeometry.AddRange(agent.GetDisplayGeometries());

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
    }
}
