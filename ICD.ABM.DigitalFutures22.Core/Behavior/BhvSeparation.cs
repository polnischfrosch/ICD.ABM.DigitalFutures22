using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICD.ABM.DigitalFutures22.Core.Behavior
{
    public class BhvSeparation : BehaviorBase
    {
        public double Distance;
        public double Power;
        public bool AffectSelf;
        public double SeparationWeight;

        public BhvSeparation(double weight, double distance, double power, bool affectSelf)
        {
            SeparationWeight = weight;
            Distance = distance;
            Power = power;
            AffectSelf = affectSelf;
        }

        public override void Execute(AgentBase agent)
        {
            DFAgent dfAgent = agent as DFAgent;
            DFAgentSystem dfAgentSystem = dfAgent.AgentSystem as DFAgentSystem;

            List<DFAgent> neighbors = dfAgentSystem.FindNeighbors(dfAgent, Distance);

            Vector3d separation = Vector3d.Zero;

            if (neighbors.Count == 0) return;


            foreach (DFAgent neighbor in neighbors)
            {
                Vector3d moveAway = dfAgent.Position - neighbor.Position;
                double len = moveAway.Length;

                if (moveAway.IsZero) moveAway = Vector3d.XAxis;

                if (len < Distance)
                {
                    moveAway.Unitize();
                    Vector3d thisMove = moveAway * Math.Pow((Distance - len) / Distance, Power) * Distance;

                    if (AffectSelf)
                    {
                        separation += thisMove;
                    }
                    else // move neighbour
                    {
                        neighbor.Moves.Add(-thisMove);
                        neighbor.Weights.Add(SeparationWeight);
                    }
                }
            }

            dfAgent.Moves.Add(separation);
            dfAgent.Weights.Add(SeparationWeight);
        }
    }
}