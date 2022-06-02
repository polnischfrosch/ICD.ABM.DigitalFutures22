using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Grasshopper.GhComponents.GhcAgentSystem
{
    public class GhcDefineDFAgentSystem : GH_Component
    {
        DFAgentSystem agentSystem = null;
        List<DFAgent> agents = new List<DFAgent>();

        public GhcDefineDFAgentSystem()
          : base("GhcDefineDFAgentSystem", "DFAgentSystem",
              "DefineDFAgentSystem",
              "ICD", "DigitalFutures2022")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Single Brep Environment", "E", "Environment", GH_ParamAccess.item);
            //pManager[0].Optional = true;
            pManager.AddGenericParameter("Cartesian Agents", "A", "Cartesian Agents", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Compute Diagram", "D", "0 = None, 1 = Delaunay, 2 = Voronoi", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cartesian Agent System", "AS", "The cartesian agent system", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SingleBrepEnvironment iEnvironment = null;
            DA.GetData(0, ref iEnvironment);

            List<DFAgent> iAgents = new List<DFAgent>();
            DA.GetDataList(1, iAgents);

            int iDiagram = 0;
            DA.GetData(2, ref iDiagram);

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
            // otherwise, update agent system's environment, plate generator and threshold
            if (agentsChanged)
            {
                agentSystem = new DFAgentSystem(agents); //, iEnvironment);
            }

            agentSystem.SingleBrepEnvironment = iEnvironment;

            if (iDiagram == 0 || iDiagram > 2)
            {
                agentSystem.ComputeDelaunayConnectivity = false;
                agentSystem.ComputeVoronoiCells = false;
            }
            else if (iDiagram == 1)
            {
                agentSystem.ComputeDelaunayConnectivity = true;
                agentSystem.ComputeVoronoiCells = false;
            }
            else if (iDiagram == 2)
            {
                agentSystem.ComputeDelaunayConnectivity = false;
                agentSystem.ComputeVoronoiCells = true;
            }

            DA.SetData(0, agentSystem);
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        public override Guid ComponentGuid => new Guid("2E80A5A3-2314-428E-A054-86787421121C");
    }
}