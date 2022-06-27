using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;

using ICD.ABM.DigitalFutures22.Core.AgentSystem;

namespace ICD.ABM.DigitalFutures22.Core.Agent
{
    public class RailsAgent : AgentBase
    {
        public double T = -1.0;
        private double startT;

        public Curve SystemCurve;
        public Polyline flowCurve = new Polyline();

        /// <summary>
        /// The list of 2-dimensional moves
        /// </summary>
        public List<double> Moves = new List<double>();
        public List<double> Weights = new List<double>();


        public RailsAgent(double parameter, Curve systemCurve, List<BehaviorBase> behaviors)
        {
            this.startT = this.T = parameter;
            this.Behaviors = behaviors;
            this.SystemCurve = systemCurve;
        }

        /// <summary>
        /// Method for resetting the agent.
        /// </summary>
        public override void Reset()
        {
            this.T = this.startT;
            Moves.Clear();
            Weights.Clear();
            //SystemCurve.Clear(); // how to clear a curve?
        }

        /// <summary>
        /// Method for running code that should be pre-executed.
        /// </summary>
        public override void PreExecute()
        {
            Moves.Clear();
            Weights.Clear();

            RailsAgentSystem thisAgentSystem = this.AgentSystem as RailsAgentSystem;

        }

        /// <summary>
        /// Method for updating the agent's state.
        /// </summary>
        public override void Execute()
        {
            foreach (BehaviorBase behavior in this.Behaviors)
                behavior.Execute(this);
        }

        /// <summary>
        /// Method for running code that should be post-executed.
        /// </summary>
        public override void PostExecute()
        {
            if (this.Moves.Count == 0) return;

            double totalWeightedMove = 0.0;
            double totalWeight = 0.0;

            for (int i = 0; i < Moves.Count; ++i)
            {
                totalWeightedMove += Weights[i] * Moves[i];
                totalWeight += Weights[i];
            }

            if (totalWeight > 0.0)
                this.T += totalWeightedMove / totalWeight;

            // ## After update position ## //
            // point on curve
            Point3d pointOnCurve = SystemCurve.PointAt(this.T);
            // surface cp get UV
            double u, v;
            (this.AgentSystem as RailsAgentSystem).SystemSurface.ClosestPoint(pointOnCurve, out u, out v);
            Point3d surfaceUV = new Point3d(u, v, 0.0);
            // Find flow line
            //Limit the accuracy to the document tolerance.
            double accuracy = 1.0;
            //accuracy = System.Convert.ToDouble(Math.Max(accuracy, doc.ModelAbsoluteTolerance));
            bool max = false; // set which direction, either 1,0 or 0,1 in GetDir

            //Declare our list of samples.
            List<Point3d> dir0 = SampleCurvature((this.AgentSystem as RailsAgentSystem).SystemSurface, surfaceUV, accuracy, max, 0);
            List<Point3d> dir1 = SampleCurvature((this.AgentSystem as RailsAgentSystem).SystemSurface, surfaceUV, accuracy, max, Math.PI);

            //Remove the first point in dir1 as it's a duplicate
            dir1.RemoveAt(0);
            dir1.Reverse();

            dir1.AddRange(dir0);
            flowCurve = new Polyline(dir1);
        }

        /// <summary>
        /// Method for collecting the geometry that should be displayed.
        /// </summary>
        /// <returns>Returns a list containing each agent's position.</returns>
        public override List<object> GetDisplayGeometries()
        {
            //return new List<object> { Frame, SystemCurve };
            return new List<object> { };
        }

        /// <summary>
        /// The ID of the agent, unique within the given system
        /// </summary>
        public new int Id { get; internal set; }
        
        /// <summary>
        /// creates a list of points from which to make a flow line
        /// </summary>
        /// <param name="srf"></param>
        /// <param name="uv"></param>
        /// <param name="accuracy"></param>
        /// <param name="max"></param>
        /// <param name="angle"></param>
        /// <param name="alg"></param>
        /// <returns>
        /// returns a list of p3d
        /// </returns>
        private List<Point3d> SampleCurvature(Surface srf, Point3d uv, double accuracy, bool max, double angle)
        {
            Point3d p = uv;
            Interval U = srf.Domain(0);
            Interval V = srf.Domain(1);

            List<Point3d> samples = new List<Point3d>();
            do
            {
                //Add the current point.
                samples.Add(srf.PointAt(p.X, p.Y));

                //####################
                //##### JUST RK4 #####
                //####################
                Vector3d dir = new Vector3d();

                dir = RK4(srf, p, max, angle, accuracy, samples);

                if (ReferenceEquals(dir, null))
                {
                    break;
                }

                double s = 0;
                double t = 0;
                Point3d pt = samples[samples.Count - 1] + dir;
                if (!srf.ClosestPoint(pt, out s, out t))
                {
                    break;
                }

                //##################
                //##### Checks #####
                //##################
                //Abort if we've added more than 10,000 samples.
                if (samples.Count > 9999)
                {
                    break;
                }

                //Abort if we've wandered beyond the surface edge.
                if (!U.IncludesParameter(s, true))
                {
                    break;
                }
                if (!V.IncludesParameter(t, true))
                {
                    break;
                }

                //Abort if the new point is basically the same as the old point.
                if ((Math.Abs(p.X - s) < 1e-12) && (Math.Abs(p.Y - t) < 1e-12))
                {
                    break;
                }

                p.X = s;
                p.Y = t;
            } while (true);

            return samples;
        }

