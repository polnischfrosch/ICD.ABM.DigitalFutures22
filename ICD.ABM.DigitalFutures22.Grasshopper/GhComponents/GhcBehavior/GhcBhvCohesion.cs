using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcBehavior
{
    public class GhcBhvCohesion : GH_Component
    {
        BhvCohesion behavior = new BhvCohesion(1.0, 10.0);

        public GhcBhvCohesion()
          : base("GhcBhvCohesion", "GhcBhvCohesion",
              "GhcBhvCohesion",
              "ICD", "DigitalFutures2022")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Distance", "D", "Neighbor Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "W", "SurfaceAttraction Behavior Weight", GH_ParamAccess.item);
        }



        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cohesion Behavior", "B", "Cohesion Behavior", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iDist = double.NaN;
            DA.GetData(0, ref iDist);

            double iWeight = double.NaN;
            DA.GetData(1, ref iWeight);

            behavior.CohesionWeight = iWeight;
            behavior.Distance = iDist;

            DA.SetData(0, behavior);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("E16C8933-8C85-47A6-B1F3-92394427B538");
    }
}