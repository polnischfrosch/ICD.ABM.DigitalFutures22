using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ABxM.Core.Utilities;
using Grasshopper.Kernel;
using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using ICD.ABM.DigitalFutures22.Core.Behavior;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

namespace ICD.ABM.DigitalFutures22.Core.Behavior
{
    public class BhvMouseTracker : BehaviorBase
    {
        double MouseRange = 0.5;
        public double MouseWeight = 1.0;

        public BhvMouseTracker(double weight)
        {
            MouseWeight = weight;
        }

        public override void Execute(AgentBase agent)
        {
            DFAgentSystem system = (DFAgentSystem)agent.AgentSystem;

            ProcessMouseManipulation(system);
        }

        private void ProcessMouseManipulation(DFAgentSystem system)
        {
            // PROCESS MOUSE TRACKER
            if (MouseTracker.LeftMousePressed)
            {
                if (system.manipulatedAgent == null)
                {
                    double nearestDistance = MouseRange;
                    foreach (DFAgent v in system.Agents)
                    {
                        double distance = MouseTracker.MouseLine.DistanceTo(v.Position, false);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            system.manipulatedAgent = v;
                        }
                    }
                }

                //agentSystem.manipulatedAgent = agents[0];

                if (system.manipulatedAgent != null)
                {
                    Point3d p = system.manipulatedAgent.Position;
                    Point3d target = MouseTracker.MouseLine.ClosestPoint(p, false);
                    //if (!vData.IsMoveable) // If this is a fixed vertex, then we only allow it to be dragged on the XY plane
                    //{
                    //double t;
                    //Intersection.LinePlane(MouseTracker.MouseLine, Plane.WorldXY, out t);
                    //target = MouseTracker.MouseLine.From + MouseTracker.MouseLine.Direction * t;
                    //}

                    system.manipulatedAgent.Moves.Add(target - p);
                    //agentSystem.manipulatedAgent.Weights.Add(mouseWeight);
                    system.manipulatedAgent.Weights.Add(MouseWeight);
                }
            }
            else
            {
                system.manipulatedAgent = null;
                //Pick(system);
                //ClearPickedObjects();
            }
        }

        //private void Pick(DFAgentSystem system)
        //{
        //    if (MouseTracker.MouseLine.IsValid)
        //    {
        //        Triple rayStart = MouseTracker.MouseLine.From.ToTriple();
        //        Triple rayDir = MouseTracker.MouseLine.Direction.ToTriple().Normalise();
        //        hoveredVertex = system.Mesh.PickVertex(rayStart, rayDir);

        //    }
        //    else
        //    {
        //        hoveredVertex = null;
        //        hoveredHalfedge = null;
        //        hoveredFace = null;
        //    }

        //    if (MouseTracker.LeftMouseReleased)
        //    {
        //        if (hoveredVertex == null && hoveredHalfedge == null && hoveredFace == null)
        //        {
        //            pickedVertices.Clear();
        //            pickedHalfedges.Clear();
        //            pickedFaces.Clear();
        //        }
        //        else if (hoveredVertex != null)
        //        {
        //            if (pickedVertices.Contains(hoveredVertex)) pickedVertices.Remove(hoveredVertex);
        //            else pickedVertices.Add(hoveredVertex);
        //        }
        //        else if (hoveredHalfedge != null)
        //        {
        //            if (pickedHalfedges.Contains(hoveredHalfedge)) pickedHalfedges.Remove(hoveredHalfedge);
        //            else pickedHalfedges.Add(hoveredHalfedge);
        //        }
        //        else if (hoveredFace != null)
        //        {
        //            if (pickedFaces.Contains(hoveredFace)) pickedFaces.Remove(hoveredFace);
        //            else pickedFaces.Add(hoveredFace);
        //        }

        //        MouseTracker.LeftMouseReleased = false;
        //    };
        //}
    }
}
