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
        public List<Curve> Rails;

        public RailEnvironment(Brep brep, List<Curve> rails)
        {
            BrepObject = brep;
            Rails = rails;
        }

        public RailEnvironment(Brep brep, List<Curve> rails, double spacing)
        {
            BrepObject = brep;
            Rails = rails;
            ComputeCurvatureField(spacing);
        }
    }
}
