using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcAgent
{
    public class GhcDefinePanelAgent : GH_Component
    {
        List<PanelAgent> agents = new List<PanelAgent>();
        List<BehaviorBase> behaviors = new List<BehaviorBase>();

        public GhcDefinePanelAgent()
          : base("GhcDefinePanelAgent", "PanelAgent",
              "PanelAgent",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("UV Position", "UV", "The initial UV position of the panel agent.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Behaviors", "B", "Behaviors of the panel agent", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PanelAgent", "A", "The panel agent", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> iPositions = new List<Point3d>();
            //List<Point2d> iUVs = new List<Point2d>();
            DA.GetDataList(0, iPositions);

            //foreach (Point3d p in iUVs3d)
            //    iUVs.Add(new Point2d(p.X, p.Y));


            List<BehaviorBase> iBehaviors = new List<BehaviorBase>();
            DA.GetDataList(1, iBehaviors);

            // check if positions changed
            bool positionsChanged = false;
            if (iPositions.Count != agents.Count)
            {
                positionsChanged = true;
            }
            else
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    if (agents[i].Position != iPositions[i])
                    {
                        positionsChanged = true;
                        break;
                    }
                }
            }

            // check if behaviours changed
            bool behaviorsChanged = false;
            if (iBehaviors.Count != behaviors.Count)
            {
                behaviorsChanged = true;
            }
            else
            {
                for (int i = 0; i < iBehaviors.Count; i++)
                {
                    if (iBehaviors[i] != behaviors[i])
                    {
                        behaviorsChanged = true;
                    }
                }
            }
            behaviors = iBehaviors;

            // create agents
            if (positionsChanged)
            {
                agents = new List<PanelAgent>();
                for (int i = 0; i < iPositions.Count; i++)
                {
                    //Point2d uvParameter = iUVs[i];

                    agents.Add(new PanelAgent(iPositions[i], behaviors));
                }
            }

            // update behaviours
            if (!positionsChanged && behaviorsChanged)
            {
                foreach (PanelAgent pa in agents)
                {
                    pa.Behaviors = iBehaviors;
                }
            }
            DA.SetDataList(0, agents);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("2120E0A5-5082-4255-A6B5-834E9B8E5C97");
    }
}