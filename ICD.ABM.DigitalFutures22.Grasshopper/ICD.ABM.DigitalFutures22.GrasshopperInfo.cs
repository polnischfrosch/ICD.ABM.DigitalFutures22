using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ICD.ABM.DigitalFutures22.Grasshopper
{
    public class ICD_ABM_DigitalFutures22_GrasshopperInfo : GH_AssemblyInfo
    {
        public override string Name => "ICD.ABM.DigitalFutures22.Grasshopper";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("05E519BE-58C6-4111-A5C9-284BDDBDF7ED");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}