using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.ABM.DigitalFutures22.Core.Environments;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcAgentSystem
{
    public class GhcDefinePanelAgentSystem : GH_Component
    {
        PanelAgentSystem agentSystem = null;
        List<PanelAgent> agents = new List<PanelAgent>();

        public GhcDefinePanelAgentSystem()
          : base("GhcDefinePanelAgentSystem", "PanelAgentSystem",
              "DefinePanelAgentSystem",
              "ABxM", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rail Environment", "E", "Rail Environment", GH_ParamAccess.item);
            pManager.AddGenericParameter("Panel Agents", "A", "Panel Agents", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel Agent System", "AS", "The panel agent system", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RailEnvironment iEnvironment = null;
            DA.GetData(0, ref iEnvironment);

            List<PanelAgent> iAgents = new List<PanelAgent>();
            DA.GetDataList(1, iAgents);


            // check if agents changed
            bool agentsChanged = false;
            if (agents.Count != iAgents.Count)
            {
                agentsChanged = true;
            }
            else
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    if (agents[i] != iAgents[i])
                    {
                        agentsChanged = true;
                    }
                }
            }
            agents = iAgents;

            // if agents changed, create a new agent system
            // otherwise, update agent system's environment
            if (agentsChanged)
            {
                agentSystem = new PanelAgentSystem(agents, iEnvironment); //, iEnvironment);
            }

            agentSystem.RailEnvironment = iEnvironment;

            DA.SetData(0, agentSystem);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("C535F550-7814-44B3-9E9B-610EEDC52588");
    }
}