        /// <summary>
        /// FLow line method
        /// </summary>
        /// <param name="srf"></param>
        /// <param name="p"></param>
        /// <param name="max"></param>
        /// <param name="angle"></param>
        /// <param name="h"></param>
        /// <param name="samples"></param>
        /// <returns>
        /// Returns a Vector3d
        /// </returns>
        private Vector3d RK4(Surface srf, Point3d p, bool max, double angle, double h, List<Point3d> samples)
        {
            int N = samples.Count;
            Vector3d PrevDir = new Vector3d();
            if (N > 1)
            {
                PrevDir = samples[N - 1] - samples[N - 2];
            }

            Vector3d K1 = GetDir(srf, p, max, angle, h, PrevDir);
            if (ReferenceEquals(K1, null))
            {
                return default(Vector3d);
            }

            //Move the last point in the list along the curvature direction.
            Point3d pt1 = samples[samples.Count - 1] + K1 * 0.5;

            double s = 0;
            double t = 0;
            if (!srf.ClosestPoint(pt1, out s, out t))
            {
                return default(Vector3d);
            }
            pt1.X = s;
            pt1.Y = t;

            Vector3d K2 = GetDir(srf, pt1, max, angle, h, K1);
            if (ReferenceEquals(K2, null))
            {
                return default(Vector3d);
            }

            Point3d pt2 = samples[samples.Count - 1] + K2 * 0.5;

            if (!srf.ClosestPoint(pt2, out s, out t))
            {
                return default(Vector3d);
            }
            pt2.X = s;
            pt2.Y = t;

            Vector3d K3 = GetDir(srf, pt2, max, angle, h, K1);
            if (ReferenceEquals(K3, null))
            {
                return default(Vector3d);
            }

            Point3d pt3 = samples[samples.Count - 1] + K3;

            if (!srf.ClosestPoint(pt3, out s, out t))
            {
                return default(Vector3d);
            }
            pt3.X = s;
            pt3.Y = t;

            Vector3d K4 = GetDir(srf, pt3, max, angle, h, K1);
            if (ReferenceEquals(K4, null))
            {
                return default(Vector3d);
            }

            Vector3d dir = (double)1 / 6 * (K1 + 2 * K2 + 2 * K3 + K4);
            return dir;
        }

        /// <summary>
        /// choose correct direction for flow
        /// </summary>
        /// <param name="srf"></param>
        /// <param name="p"></param>
        /// <param name="max"></param>
        /// <param name="angle"></param>
        /// <param name="h"></param>
        /// <param name="PrevDir"></param>
        /// <returns>
        /// returns a Vector3d
        /// </returns>
        private Vector3d GetDir(Surface srf, Point3d p, bool max, double angle, double h, Vector3d PrevDir)
        {
            //Get the curvature at the current point.
            SurfaceCurvature crv = srf.CurvatureAt(p.X, p.Y);

            //Get the maximum principal direction.
            Vector3d dir = new Vector3d();
            if (crv.Kappa(0) > crv.Kappa(1))
            {
                if (max)
                {
                    dir = crv.Direction(0);
                }
                else
                {
                    dir = crv.Direction(1);
                }
            }
            else
            {
                if (max)
                {
                    dir = crv.Direction(1);
                }
                else
                {
                    dir = crv.Direction(0);
                }
            }

            dir.Rotate(angle, crv.Normal);

            if (!dir.IsValid)
            {
                return default(Vector3d);
            }
            if (!dir.Unitize())
            {
                return default(Vector3d);
            }

            //Scale the direction vector to match our accuracy.
            dir *= h;

            //Flip the direction 180 degrees if it seems to be going backwards
            if (!ReferenceEquals(PrevDir, null))
            {
                if (dir.IsParallelTo(PrevDir, 0.5 * Math.PI) < 0)
                {
                    dir.Reverse();
                }
            }

            return dir;
        }
    }
}
