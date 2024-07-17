using System;
using RainbowMage.OverlayPlugin;

namespace Centurion
{
    public class CenturionOverlayConfig : OverlayConfigBase
    {
        public CenturionOverlayConfig(string name) : base(name)
        {

        }

        private CenturionOverlayConfig() : base(null)
        {

        }

        public override Type OverlayType
        {
            get { return typeof(CenturionOverlay); }
        }
    }
}
