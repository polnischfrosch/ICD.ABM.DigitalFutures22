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
            pManager.AddLineParameter("Neighbors on Rail", "N", "The agent Neighbors on rail", GH_ParamAccess.list);
            pManager.AddCurveParameter("Panel Cuts", "PC", "The panel cuts on rail", GH_ParamAccess.list);
            pManager.AddPlaneParameter("cutplanes", "cp", "The intersections with rail", GH_ParamAccess.list);
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
            List<Curve> panelCuts = new List<Curve>();
            List<Plane> cutPlanes = new List<Plane>();

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

                    LineCurve ln2D = new LineCurve(new Point3d(
                                                             agent.UV.X, agent.UV.Y, 0.0),
                                                new Point3d(
                                                             agent.NeighborsOnRail[index].UV.X,
                                                             agent.NeighborsOnRail[index].UV.Y, 0.0));

                    neighborsOnRail.Add(ln);

                    Point3d mid = ln.PointAtNormalizedLength(0.5);

                    Point3d mid2D = ln2D.PointAtNormalizedLength(0.5);

                    Vector3d vec = ln.PointAtStart - ln.PointAtEnd;

                    Vector3d dir = Vector3d.CrossProduct(vec, system.RailEnvironment.BrepObject.Surfaces[0].NormalAt(
                        system.RailEnvironment.UVCoordinates(mid).X,
                        system.RailEnvironment.UVCoordinates(mid).Y));

                    //Vector3d dir = Vector3d.CrossProduct(vec, Vector3d.ZAxis);

                    //Line l = new Line(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                    //                                          system.RailEnvironment.UVCoordinates(mid).X,
                    //                                          system.RailEnvironment.UVCoordinates(mid).Y), dir, 200);
                    //Line l2 = new Line(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                    //                                          system.RailEnvironment.UVCoordinates(mid).X,
                    //                                          system.RailEnvironment.UVCoordinates(mid).Y), -dir, 200);

                    //Line l2D = new Line(system.RailEnvironment.UVCoordinates(mid), dir, 100000);

                    //plane intersection
                    Curve[] itxCrv = null;
                    Point3d[] itxPts = null;

                    Point3d plnPos = system.RailEnvironment.BrepObject.Surfaces[0].PointAt(mid2D.X, mid2D.Y);

                    Plane cutPlane = new Plane(plnPos, dir, system.RailEnvironment.BrepObject.Surfaces[0].NormalAt(
                      system.RailEnvironment.UVCoordinates(mid).X,
                      system.RailEnvironment.UVCoordinates(mid).Y));

                    bool intersect = Rhino.Geometry.Intersect.Intersection.BrepPlane(agent.Rail, cutPlane, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out itxCrv, out itxPts);

                    cutPlanes.Add(cutPlane);

                    if (intersect)
                    {
                        if (itxCrv.Count() > 1)
                        {
                            foreach (Curve c in itxCrv)
                            {
                                double t;
                                bool closest = c.ClosestPoint(plnPos, out t, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                                if (closest)
                                {
                                    panelCuts.Add(c);
                                }
                            }
                        }
                        else
                        {
                            panelCuts.Add(itxCrv[0]);
                        }
                    }

                }
                mesh = system.SystemMesh;
            }

            //DA.SetDataList(0, agentFrames);
            DA.SetDataList(0, agentPositions);
            DA.SetDataList(1, cellPolylines);
            DA.SetData(2, mesh);
            DA.SetDataList(3, neighborsOnRail);
            DA.SetDataList(4, panelCuts);
            DA.SetDataList(5, cutPlanes);
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