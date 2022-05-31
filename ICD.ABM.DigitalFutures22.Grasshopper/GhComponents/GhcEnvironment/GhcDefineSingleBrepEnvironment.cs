using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using ICD.AbmFramework.Core.Environments;
using ICD.AbmFramework.Core.Utilities;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineSingleBrepEnvironment : GH_Component
    {
        SingleBrepEnvironment brepEnv = null;

        public GhcDefineSingleBrepEnvironment()
          : base("GhcDefineSingleBrepEnvironment", "GhcDefineSingleBrepEnvironment",
              "GhcDefineSingleBrepEnvironment",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Single Brep", "S", "Single Brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Spacing", "S", "The double defining the curvature field's spacing", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Single Brep Environment", "E", "The Single Brep Environment", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep iBrep = null;
            DA.GetData(0, ref iBrep);

            double iSpacing = double.NaN;
            DA.GetData(1, ref iSpacing);

            brepEnv = new SingleBrepEnvironment(iBrep, iSpacing);

            DA.SetData(0, brepEnv);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("27192D28-318C-4D9D-BB19-C1E743183EED");
    }
}