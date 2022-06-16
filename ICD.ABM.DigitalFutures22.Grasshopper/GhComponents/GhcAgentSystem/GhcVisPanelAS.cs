using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcAgentSystem
{
    public class GhcVisPanelAS : GH_Component
    {
        public PanelAgentSystem system = new PanelAgentSystem();

        public GhcVisPanelAS()
          : base("GhcVisPanelAS", "VisPanelAS",
              "Visualize Panel Agent System",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel AgentSystem", "AS", "Panel AgentSystem", GH_ParamAccess.item);
            pManager.AddBooleanParameter("DisplayOnly", "D", "Only display, do not output geometry", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Agent Frames", "P", "The agent frames", GH_ParamAccess.list);
            pManager.AddCurveParameter("CellPolylines", "C", "The agent cell polylines", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "The agent mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Neighbors on Rail", "N", "The agent Neighbors on rail", GH_ParamAccess.list);
            pManager.AddLineParameter("Panel Cuts", "PC", "The panel cuts on rail", GH_ParamAccess.list);
            pManager.AddPointParameter("Intersections", "ITX", "The intersections with rail", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iDisplayOnly = false;

            DA.GetData(0, ref system);
            DA.GetData(1, ref iDisplayOnly);

            //List<Plane> agentFrames = new List<Plane>();
            List<Point3d> agentPositions = new List<Point3d>();
            List<Polyline> cellPolylines = new List<Polyline>();
            Mesh mesh = null;
            List<LineCurve> neighborsOnRail = new List<LineCurve>();
            List<Line> panelCuts = new List<Line>();
            List<Point3d> itx = new List<Point3d>();

            if (!iDisplayOnly)
            {
                foreach (PanelAgent agent in system.Agents)
                {
                    //agentFrames.Add(agent.Frame);
                    agentPositions.Add(agent.Position);
                    cellPolylines.Add(agent.Cell);

                    List<double> distances = new List<double>();

                    // TO DO: getting only closest results in not always picking all connections, find other solution
                    foreach (PanelAgent otherAgent in agent.NeighborsOnRail)
                    {
                        distances.Add(agent.Position.DistanceTo(otherAgent.Position));
                    }

                    double minVal = distances.Min();
                    int index = distances.IndexOf(minVal);

                    LineCurve ln = new LineCurve(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                                                              agent.UV.X, agent.UV.Y),
                                                 system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                                                              agent.NeighborsOnRail[index].UV.X,
                                                              agent.NeighborsOnRail[index].UV.Y));

                    neighborsOnRail.Add(ln);

                    Point3d mid = ln.PointAtNormalizedLength(0.5);

                    Vector3d vec = ln.PointAtStart - ln.PointAtEnd;

                    Vector3d dir = Vector3d.CrossProduct(vec, system.RailEnvironment.BrepObject.Surfaces[0].NormalAt(
                        system.RailEnvironment.UVCoordinates(mid).X,
                        system.RailEnvironment.UVCoordinates(mid).Y));

                    //Vector3d dir = Vector3d.CrossProduct(vec, Vector3d.ZAxis);

                    Line l = new Line(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                                                              system.RailEnvironment.UVCoordinates(mid).X,
                                                              system.RailEnvironment.UVCoordinates(mid).Y), dir, 200);
                    Line l2 = new Line(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                                                              system.RailEnvironment.UVCoordinates(mid).X,
                                                              system.RailEnvironment.UVCoordinates(mid).Y), -dir, 200);

                    //Line l2D = new Line(system.RailEnvironment.UVCoordinates(mid), dir, 100000);

                    // find intersections with boundary and rails
                    // CurveLine too heavy, too many CPs? crashes - try LineLine for now

                    //foreach (Curve rail in system.RailEnvironment.Rails)
                    //{
                    //    Curve crv = system.RailEnvironment.BrepObject.Surfaces[0].Pullback(rail, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                    //    var events = Rhino.Geometry.Intersect.Intersection.CurveLine(crv, l2D, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                    //    foreach (var e in events)
                    //    {
                    //        itx.Add(e.PointA);
                    //    }
                    //}

                    panelCuts.Add(l);
                    panelCuts.Add(l2);
                }

                mesh = system.SystemMesh;
            }

            //DA.SetDataList(0, agentFrames);
            DA.SetDataList(0, agentPositions);
            DA.SetDataList(1, cellPolylines);
            DA.SetData(2, mesh);
            DA.SetDataList(3, neighborsOnRail);
            DA.SetDataList(4, panelCuts);
            DA.SetDataList(5, itx);
        }

        //public override void DrawViewportMeshes(IGH_PreviewArgs args)
        //{
        //    base.DrawViewportMeshes(args);
        //    system.DisplayMeshes(args.Display, system);
        //}

        //public override void DrawViewportWires(IGH_PreviewArgs args)
        //{
        //    base.DrawViewportWires(args);
        //    system.DisplayWires(args.Display, system);
        //}

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("8477BA11-7AB3-4BE6-881A-287C2191EF3C");
    }
}