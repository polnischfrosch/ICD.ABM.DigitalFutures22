using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICD.ABM.DigitalFutures22.Core.Agent
{
    public class DFAgent : CartesianAgent
    {
        public Vector3d Normal;
        public Plane Frame;

        public Polyline PlatePolyline;
        public Curve PlateCurve;

        public DFAgent(Point3d startPosition, List<BehaviorBase> behaviors) : base(startPosition, behaviors)
        {
            this.StartPosition = this.Position = startPosition;
            this.Behaviors = behaviors;
        }

        /// <summary>
        /// Method for resetting the agent.
        /// </summary>
        public override void Reset()
        {
            this.Position = this.StartPosition;
            Moves.Clear();
            Weights.Clear();
        }

        /// <summary>
        /// Method for running code that should be pre-executed.
        /// </summary>
        public override void PreExecute()
        {
            Moves.Clear();
            Weights.Clear();

            DFAgentSystem thisAgentSystem = this.AgentSystem as DFAgentSystem;

            // get Normal
            Vector3d normalFromEnvironment = thisAgentSystem.SingleBrepEnvironment.GetNormal(Position);
            if (normalFromEnvironment != Vector3d.Unset) Normal = normalFromEnvironment;

            //get Frame

            this.Frame = new Plane(this.Position, this.Normal);
        }

        /// <summary>
        /// Method for updating the agent's state.
        /// </summary>
        public override void Execute()
        {
            foreach (BehaviorBase behavior in this.Behaviors)
                behavior.Execute(this);
        }

        /// <summary>
        /// Method for running code that should be post-executed.
        /// </summary>
        public override void PostExecute()
        {
            if (Moves.Count == 0) return;

            Vector3d totalWeightedMove = Vector3d.Zero;
            double totalWeight = 0.0;

            for (int i = 0; i < Moves.Count; ++i)
            {
                totalWeightedMove += Weights[i] * Moves[i];
                totalWeight += Weights[i];
            }

            if (totalWeight > 0.0)
                Position += totalWeightedMove / totalWeight;
        }

        /// <summary>
        /// Method for collecting the geometry that should be displayed.
        /// </summary>
        /// <returns>Returns a list containing each agent's position.</returns>
        public override List<object> GetDisplayGeometries()
        {
            //return new List<object> { Frame, PlatePolyline };
            return new List<object> { };
        }

        /// <summary>
        /// The ID of the agent, unique within the given system
        /// </summary>
        public new int Id { get; internal set; }
    }
}
