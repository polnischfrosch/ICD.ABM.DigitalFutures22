using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
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
              "ABxM", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel AgentSystem", "AS", "Panel AgentSystem", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Output Geometry", "OG", "Output geometry, otherwise only display", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Cut Panels", "CP", "Cut the breps (slow)", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Agent Frames", "P", "The agent frames", GH_ParamAccess.list);
            pManager.AddCurveParameter("CellPolylines", "C", "The agent cell polylines", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "The agent mesh", GH_ParamAccess.item);
            pManager.AddLineParameter("Neighbors on Rail", "N", "The agent Neighbors on rail", GH_ParamAccess.list);
            pManager.AddCurveParameter("Panel Cuts", "PC", "The panel cuts on rail", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("Cut Planes", "CP", "The intersection planes with the rail", GH_ParamAccess.list);
            pManager.AddBrepParameter("Panel Brep", "PB", "The panel brep", GH_ParamAccess.list);
            pManager.AddNumberParameter("Panel Area", "PA", "The panel area", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iOutputGeometry = false;
            bool iCutBreps = false;

            DA.GetData(0, ref system);
            DA.GetData(1, ref iOutputGeometry);
            DA.GetData(2, ref iCutBreps);

            List<Point3d> agentPositions = new List<Point3d>();
            List<Polyline> cellPolylines = new List<Polyline>();
            Mesh mesh = null;
            GH_Structure<GH_Curve> directNeighborConnections = new GH_Structure<GH_Curve>();
            GH_Structure<GH_Curve> panelCuts = new GH_Structure<GH_Curve>();
            GH_Structure<GH_Plane> cutPlanes = new GH_Structure<GH_Plane>();
            List<Brep> panelBreps = new List<Brep>();
            List<double> panelArea = new List<double>();

            if (iOutputGeometry)
            {
                foreach (PanelAgent agent in system.Agents)
                {
                    agentPositions.Add(agent.Position);
                    cellPolylines.Add(agent.Cell);

                    foreach (Curve c in agent.PanelCuts)
                    {
                        GH_Curve _c = new GH_Curve(c);
                        panelCuts.Append(_c, new GH_Path(agent.Id));
                    }

                    foreach (LineCurve ln in agent.DirectNeighborConnections)
                    {
                        GH_Curve _ln = new GH_Curve(ln);
                        directNeighborConnections.Append(_ln, new GH_Path(agent.Id));
                    }

                    foreach (Plane pln in agent.CutPlanes)
                    {
                        GH_Plane _pln = new GH_Plane(pln);
                        cutPlanes.Append(_pln, new GH_Path(agent.Id));
                    }
                }
                mesh = system.SystemMesh;
            }
            if (iCutBreps) // slow, so only only cut the breps when true
            {
                foreach (PanelAgent agent in system.Agents)
                {
                    panelBreps.Add(agent.GetPanelSurface());
                    panelArea.Add(agent.PanelArea);
                }
            }

            DA.SetDataList(0, agentPositions);
            DA.SetDataList(1, cellPolylines);
            DA.SetData(2, mesh);
            DA.SetDataTree(3, directNeighborConnections);
            DA.SetDataTree(4, panelCuts);
            DA.SetDataTree(5, cutPlanes);
            DA.SetDataList(6, panelBreps);
            DA.SetDataList(7, panelArea);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            system.DisplayWires(args.Display, system);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("8477BA11-7AB3-4BE6-881A-287C2191EF3C");
    }
}