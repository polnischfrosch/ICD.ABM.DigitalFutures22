using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcBehavior
{
    public class GhcBhvMoveToCentroid : GH_Component
    {
        BhvMoveToCentroid behavior = new BhvMoveToCentroid(1.0);

        public GhcBhvMoveToCentroid()
          : base("GhcBhvMoveToCentroid", "BhvMoveToCentroid",
              "BhvMoveToCentroid",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "W", "SurfaceAttraction Behavior Weight", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Move to Centroid Behavior", "B", "Move to Centroid Behavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iWeight = double.NaN;
            DA.GetData(0, ref iWeight);

            behavior.MoveToCentroidWeight = iWeight;

            DA.SetData(0, behavior);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("3D224543-A7BE-40CB-BF6E-EDD2C57811CC");

    }
}