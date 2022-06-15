using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// <Custom "using" statements>

using ICD.AbmFramework.Core;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Behavior;
using ICD.AbmFramework.Core.Environments;

using ICD.ABM.DigitalFutures22.Core.Agent;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;

// </Custom "using" statements>


#region padding (this ensures the line number of this file match with those in the code editor of the C# Script component












#endregion

public partial class MyExternalScript : GH_ScriptInstance
{
  #region Do_not_modify_this_region
  private void Print(string text) { }
  private void Print(string format, params object[] args) { }
  private void Reflect(object obj) { }
  private void Reflect(object obj, string methodName) { }
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA) { }
  public RhinoDoc RhinoDocument;
  public GH_Document GrasshopperDocument;
  public IGH_Component Component;
  public int Iteration;
  #endregion


  private void RunScript(object x, object y, ref object A)
  {
    // <Custom code>

    behavior.Stepsize = iStep;
    behavior.Flip = iFlip;
    oBehavior = behavior;

    // </Custom code>
  }

  // <Custom additional code>

  DescentUVBehavior behavior = new DescentUVBehavior(1.0);

  public class DescentUVBehavior : BehaviorBase
  {
    public double Stepsize;
    public bool Flip;
    //public Vector3d derivatives[1];

    public DescentUVBehavior(double _stepsize)
    {
      Stepsize = _stepsize;
      Weight = 1.0;
    }

    public override void Execute(AgentBase agent)
    {
      UmbilicalAgent myAgent = (UmbilicalAgent) agent;
      UmbilicalAgentSystem mySystem = (UmbilicalAgentSystem) myAgent.AgentSystem;

      // get the curvature at the current location
      SurfaceCurvature curvature = mySystem.SystemSurface.CurvatureAt(myAgent.UV.X, myAgent.UV.Y);
      double uCurvature = curvature.Kappa(0);
      double vCurvature = curvature.Kappa(1);
      double delta = Math.Abs(uCurvature - vCurvature);
      double tol = 0.00001;
      if(delta < tol)
      {
        //Rhino.RhinoApp.WriteLine("Agent " + myAgent.Id + ": I've arrived at an umbilical point!");
        myAgent.isFinished = true;
        return;
      }

      // let's assume we're at a point where principal curvatures are different
      // question: which way do we have to go to reduce the difference between principal curvatures?
      // we know the principal curvature directions and the curvature amounts
      // approach: let's take a small step in the four directions of principal curvature and see if the delta
      // is reduced. Then go in the direction of the largest decrease

      Vector3d uDirection = curvature.Direction(0);
      uDirection.Unitize();
      Vector3d vDirection = curvature.Direction(1);
      vDirection.Unitize();

      double Factor = Stepsize * delta * 1000;

      Vector3d moveVectorUup = Factor * uDirection;
      Vector3d moveVectorUdown = moveVectorUup * -1;
      Vector3d moveVectorVup = Factor * vDirection;
      Vector3d moveVectorVdown = moveVectorVup * -1;

      List<Vector3d> testVectors = new List<Vector3d>{moveVectorUup, moveVectorUdown, moveVectorVup, moveVectorVdown};
      List<SurfaceCurvature> testCurvatures = new List<SurfaceCurvature>();
      List<double> testDeltas = new List<double>();

      foreach(Vector3d testVector in testVectors)
      {
        Point3d testPoint = curvature.Point + testVector;
        double uTest, vTest = 0.0;
        mySystem.SystemSurface.ClosestPoint(testPoint, out uTest, out vTest);
        SurfaceCurvature testCurvature = mySystem.SystemSurface.CurvatureAt(uTest, vTest);
        testCurvatures.Add(testCurvature);
        double uKappa = testCurvature.Kappa(0);
        double vKappa = testCurvature.Kappa(1);
        double testDelta = Math.Abs(uKappa - vKappa);
        //double testDelta = Math.Abs(Math.Abs(uKappa) - Math.Abs(vKappa));
        testDeltas.Add(testDelta);
      }

      //now find the min testDelta
      Double minDelta = testDeltas[0];
      int minIndex = 0;
      for(int i = 1; i < testDeltas.Count; i++)
      {
        if(testDeltas[i] < minDelta)
        {
          minDelta = testDeltas[i];
          minIndex = i;
        }
      }

      if(minDelta < delta)
      {
        //now choose the direction and curvature that will result in the largest decrease
        Vector3d targetDirection = testVectors[minIndex];
        SurfaceCurvature targetCurvature = testCurvatures[minIndex];

        //get the UV parameters at the target location
        Point2d targetUVPoint = targetCurvature.UVPoint;

        Vector2d move2D = targetUVPoint - myAgent.UV;
        move2D.Unitize();
        move2D *= Factor;

        myAgent.Moves.Add(move2D);
        myAgent.Weights.Add(Weight);
      }
    }
  }
  
  // </Custom additional code>
}
