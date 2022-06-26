using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ICD.ABM.DigitalFutures22.Core.Agent
{
    public class PanelAgent : AgentBase
    {
        public Point3d Position = Point3d.Unset;
        public Point2d UV = Point2d.Unset;
        public Point2d startUV;
        public Polyline Trail = new Polyline();
        public Polyline Cell = new Polyline();
        public List<PanelAgent> NeighborsOnRail = new List<PanelAgent>();
        public List<PanelAgent> DirectNeighborsOnRail = new List<PanelAgent>();
        public Brep Rail = null;
        public bool IsEdge;

        public bool isFinished = false;

        /// <summary>
        /// The list of 2-dimensional moves
        /// </summary>
        public List<Vector2d> Moves = new List<Vector2d>();
        public List<double> Weights = new List<double>();

        public PanelAgent(Point3d position, List<BehaviorBase> behaviors)
        {
            this.Position = position;
            //this.UV = this.startUV = uvParameter;
            this.Behaviors = behaviors;
        }

        public override void Reset()
        {
            this.Moves.Clear();
            this.UV = this.startUV;
            this.Trail.Clear();
            this.isFinished = false;
            this.Rail = null;

            FindRail();
            FindNeighborsOnRail();
            FindDirectNeighborsOnRail();
        }

        public override void PreExecute()
        {
            this.Moves.Clear();

            //this.Position = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(this.UV.X, this.UV.Y);
            this.Rail = null;
            FindRail();

            FindNeighborsOnRail();
            FindDirectNeighborsOnRail();
        }

        public override void Execute()
        {
            if (this.isFinished)
                return;

            foreach (BehaviorBase behavior in this.Behaviors)
                behavior.Execute(this);
        }

        public override void PostExecute()
        {
            if (this.Moves.Count > 0)
            {
                Vector2d totalWeightedMove = Vector2d.Zero;
                double totalWeight = 0.0;

                for (int i = 0; i < Moves.Count; ++i)
                {
                    totalWeightedMove += Weights[i] * Moves[i];
                    totalWeight += Weights[i];
                }
                this.UV += totalWeightedMove / totalWeight;
            }

            this.Trail.Add((this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(UV.X, UV.Y));
            if (Trail.Count > 20)
                Trail.RemoveAt(0);

            this.Position = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(UV.X, UV.Y);

            // constrain agent to stay on brep
            Point3d cloPt = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.ClosestPoint(this.Position);

            if (this.Position.DistanceTo(cloPt) > Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
            {
                this.Position = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.ClosestPoint(this.Position);

                Point3d UV = (this.AgentSystem as PanelAgentSystem).RailEnvironment.UVCoordinates(this.Position);
                this.UV = new Point2d(UV.X, UV.Y);
            }
        }

        public override List<object> GetDisplayGeometries()
        {
            List<object> displayGeometry = new List<object>();

            //if ((this.AgentSystem as UVAgentSystem).SystemSurface == null)
            //    return displayGeometry;

            displayGeometry.Add((this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(UV.X, UV.Y));

            if ((this.Trail.Count < 2))
                return displayGeometry;

            displayGeometry.Add(this.Trail);
            displayGeometry.Add(this.Cell);

            return displayGeometry;
        }

        /// <summary>
        /// Find all agents that are topologically connected to a given agent
        /// </summary>
        /// <param name="agent">The agent to search from.</param>
        /// <returns>Returns the list of topologically connected neighboring agents.</returns>
        public List<PanelAgent> FindTopologicalNeighbors()
        {
            List<PanelAgent> panelAgentList = new List<PanelAgent>();

            List<int> connections = (this.AgentSystem as PanelAgentSystem).diagram.GetConnections(this.Id);

            foreach (int index in connections)
            {
                panelAgentList.Add((PanelAgent)((this.AgentSystem as PanelAgentSystem).Agents[index]));
            }

            return panelAgentList;
        }

        public void FindRail()
        {
            foreach (Brep rail in (this.AgentSystem as PanelAgentSystem).RailEnvironment.Rails)
            {
                Point3d cloPt = rail.ClosestPoint(this.Position);

                if (this.Position.DistanceTo(cloPt) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                {
                    this.Rail = rail;
                }
            }
        }

        public void FindNeighborsOnRail()
        {
            List<PanelAgent> topoNeighbors = FindTopologicalNeighbors();

            List<PanelAgent> neighborsOnRail = new List<PanelAgent>();

            foreach (PanelAgent otherAgent in topoNeighbors)
            {
                Point3d cloPt = this.Rail.ClosestPoint(otherAgent.Position);

                if (otherAgent.Position.DistanceTo(cloPt) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                {
                    neighborsOnRail.Add(otherAgent);
                }
            }
            this.NeighborsOnRail = neighborsOnRail;
        }


        public void FindDirectNeighborsOnRail()
        {
            List<PanelAgent> directNeighborsOnRail = new List<PanelAgent>();

            List<double> distances = new List<double>();

            foreach (PanelAgent otherAgent in this.NeighborsOnRail)
            {

                distances.Add(this.Position.DistanceTo(otherAgent.Position));
            }

            double minVal = distances.Min();
            int index = distances.IndexOf(minVal);

            directNeighborsOnRail.Add(this.NeighborsOnRail[index]);

            // if agents are not on the outer edge, they should have another neighbor
            if (!this.IsEdge)
            {
                var secondLowest = distances.OrderBy(num => num).ElementAt(0); // should be 1 not zero, just debugging
                int indexSecondLowest = distances.IndexOf(secondLowest);
                directNeighborsOnRail.Add(this.NeighborsOnRail[indexSecondLowest]);
            }

            this.DirectNeighborsOnRail = directNeighborsOnRail;
        }
    }
}
