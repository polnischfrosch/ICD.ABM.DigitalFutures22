using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Behavior;

using ICD.ABM.DigitalFutures22.Core.AgentSystem;

namespace ICD.ABM.DigitalFutures22.Core.Agent
{
    public class RailsAgent : AgentBase
    {
        public double LocationParameter = -1.0;
        public double StartLocation = -1.0;
        
        // Maybe I don't need either a normal (because it is the nromal at my UV)
        //public Vector3d Normal;
        // Maybe I don't need a Frame either, also because it is from UV
        //public Plane Frame;

        public Polyline PlatePolyline;
        // Curve Cannot be set to "null". are there properties of Curve I need taht Polyline does not have?
        //public Curve PlateCurve;

        /// <summary>
        /// The list of 2-dimensional moves
        /// </summary>
        public List<Vector2d> Moves = new List<Vector2d>();
        public List<double> Weights = new List<double>();


        public RailsAgent(double startPosition, List<BehaviorBase> behaviors) : base(startPosition, behaviors)
        {
            this.StartLocation = this.LocationParameter = startPosition;
            this.Behaviors = behaviors;
        }

        /// <summary>
        /// Method for resetting the agent.
        /// </summary>
        public override void Reset()
        {
            this.LocationParameter = this.StartLocation;
            Moves.Clear();
            Weights.Clear();
            PlatePolyline.Clear();
        }

        /// <summary>
        /// Method for running code that should be pre-executed.
        /// </summary>
        public override void PreExecute()
        {
            Moves.Clear();
            Weights.Clear();

            RailsAgentSystem thisAgentSystem = this.AgentSystem as RailsAgentSystem;

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
