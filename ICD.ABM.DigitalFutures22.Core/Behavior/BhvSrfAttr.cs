using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICD.ABM.DigitalFutures22.Core.Behavior
{
    public class BhvSrfAttr : BehaviorBase
    {
        public Surface Surface;
        public double SurfaceAttractionWeight;


        public BhvSrfAttr(Surface srf, double weight)
        {
            Surface = srf;
            SurfaceAttractionWeight = weight;
        }
        public override void Execute(AgentBase agent)
        {
            DFAgent dfAgent = (DFAgent)agent;
            // TO DO: get surface from environment in system
            DFAgentSystem system = (DFAgentSystem)agent.AgentSystem;

            if (Surface == null) return;

            double u, v;
            Point3d p = dfAgent.Position;
            Surface.ClosestPoint(p, out u, out v);
            Vector3d move = Surface.PointAt(u, v) - p;
            dfAgent.Moves.Add(SurfaceAttractionWeight * move);
            dfAgent.Weights.Add(SurfaceAttractionWeight);

            //foreach (Vertex vertex in system.Mesh.Vertices.GetItems())
            //{
            //    double u, v;
            //    Point3d p = RhinoUtil.ToPoint3d(vertex.Position);
            //    Surface.ClosestPoint(p, out u, out v);
            //    Vector3d move = Surface.PointAt(u, v) - RhinoUtil.ToPoint3d(vertex.Position);
            //    FiftyMeshSystem.VertexData vertexData = system.VertexDataLut[vertex];
            //    vertexData.TotalWeightedMove += SurfaceAttractionWeight * move;
            //    vertexData.TotalWeight += SurfaceAttractionWeight;
            //}
        }
    }
}
