using System.Windows.Forms;
using RainbowMage.OverlayPlugin;

namespace Centurion
{
    public class CenturionOverlay : OverlayBase<CenturionOverlayConfig>
    {
        public CenturionOverlay(CenturionOverlayConfig config, string name, TinyIoCContainer container) : base(config, name, container)
        {

        }

        public override Control CreateConfigControl()
        {
            return new CenturionOverlayConfigPanel();
        }

        public override void Start()
        {
            // Start the embedded timer when using it.
            // Call base.Start() or timer.Start() to start the embedded timer manually.
            base.Start();
        }

        public override void Stop()
        {
            // Stop the embedded timer when using it.
            // Call base.Stop() or timer.Stop() to stop the embedded timer manually.
            base.Stop();
        }

        protected override void Update()
        {

        }
    }
}
