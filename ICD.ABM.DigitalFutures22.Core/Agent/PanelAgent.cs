using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ICD.ABM.DigitalFutures22.Core.AgentSystem;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ICD.ABM.DigitalFutures22.Core.Agent
{
    public class PanelAgent : AgentBase
    {
        public Point3d Position = Point3d.Unset;
        public Point2d UV = Point2d.Unset;
        public Point2d startUV;
        public Polyline Trail = new Polyline();
        public Polyline Cell = new Polyline();
        public bool isFinished = false;

        public Brep Rail = null;
        public bool IsEdge;

        public List<PanelAgent> NeighborsOnRail = new List<PanelAgent>();
        public List<PanelAgent> DirectNeighborsOnRail = new List<PanelAgent>();
        public List<LineCurve> DirectNeighborConnections = new List<LineCurve>();

        public List<Plane> CutPlanes = new List<Plane>();
        public List<Curve> PanelCuts = new List<Curve>();

        public Brep Panel = null;
        public double PanelArea;

        /// <summary>
        /// The list of 2-dimensional moves
        /// </summary>
        public List<Vector2d> Moves = new List<Vector2d>();
        public List<double> Weights = new List<double>();

        public PanelAgent(Point3d position, List<BehaviorBase> behaviors)
        {
            this.Position = position;
            //this.UV = this.startUV = uvParameter;
            this.Behaviors = behaviors;
        }

        public override void Reset()
        {
            this.Moves.Clear();
            this.UV = this.startUV;
            this.Trail.Clear();
            this.isFinished = false;
            this.Rail = null;

            FindRail();
            FindNeighborsOnRail();
            FindDirectNeighborsOnRail();
        }

        public override void PreExecute()
        {
            this.Moves.Clear();

            //this.Position = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(this.UV.X, this.UV.Y);
            this.Rail = null;
            this.CutPlanes.Clear();
            this.PanelCuts.Clear();
            this.DirectNeighborConnections.Clear();
            FindRail();

            FindNeighborsOnRail();
            FindDirectNeighborsOnRail();
            GetPanelCuts();
        }

        public override void Execute()
        {
            if (this.isFinished)
                return;

            foreach (BehaviorBase behavior in this.Behaviors)
                behavior.Execute(this);
        }

        public override void PostExecute()
        {
            if (this.Moves.Count > 0)
            {
                Vector2d totalWeightedMove = Vector2d.Zero;
                double totalWeight = 0.0;

                for (int i = 0; i < Moves.Count; ++i)
                {
                    totalWeightedMove += Weights[i] * Moves[i];
                    totalWeight += Weights[i];
                }
                this.UV += totalWeightedMove / totalWeight;
            }

            this.Trail.Add((this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(UV.X, UV.Y));
            if (Trail.Count > 20)
                Trail.RemoveAt(0);

            this.Position = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(UV.X, UV.Y);

            // constrain agent to stay on brep
            Point3d cloPt = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.ClosestPoint(this.Position);

            if (this.Position.DistanceTo(cloPt) > Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
            {
                this.Position = (this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.ClosestPoint(this.Position);

                Point3d UV = (this.AgentSystem as PanelAgentSystem).RailEnvironment.UVCoordinates(this.Position);
                this.UV = new Point2d(UV.X, UV.Y);
            }
        }

        public override List<object> GetDisplayGeometries()
        {
            List<object> displayGeometry = new List<object>();

            //if ((this.AgentSystem as UVAgentSystem).SystemSurface == null)
            //    return displayGeometry;

            displayGeometry.Add((this.AgentSystem as PanelAgentSystem).RailEnvironment.BrepObject.Surfaces[0].PointAt(UV.X, UV.Y));

            if ((this.Trail.Count < 2))
                return displayGeometry;

            displayGeometry.Add(this.Trail);
            displayGeometry.Add(this.Cell);

            return displayGeometry;
        }

        /// <summary>
        /// Find all agents that are topologically connected to a given agent
        /// </summary>
        /// <param name="agent">The agent to search from.</param>
        /// <returns>Returns the list of topologically connected neighboring agents.</returns>
        public List<PanelAgent> FindTopologicalNeighbors()
        {
            List<PanelAgent> panelAgentList = new List<PanelAgent>();

            List<int> connections = (this.AgentSystem as PanelAgentSystem).diagram.GetConnections(this.Id);

            foreach (int index in connections)
            {
                panelAgentList.Add((PanelAgent)((this.AgentSystem as PanelAgentSystem).Agents[index]));
            }

            return panelAgentList;
        }

        public void FindRail()
        {
            foreach (Brep rail in (this.AgentSystem as PanelAgentSystem).RailEnvironment.Rails)
            {
                Point3d cloPt = rail.ClosestPoint(this.Position);

                if (this.Position.DistanceTo(cloPt) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                {
                    this.Rail = rail;
                }
            }
        }

        public void FindNeighborsOnRail()
        {
            //List<PanelAgent> topoNeighbors = FindTopologicalNeighbors();

            List<PanelAgent> neighborsOnRail = new List<PanelAgent>();

            //foreach (PanelAgent otherAgent in topoNeighbors)
            foreach (PanelAgent otherAgent in AgentSystem.Agents)
            {
                if (otherAgent != this)
                {
                    Point3d cloPt = Rail.ClosestPoint(otherAgent.Position);


                    if (otherAgent.Position.DistanceTo(cloPt) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        neighborsOnRail.Add(otherAgent);
                    }
                }
            }
            this.NeighborsOnRail = neighborsOnRail;
        }

        public void FindDirectNeighborsOnRail()
        {
            List<PanelAgent> directNeighborsOnRail = new List<PanelAgent>();

            List<double> distances = new List<double>();

            foreach (PanelAgent otherAgent in NeighborsOnRail)
            {
                distances.Add(this.Position.DistanceTo(otherAgent.Position));
            }

            double minVal = distances.Min();
            int index = distances.IndexOf(minVal);

            directNeighborsOnRail.Add(NeighborsOnRail[index]);

            // if agents are not on the outer edge, they should have another neighbor
            if (!this.IsEdge)
            {
                var secondLowest = distances.OrderBy(num => num).ElementAt(1);

                int indexSecondLowest = distances.IndexOf(secondLowest);

                // check whether the second closest neighbor is in the wrong direction
                if ((UV.X < NeighborsOnRail[index].UV.X &&
                NeighborsOnRail[indexSecondLowest].UV.X > NeighborsOnRail[index].UV.X) ||
                (UV.X > NeighborsOnRail[index].UV.X &&
                NeighborsOnRail[indexSecondLowest].UV.X < NeighborsOnRail[index].UV.X) ||
                (UV.X < NeighborsOnRail[index].UV.X &&
                 UV.X < NeighborsOnRail[indexSecondLowest].UV.X))
                {
                    var thirdLowest = distances.OrderBy(num => num).ElementAt(2);

                    int indexThirdLowest = distances.IndexOf(thirdLowest);
                    directNeighborsOnRail.Add(NeighborsOnRail[indexThirdLowest]);
                }
                else
                {
                    directNeighborsOnRail.Add(NeighborsOnRail[indexSecondLowest]);
                }
            }

            this.DirectNeighborsOnRail = directNeighborsOnRail;
        }

        public void FindDirectNeighborsOnRailUV()
        {
            List<PanelAgent> directNeighborsOnRail = new List<PanelAgent>();

            List<double> distances = new List<double>();

            foreach (PanelAgent otherAgent in NeighborsOnRail)
            {
                distances.Add(this.Position.DistanceTo(otherAgent.Position));
            }

            double minVal = distances.Min();
            int index = distances.IndexOf(minVal);

            var secondLowest = distances.OrderBy(num => num).ElementAt(1);

            int indexSecondLowest = distances.IndexOf(secondLowest);

            // check whether the u values of the second closest are closer
            if ((UV.X < NeighborsOnRail[index].UV.X &&
                NeighborsOnRail[indexSecondLowest].UV.X < NeighborsOnRail[index].UV.X))
            {
                var thirdLowest = distances.OrderBy(num => num).ElementAt(2);

                int indexThirdLowest = distances.IndexOf(thirdLowest);
                directNeighborsOnRail.Add(NeighborsOnRail[indexThirdLowest]);
            }
            else
            {
                directNeighborsOnRail.Add(NeighborsOnRail[index]);
            }

            // if agents are not on the outer edge, they should have another neighbor
            if (!this.IsEdge)
            {


                // check whether the second closest neighbor is in the wrong direction
                if ((UV.X < NeighborsOnRail[index].UV.X &&
                NeighborsOnRail[indexSecondLowest].UV.X > NeighborsOnRail[index].UV.X) ||
                (UV.X > NeighborsOnRail[index].UV.X &&
                NeighborsOnRail[indexSecondLowest].UV.X < NeighborsOnRail[index].UV.X) ||
                (UV.X < NeighborsOnRail[index].UV.X &&
                 UV.X < NeighborsOnRail[indexSecondLowest].UV.X //||
                                                                //UV.X > NeighborsOnRail[index].UV.X &&
                                                                //UV.X > NeighborsOnRail[indexSecondLowest].UV.X //||
                                                                //NeighborsOnRail[indexSecondLowest].UV.X < NeighborsOnRail[index].UV.X ||
                                                                //NeighborsOnRail[indexSecondLowest].UV.X > NeighborsOnRail[index].UV.X
                 ))

                {
                    var thirdLowest = distances.OrderBy(num => num).ElementAt(2);

                    int indexThirdLowest = distances.IndexOf(thirdLowest);
                    directNeighborsOnRail.Add(NeighborsOnRail[indexThirdLowest]);
                }
                else
                {
                    directNeighborsOnRail.Add(NeighborsOnRail[indexSecondLowest]);
                }
            }

            this.DirectNeighborsOnRail = directNeighborsOnRail;
        }

        public void GetPanelCuts()
        {
            PanelAgentSystem system = this.AgentSystem as PanelAgentSystem;

            foreach (PanelAgent dNeighbor in DirectNeighborsOnRail)
            {

                LineCurve ln = new LineCurve(system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                                                          UV.X, UV.Y),
                                             system.RailEnvironment.BrepObject.Surfaces[0].PointAt(
                                                          dNeighbor.UV.X, dNeighbor.UV.Y));
                DirectNeighborConnections.Add(ln);

                Point3d mid = ln.PointAtNormalizedLength(0.5);

                LineCurve ln2D = new LineCurve(new Point3d(UV.X, UV.Y, 0.0),
                                               new Point3d(dNeighbor.UV.X, dNeighbor.UV.Y, 0.0));

                Point3d mid2D = ln2D.PointAtNormalizedLength(0.5);

                Vector3d vec = ln.PointAtStart - ln.PointAtEnd;

                Vector3d dir = Vector3d.CrossProduct(vec, system.RailEnvironment.BrepObject.Surfaces[0].NormalAt(
                    system.RailEnvironment.UVCoordinates(mid).X,
                    system.RailEnvironment.UVCoordinates(mid).Y));

                //plane intersection
                Curve[] itxCrv = null;
                Point3d[] itxPts = null;

                Point3d plnPos = system.RailEnvironment.BrepObject.Surfaces[0].PointAt(mid2D.X, mid2D.Y);

                Plane cutPlane = new Plane(plnPos, dir, system.RailEnvironment.BrepObject.Surfaces[0].NormalAt(
                  system.RailEnvironment.UVCoordinates(mid).X,
                  system.RailEnvironment.UVCoordinates(mid).Y));

                bool intersect = Rhino.Geometry.Intersect.Intersection.BrepPlane(Rail, cutPlane, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out itxCrv, out itxPts);

                CutPlanes.Add(cutPlane);

                if (intersect)
                {
                    if (itxCrv.Count() > 1)
                    {
                        foreach (Curve c in itxCrv)
                        {
                            double t;
                            bool closest = c.ClosestPoint(plnPos, out t, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                            if (closest)
                            {
                                PanelCuts.Add(c);
                            }
                        }
                    }
                    else
                    {
                        PanelCuts.Add(itxCrv[0]);
                    }
                }
            }
        }

        public Brep GetPanelSurface()
        {
            Brep[] splits = Rail.Split(PanelCuts, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            Brep panel = null;

            foreach (Brep b in splits)
            {
                if (b.ClosestPoint(Position).DistanceTo(Position) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) ;
                {
                    panel = b;
                }
            }

            Panel = panel;
            PanelArea = panel.GetArea();

            return panel;
        }
    }
}
