using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.ABM.DigitalFutures22.Core.Environments;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using Rhino.Geometry;

namespace ICD.ABM.DigitalFutures22.Core.Behavior
{
    public class BhvMoveToCentroid : BehaviorBase
    {
        public int CentroidOption = 0;
        public double MoveToCentroidWeight;

        public BhvMoveToCentroid(double weight)
        {
            MoveToCentroidWeight = weight;
        }

        public override void Execute(AgentBase agent)
        {
            DFAgent dfAgent = agent as DFAgent;
            DFAgentSystem dfAgentSystem = dfAgent.AgentSystem as DFAgentSystem;
            SingleBrepEnvironment singleBrepEnvironment = dfAgentSystem.SingleBrepEnvironment;

            //======================================================================================================
            // Task: Compute the "move" vector caused by this behavior and add it to the list "cartesianAgent.Moves"
            //======================================================================================================

            Polyline cellPolyline = dfAgentSystem.VoronoiCells[dfAgent.Id].ToPolyline();

            Point3d cellCenter = Point3d.Origin;
            for (int i = 0; i < cellPolyline.Count - 1; i++)
                cellCenter += cellPolyline[i];

            cellCenter /= (cellPolyline.Count - 1);

            dfAgent.Moves.Add(cellCenter - dfAgent.Position);
            dfAgent.Weights.Add(MoveToCentroidWeight);
        }
    }
}