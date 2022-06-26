using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICD.ABM.DigitalFutures22.Core.Behavior
{
    public class BhvCohesion : BehaviorBase
    {
        public double Distance;
        public double CohesionWeight;

        public BhvCohesion(double weight, double distance)
        {
            CohesionWeight = weight;
            Distance = distance;
        }

        public override void Execute(AgentBase agent)
        {
            DFAgent dfAgent = agent as DFAgent;
            DFAgentSystem dfAgentSystem = dfAgent.AgentSystem as DFAgentSystem;

            List<DFAgent> neighbors = dfAgentSystem.FindNeighbors(dfAgent, Distance);

            if (neighbors.Count == 0) return;

            // get average of neighbour positions
            Point3d center = Point3d.Origin;
            foreach (DFAgent neighbor in neighbors)
                center += neighbor.Position;
            center /= neighbors.Count;

            // move to average of neighbour positions
            Vector3d moveToCenter = center - dfAgent.Position;
            //moveToCenter.Unitize();

            dfAgent.Moves.Add(moveToCenter);
            dfAgent.Weights.Add(CohesionWeight);
        }
    }
}
