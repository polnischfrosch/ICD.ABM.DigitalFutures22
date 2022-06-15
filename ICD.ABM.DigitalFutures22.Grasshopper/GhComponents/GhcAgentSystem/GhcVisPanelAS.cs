using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

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

            if (!iDisplayOnly)
            {
                foreach (PanelAgent agent in system.Agents)
                {
                    //agentFrames.Add(agent.Frame);
                    agentPositions.Add(agent.Position);
                    cellPolylines.Add(agent.Cell);

                    foreach (PanelAgent otherAgent in agent.NeighborsOnRail)
                    {
                        neighborsOnRail.Add(new LineCurve(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(agent.UV.X, agent.UV.Y),
                                                          system.RailEnvironment.BrepObject.Surfaces[0].PointAt(otherAgent.UV.X, otherAgent.UV.Y)));
                    }
                }

                mesh = system.SystemMesh;
            }

            //DA.SetDataList(0, agentFrames);
            DA.SetDataList(0, agentPositions);
            DA.SetDataList(1, cellPolylines);
            DA.SetData(2, mesh);
            DA.SetDataList(3, neighborsOnRail);
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