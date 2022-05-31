using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcBehavior
{
    public class GhcBhvSrfAttr : GH_Component
    {
        BhvSrfAttr behavior = new BhvSrfAttr(null, 1.0);

        public GhcBhvSrfAttr()
          : base("GhcBhvSrfAttr", "BhvSrfAttr",
              "BhvSrfAttr",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Srf", "Attraction Surface", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "W", "SurfaceAttraction Behavior Weight", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Surface Attraction Behavior", "B", "Surface Attraction Behavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface iSrf = null;
            DA.GetData(0, ref iSrf);

            double iWeight = double.NaN;
            DA.GetData(1, ref iWeight);

            behavior.Surface = iSrf;
            behavior.SurfaceAttractionWeight = iWeight;

            DA.SetData(0, behavior);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("C40D5A57-ED86-468C-9DC8-5D88C241F2FF");
    }
}