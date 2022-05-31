using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcBehavior
{
    public class GhcBhvSeparation : GH_Component
    {
        BhvSeparation behavior = new BhvSeparation(1.0, 2000.0, 1000.0, true);

        public GhcBhvSeparation()
          : base("GhcSeparation", "GhcSeparation",
              "GhcSeparation",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Distance", "D", "Neighbor Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("Power", "P", "Power", GH_ParamAccess.item);
            pManager.AddBooleanParameter("AffectSelf", "A", "Affect Self", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "W", "SurfaceAttraction Behavior Weight", GH_ParamAccess.item);
        }



        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Separation Behavior", "B", "Separation Behavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iDist = double.NaN;
            DA.GetData(0, ref iDist);
            double iPower = double.NaN;
            DA.GetData(1, ref iPower);

            bool iAffectSelf = true;
            DA.GetData(2, ref iAffectSelf);

            double iWeight = double.NaN;
            DA.GetData(3, ref iWeight);

            behavior.SeparationWeight = iWeight;
            behavior.Distance = iDist;
            behavior.Power = iPower;
            behavior.AffectSelf = iAffectSelf;

            DA.SetData(0, behavior);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("514BBC46-8979-4AD2-AF6B-9220A6297009");
    }
}