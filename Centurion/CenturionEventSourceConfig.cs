using System;
using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin;

namespace Centurion
{
    [Serializable]
    public class CenturionEventSourceConfig
    {
        public string ExampleString = "Example String";
        public CenturionEventSourceConfig()
        {

        }

        public static CenturionEventSourceConfig LoadConfig(IPluginConfig pluginConfig)
        {
            var result = new CenturionEventSourceConfig();
            if (pluginConfig.EventSourceConfigs.ContainsKey("CenturionESConfig"))
            {
                var obj = pluginConfig.EventSourceConfigs["CenturionESConfig"];

                if (obj.TryGetValue("ExampleString", out JToken value))
                {
                    result.ExampleString = value.ToString();
                }
            }
            return result;
        }

        public void SaveConfig(IPluginConfig pluginConfig)
        {
            pluginConfig.EventSourceConfigs["CenturionESConfig"] = JObject.FromObject(this);
        }
    }
}
