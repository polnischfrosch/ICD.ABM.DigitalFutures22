
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Display;
using Rhino.Geometry;
using System.Drawing;

namespace ICD.ABM.DigitalFutures22.Core.AgentSystem
{
    public partial class DFAgentSystem : AgentSystemBase
    {
        public DisplayPipeline Dpl;

        public void DisplayMeshes(DisplayPipeline dpl, DFAgentSystem system)
        {
            Dpl = dpl;
        }

        public void DisplayWires(DisplayPipeline dpl, DFAgentSystem system)
        {
            Dpl = dpl;

            foreach (Polyline p in system.DualPolylines)
            {
                Dpl.DrawPolyline(p, Color.Crimson);
            }
        }
    }
}
