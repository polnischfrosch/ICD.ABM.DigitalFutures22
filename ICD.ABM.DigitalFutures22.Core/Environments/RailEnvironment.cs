using ICD.AbmFramework.Core.Environments;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICD.ABM.DigitalFutures22.Core.Environments
{
    public class RailEnvironment : SingleBrepEnvironment
    {
        public List<Curve> RailCurves;
        public List<Brep> Rails;

        public RailEnvironment(Brep brep, List<Curve> railCurves, List<Brep> rails)
        {
            BrepObject = brep;
            RailCurves = railCurves;
            Rails = rails;
        }

        public RailEnvironment(Brep brep, List<Brep> rails, double spacing)
        {
            BrepObject = brep;
            Rails = rails;
            ComputeCurvatureField(spacing);
        }
    }
}
