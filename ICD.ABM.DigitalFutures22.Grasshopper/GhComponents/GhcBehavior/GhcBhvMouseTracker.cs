using Grasshopper.Kernel;
using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using ICD.AbmFramework.Core.Utilities;
using Rhino.Geometry;
using Rhino.Geometry;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Generic;


namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcBehavior
{
    public class GhcBhvMouseTracker : GH_Component
    {
        BhvMouseTracker behavior = new BhvMouseTracker(1.0);

        public GhcBhvMouseTracker()
          : base("GhcBhvMouseTracker", "GhcBhvMouseTracker",
              "GhcBhvMouseTracker",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "W", "MouseTracker Behavior Weight", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MouseTracker Behavior", "B", "MouseTracker Behavior", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iWeight = double.NaN;
            DA.GetData(0, ref iWeight);

            double mouseWeight = iWeight;
            behavior.MouseWeight = mouseWeight;

            DA.SetData(0, behavior);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("4599A8B3-A451-4F33-9B60-D6753873231F");
    }
}