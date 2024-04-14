using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace SeewoKiller
{
    internal class ResolutionAction : Action
    {
        internal uint DPI { get;private set; }
        internal uint XRes { get; private set; }
        internal uint YRes { get; private set; }
        public override string ToString()
        {
            return "调整DPI为" + DPI + "%，分辨率为" + XRes + "×" + YRes;
        }

        internal override bool Execute()
        {
            MessageBox.Show("DPI:" + DPI + "\n分辨率：" + XRes + "×" + YRes);
            return true;
        }

        internal ResolutionAction(uint dpi, uint x_res, uint y_res)
        {
            DPI = dpi;
            XRes = x_res;
            YRes = y_res;
        }
    }
}
