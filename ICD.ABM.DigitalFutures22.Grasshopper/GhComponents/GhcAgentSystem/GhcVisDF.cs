using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.ABM.DigitalFutures22.Core.Behavior;

using ICD.AbmFramework.Core.Environments;
using ICD.AbmFramework.Core.Utilities;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcAgentSystem
{
    public class GhcVisDF : GH_Component
    {
        public DFAgentSystem system = new DFAgentSystem();

        public GhcVisDF()
          : base("GhcVisDF", "VisDF",
              "Visualize DF Agent System",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DF AgentSystem", "AS", "DF AgentSystem", GH_ParamAccess.item);
            pManager.AddBooleanParameter("DisplayOnly", "D", "Only display, do not output geometry", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Agent Frames", "P", "The agent frames", GH_ParamAccess.list);
            pManager.AddCurveParameter("PlatePolylines", "PL", "The agent plate polylines", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "The agent mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iDisplayOnly = false;

            DA.GetData(0, ref system);
            DA.GetData(1, ref iDisplayOnly);

            List<Plane> agentFrames = new List<Plane>();
            List<Polyline> platePolylines = new List<Polyline>();
            Mesh mesh = null;

            if (!iDisplayOnly)
            {
                foreach (DFAgent agent in system.Agents)
                {
                    agentFrames.Add(agent.Frame);
                }

                platePolylines = system.DualPolylines;
                mesh = system.Mesh;
            }

            DA.SetDataList(0, agentFrames);
            DA.SetDataList(1, platePolylines);
            DA.SetData(2, mesh);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);
            system.DisplayMeshes(args.Display, system);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            system.DisplayWires(args.Display, system);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("0C7A54C6-7FDA-4DBB-B280-9781D60BEF81");
    }
}