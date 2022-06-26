using ABxM.Core.AgentSystem;
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.Environments;
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

            foreach (DFAgent a in system.Agents)
            {
                Dpl.DrawLine(a.Position, a.Position + 200 * a.Frame.XAxis, Color.SteelBlue, 3);

                Dpl.DrawPoint(
                    a.Position,
                    PointStyle.RoundSimple,
                    Color.Crimson,
                    Color.Crimson,
                    5f, 1f, 0f, 0f, true, true);
            }

            //foreach (Polyline p in system.DualPolylines)
            //for (int j = 0; j < 5; j++)
            //    //{
            //    //Dpl.DrawPolyline(p, Color.DarkSeaGreen);

            //    //double u1;
            //    //double v1;
            //    //double u2;
            //    //double v2;

            //    for (int i = 0; i < system.DualPolylines[j].Count - 1; i++)
            //    {

            //        Point3d a = system.SingleBrepEnvironment.UVCoordinates(system.DualPolylines[j][i]);
            //        Point3d b = system.SingleBrepEnvironment.UVCoordinates(system.DualPolylines[j][i + 1]);

            //        Point2d start = new Point2d(a.X, a.Y);
            //        Point2d end = new Point2d(b.X, b.Y);

            //        Curve sPath = system.SingleBrepEnvironment.BrepObject.Surfaces[0].ShortPath(start, end, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //        Dpl.DrawCurve(sPath, Color.DarkSeaGreen);

            if (system.DualPolylines != null)
            {
                foreach (Polyline pl in system.DualPolylines)
                {
                    for (int i = 0; i < pl.Count - 1; i++)
                    {
                        Dpl.DrawLine(pl[i], pl[i + 1], Color.DarkSeaGreen);
                    }
                }
            }
        }
    }
}
