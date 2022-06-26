using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcAgent
{
    public class GhcDefineDFAgent : GH_Component
    {
        List<DFAgent> agents = new List<DFAgent>();
        List<BehaviorBase> behaviors = new List<BehaviorBase>();

        public GhcDefineDFAgent()
          : base("GhcDefineDFAgent", "DFAgent",
              "DFAgent",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "P", "The initial position of the cartesian agent.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Behaviors", "B", "Behaviors of the cartesian agent", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DFAgent", "A", "The DF agent", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> iPositions = new List<Point3d>();
            DA.GetDataList(0, iPositions);

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
                agents = new List<DFAgent>();
                for (int i = 0; i < iPositions.Count; i++)
                {
                    Point3d thisPosition = iPositions[i];

                    agents.Add(new DFAgent(thisPosition, iBehaviors));
                }
            }

            // update behaviours
            if (!positionsChanged && behaviorsChanged)
            {
                foreach (DFAgent pa in agents)
                {
                    pa.Behaviors = iBehaviors;
                }
            }

            DA.SetDataList(0, agents);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("C55FE394-3CEA-4DB7-B164-B7A43648E517");
    }
}