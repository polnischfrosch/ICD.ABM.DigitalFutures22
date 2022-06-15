using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Environments;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcEnvironment
{
    public class GhcDefineRailEnvironment : GH_Component
    {
        RailEnvironment railEnv = null;

        public GhcDefineRailEnvironment()
          : base("GhcDefineRailEnvironment", "GhcDefineRailEnvironment",
              "GhcDefineRailEnvironment",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Single Brep", "S", "Single Brep", GH_ParamAccess.item);
            pManager.AddCurveParameter("Rails", "R", "Rail Curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("Spacing", "S", "The double defining the curvature field's spacing", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Single Brep Environment", "E", "The Single Brep Environment", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep iBrep = null;
            DA.GetData(0, ref iBrep);

            List<Curve> iRails = new List<Curve>();
            DA.GetDataList(1, iRails);

            double iSpacing = double.NaN;
            DA.GetData(2, ref iSpacing);

            railEnv = new RailEnvironment(iBrep, iRails);

            DA.SetData(0, railEnv);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("89784619-BEA3-487D-95FA-820D483039E1");
    }
}