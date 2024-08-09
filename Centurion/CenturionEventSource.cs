using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using FFXIV_ACT_Plugin.Common.Models;
using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin;
using RainbowMage.OverlayPlugin.EventSources;

namespace Centurion
{
    public class CenturionEventSource : EventSourceBase
    {
        public enum LogMessageChatType : uint
        {
            Say = 10,
            Shout = 11,
            Party = 14,
            LS1 = 16,
            LS2 = 17,
            LS3 = 18,
            LS4 = 19,
            LS5 = 20,
            LS6 = 21,
            LS7 = 22,
            LS8 = 23,
            Emote = 28,
            DirectEmote = 29,
            Yell = 30,
            CWLS1 = 37,
            Echo = 56,
            System = 57,
            CWLS2 = 101,
            CWLS3 = 102,
            CWLS4 = 103,
            CWLS5 = 104,
            CWLS6 = 105,
            CWLS7 = 106,
            CWLS8 = 107
        }

        private static readonly Dictionary<string, uint> INSTANCE_STRING_MAP = new Dictionary<string, uint> {
            { "", 1 },
            { "", 2 },
            { "", 3 },
            { "", 4 },
            { "", 5 },
            { "", 6 },
            { "", 7 },
            { "", 8 },
            { "", 9 }
        };

        private static readonly Dictionary<uint, uint[]> ZONE_MOBIDS_MAP = new Dictionary<uint, uint[]> {
            { 134, new uint[] {2962,2945,2928} },
            { 135, new uint[] {2963,2946,2929} },
            { 137, new uint[] {2964,2947,2930} },
            { 138, new uint[] {2965,2948,2931} },
            { 139, new uint[] {2966,2949,2932} },
            { 140, new uint[] {2957,2940,2923} },
            { 141, new uint[] {2958,2941,2924} },
            { 145, new uint[] {2959,2942,2925} },
            { 146, new uint[] {2960,2943,2926} },
            { 147, new uint[] {2961,2944,2927} },
            { 148, new uint[] {2953,2936,2919,887} },
            { 152, new uint[] {2954,2937,2920,887} },
            { 153, new uint[] {2955,2938,2921,887} },
            { 154, new uint[] {2956,2939,2922,887} },
            { 155, new uint[] {2968,2951,2934,655} },
            { 156, new uint[] {2969,2952,2935} },
            { 180, new uint[] {2967,2950,2933} },
            { 398, new uint[] {4375,4364,4365,4352,4353,3789} },
            { 399, new uint[] {4376,4366,4367,4354,4355} },
            { 400, new uint[] {4377,4368,4369,4356,4357} },
            { 401, new uint[] {4378,4370,4371,4358,4359} },
            { 402, new uint[] {4380,4372,4373,4360,4361,3783} },
            { 397, new uint[] {4374,4362,4363,4350,4351} },
            { 612, new uint[] {5987,5990,5991,6008,6009} },
            { 620, new uint[] {5988,5992,5993,6010,6011} },
            { 621, new uint[] {5989,5994,5995,6012,6013,6392} },
            { 613, new uint[] {5984,5996,5997,6002,6003} },
            { 614, new uint[] {5985,5998,5999,6004,6005,6290} },
            { 622, new uint[] {5986,6000,6001,6006,6007} },
            { 813, new uint[] {8905,8906,8907,8908,8909,8915,8916} },
            { 814, new uint[] {8910,8911,8912,8913,8914,8915,8916,8822} },
            { 815, new uint[] {8900,8901,8902,8903,8904,8915,8916} },
            { 816, new uint[] {8653,8654,8655,8656,8657,8915,8916} },
            { 817, new uint[] {8890,8891,8892,8893,8894,8915,8916} },
            { 818, new uint[] {8895,8896,8897,8898,8899,8915,8916,8234} },
            { 956, new uint[] {10617,10623,10624,10635,10636,10615,10616} },
            { 957, new uint[] {10618,10625,10626,10637,10638,10615,10616,10269} },
            { 958, new uint[] {10619,10627,10628,10639,10640,10615,10616} },
            { 959, new uint[] {10620,10629,10630,10641,10642,10615,10616} },
            { 960, new uint[] {10622,10633,10634,10645,10646,10615,10616,10400} },
            { 961, new uint[] {10621,10631,10632,10643,10644,10615,10616} },
            { 1187, new uint[] { 13358, 13362, 13361, 13144, 13145, 13406, 13407 } },
            { 1188, new uint[] { 13446, 13442, 13443, 13146, 13147, 13406, 13407 } },
            { 1189, new uint[] { 12754, 12692, 12753, 13148, 13149, 13406, 13407 } },
            { 1190, new uint[] { 13399, 13400, 13401, 13150, 13151, 13406, 13407, 12733 } },
            { 1191, new uint[] { 13156, 13157, 13158, 13152, 13153, 13406, 13407 } },
            { 1192, new uint[] { 13437, 13435, 13436, 13154, 13155, 13406, 13407, 13049 } }
        };

        private static Dictionary<uint, string> WORLDID_WORLDNAME_MAP = new Dictionary<uint, string>();

        private readonly FFXIVRepository repository;

        private readonly List<string> importedLogs = new List<string>();

        // キャラクターログイン
        private const string CenturionPlayerLoggedInEvent = "CenturionPlayerLoggedInEvent";
        // キャラクターログアウト
        private const string CenturionPlayerLoggedOutEvent = "CenturionPlayerLoggedOutEvent";
        // ゾーン移動
        private const string CenturionZoneChangedEvent = "CenturionZoneChangedEvent";
        // ゾーンインスタンス移動
        private const string CenturionZoneInstanceChangedEvent = "CenturionZoneInstanceChangedEvent";
        // <pos>や<flag>の書式を含むChat受診
        private const string CenturionLocationNotifiedEvent = "CenturionLocationNotifiedEvent";
        // 対象Mob(HP100%)に対するActionイベントの受診
        private const string CenturionMobFAEvent = "CenturionMobFAEvent";
        // MobHuntのトリガーに関するイベント(SSSの配下POPと失敗時の消失)
        private const string CenturionMobTriggerEvent = "CenturionMobTriggerEvent";
        // モブの状態変化
        private const string CenturionMobStateChangedEvent = "CenturionMobStateChangedEvent";
        // モブの位置情報(HP100%で2秒以上停止した定点)
        private const string CenturionMobLocationEvent = "CenturionMobLocationEvent";
        // モブ含むキャラデータ
        private const string CenturionCombatData = "CenturionCombatData";

        // 現在のプレイヤーのCombatant
        private Combatant currentPlayer = null;

        // 現在のプレイヤーが居るZone
        private CenturionZone currentZone = null;

        // LogParseでパースしたZoneId
        private uint destinationZoneId = 0;
        private string destinationZoneName = "";

        // LogParseでパースしたZoneInstance
        private uint destinationZoneInstance = 0;
        private string destinationZoneInstanceName = "";

        private DateTime? disconnectStartTime = null;

        public BuiltinEventConfig Config { get; set; }

        // Original Timer
        System.Timers.Timer originalTimer;

        public CenturionEventSource(TinyIoCContainer container) : base(container)
        {
            Name = "CenturionES";

            var haveRepository = container.TryResolve(out repository);
            if (!haveRepository)
            {
                Log(LogLevel.Warning, "Could not construct CenturionEventSource: missing repository");
                return;
            }

            WORLDID_WORLDNAME_MAP = new Dictionary<uint, string>(repository.GetResourceDictionary(ResourceType.WorldList_EN));

            // Register Events subscribe to other EventSources/Overlays
            RegisterEventTypes(new List<string>
            {
                 CenturionLocationNotifiedEvent,
                 CenturionMobFAEvent,
                 CenturionMobTriggerEvent,
                 CenturionMobStateChangedEvent,
                 CenturionMobLocationEvent,
                 CenturionCombatData,
            });

            // These events need to deliver cached values to new subscribers.
            RegisterCachedEventTypes(new List<string>
            {
                 CenturionPlayerLoggedInEvent,
                 CenturionPlayerLoggedOutEvent,
                 CenturionZoneChangedEvent,
                 CenturionZoneInstanceChangedEvent,
            });

            // Register EventHandler
            // This EventHandler is called from other EventSources/Overlays
            // You can execute some process or response data.
            RegisterEventHandler("CenturionSay", (msg) =>
            {
                var text = msg["text"]?.ToString();
                if (text != null)
                {
                    ActGlobals.oFormActMain.TTS(text);
                }
                return null;
            });

            RegisterEventHandler("CenturionCurrentTime", (msg) =>
            {
                return new JObject
                {
                    ["time"] = DateTimeOffset.UtcNow.ToString()
                };
            });

            RegisterEventHandler("CenturionGetVersion", (msg) =>
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                var ver = assembly.Version;

                // アセンブリ名 1.0.0.0
                return new JObject
                {
                    ["name"] = assembly.Name,
                    ["version"] = $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}"
                };
            });

            RegisterEventHandler("CenturionInitClient", (msg) =>
            {
                if (currentPlayer != null && destinationZoneId > 0)
                {
                    var currentTime = DateTime.UtcNow;
                    DispatchPlayerLoggedInEvent(currentTime, currentPlayer, destinationZoneId, destinationZoneName);
                    currentZone = new CenturionZone(this, currentPlayer.CurrentWorldID, destinationZoneId, destinationZoneName);
                    if (destinationZoneInstance > 0)
                    {
                        DispatchZoneInstanceChangedEvent(currentTime, destinationZoneInstance, destinationZoneInstanceName);
                        currentZone.CurrentInstance = destinationZoneInstance;
                        destinationZoneInstance = 0;
                        destinationZoneInstanceName = "";
                    }
                }
                return new JObject()
                {
                    ["result"] = true
                };
            });

            RegisterEventHandler("CenturionSetZoneMobs", (msg) =>
            {
                try
                {
                    foreach (KeyValuePair<string, JToken> item in (JObject)msg["data"])
                    {
                        try
                        {
                            uint key = UInt32.Parse(item.Key);
                            uint[] mobids = ((JArray)item.Value).ToObject<uint[]>();
                            ZONE_MOBIDS_MAP[key] = mobids;
                            Log(LogLevel.Info, $"CenturionSetZoneMobs: {key} = {mobids.Select(mobid => mobid.ToString()).ToArray()}");
                        }
                        catch (Exception e)
                        {
                            Log(LogLevel.Warning, $"CenturionSetZoneMobs: ignore key {item.Key}, reason = {e.Message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(LogLevel.Error, $"CenturionSetZoneMobs: {e.Message}");
                }
                return null;
            });

            ActGlobals.oFormActMain.BeforeLogLineRead += LogLineHandler;
        }

        public override Control CreateConfigControl()
        {
            return null;
        }

        public override void LoadConfig(IPluginConfig config)
        {
            Config = container.Resolve<BuiltinEventConfig>();
        }

        public override void SaveConfig(IPluginConfig config)
        {
        }

        public override void Start()
        {
            // Start the embedded timer when using it.
            // Call base.Start() or timer.Change(0, interval) to start the embedded timer manually.
            // base.Start();
            // timer.Change(0, 1000);

            // Start Original Timer
            originalTimer = new System.Timers.Timer()
            {
                Interval = 100,
                AutoReset = true,
            };
            originalTimer.Elapsed += (obj, args) =>
            {
                Update();
            };
            originalTimer.Start();

            Log(LogLevel.Info, "Plugin Started.");
        }

        public override void Stop()
        {
            // Stop original timer
            originalTimer.Stop();

            Log(LogLevel.Info, "Plugin Stopped.");

            // Stop the embedded timer when using it.
            // Call base.stop() or timer.Change(-1, -1) to stop the embedded timer manually.
            // base.Stop();
            // timer.Change(-1, -1);
        }

        protected override void Update()
        {
            var currentTime = DateTime.UtcNow;

            try
            {
                currentPlayer = repository.GetCombatants().FirstOrDefault(c => c.type == 1);
                if (currentPlayer != null)
                {
                    disconnectStartTime = null;
                    // ログアウト状態でログインに必要な情報(currentPlayerとdestinationZoneId)が揃っていたらログインに切り替える
                    if (currentZone == null)
                    {
                        if (currentPlayer != null && destinationZoneId > 0)
                        {
                            DispatchPlayerLoggedInEvent(currentTime, currentPlayer, destinationZoneId, destinationZoneName);
                            currentZone = new CenturionZone(this, currentPlayer.CurrentWorldID, destinationZoneId, destinationZoneName);
                        }
                    }
                }
                else if (currentZone != null)
                {
                    // ログイン状態で3秒間接続が切れていた(Combatantsが取れなかった)らログアウトに切り替える
                    if (disconnectStartTime != null)
                    {
                        var elapsedTime = currentTime - (DateTime)disconnectStartTime;
                        if (elapsedTime.TotalSeconds > 3)
                        {
                            DispatchPlayerLoggedOutEvent(currentTime, currentPlayer, currentZone);
                            destinationZoneId = 0;
                            destinationZoneName = "";
                            currentPlayer = null;
                            currentZone.Dispose(currentTime);
                            currentZone = null;
                        }
                    }
                    else
                    {
                        disconnectStartTime = currentTime;
                    }
                }

                // 必要な情報が揃っわなければ、この回の残処理はスキップ
                if (currentPlayer == null || currentZone == null) return;

                if (currentZone.ZoneId != destinationZoneId)
                {
                    // Zone変更
                    currentZone.Dispose(currentTime);
                    currentZone = new CenturionZone(this, currentPlayer.CurrentWorldID, destinationZoneId, destinationZoneName);
                }

                if (destinationZoneInstance > 0)
                {
                    // インスタンス変更
                    DispatchZoneInstanceChangedEvent(currentTime, destinationZoneInstance, destinationZoneInstanceName);
                    currentZone.CurrentInstance = destinationZoneInstance;
                    destinationZoneInstance = 0;
                    destinationZoneInstanceName = "";
                }

                var allCombatants = repository.GetCombatants();
                currentZone.Update(currentTime, allCombatants);
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, e.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && originalTimer != null)
            {
                originalTimer.Dispose();
                originalTimer = null;
            }
            // base.Dispose(disposing);
        }

        private void LogLineHandler(bool isImport, LogLineEventArgs args)
        {
            if (isImport)
            {
                lock (importedLogs)
                {
                    importedLogs.Add(args.originalLogLine);
                }
                return;
            }

            DateTime utcNow = DateTime.UtcNow;

            try
            {
                LogMessageType lineType = (LogMessageType)args.detectedType;
                var line = args.originalLogLine.Split('|');

                switch (lineType)
                {
                    case LogMessageType.ChangeZone:
                        if (line.Length < 4) return;
                        destinationZoneId = Convert.ToUInt32(line[2], 16);
                        destinationZoneName = line[3];
                        // Event will be dipatched on next Update
                        break;

                    case LogMessageType.ChangePrimaryPlayer:
                        if (line.Length < 4) return;
                        //currentPlayerID = Convert.ToUInt32(line[2], 16);
                        //currentPlayerName = line[3];
                        break;

                    case LogMessageType.NetworkAbility:
                        IEnumerable<CenturionMob> candidateMobs = currentZone?.Mobs;
                        if (candidateMobs != null && candidateMobs.Any())
                        {
                            if (line.Length < 26 || line[24] == "" || line[25] == "") return;
                            // uint attackerId = Convert.ToUInt32(line[2], 16);
                            string attackerName = line[3];
                            // uint actionId = Convert.ToUInt32(line[4], 16);
                            string actionName = line[5];
                            uint targetId = Convert.ToUInt32(line[6], 16);
                            string targetName = line[7];
                            uint targetHP = Convert.ToUInt32(line[24], 10);
                            uint targetMaxHP = Convert.ToUInt32(line[25], 10);
                            if (targetHP == targetMaxHP)
                            {
                                var targetMob = candidateMobs.FirstOrDefault(mob => mob.Id == targetId);
                                if (targetMob != null)
                                {
                                    DispatchMobFAEvent(utcNow, attackerName, actionName, targetMob, targetName);
                                }
                            }
                        }
                        break;

                    case LogMessageType.LogLine:
                        if (line.Length <= 4) return;
                        LogMessageChatType logMessageChatType = (LogMessageChatType)Convert.ToUInt32(line[2], 16);
                        switch (logMessageChatType)
                        {
                            case LogMessageChatType.System:
                                var message = line[4];
                                if (ValidateZoneInstanceChange(message)) return;
                                if (ValidateSSChallengeStarted(message) && currentZone != null)
                                {
                                    DispatchMobTriggerEvent(utcNow, "SSChallengeStarted", message, currentZone);
                                    return;
                                }
                                if (ValidateSSChallengeFailed(message) && currentZone != null)
                                {
                                    DispatchMobTriggerEvent(utcNow, "SSChallengeFailed", message, currentZone);
                                    return;
                                }
                                break;

                            case LogMessageChatType.Say:
                            case LogMessageChatType.Shout:
                            case LogMessageChatType.Party:
                            case LogMessageChatType.LS1:
                            case LogMessageChatType.LS2:
                            case LogMessageChatType.LS3:
                            case LogMessageChatType.LS4:
                            case LogMessageChatType.LS5:
                            case LogMessageChatType.LS6:
                            case LogMessageChatType.LS7:
                            case LogMessageChatType.LS8:
                            case LogMessageChatType.Yell:
                            case LogMessageChatType.Echo:
                            case LogMessageChatType.CWLS1:
                            case LogMessageChatType.CWLS2:
                            case LogMessageChatType.CWLS3:
                            case LogMessageChatType.CWLS4:
                            case LogMessageChatType.CWLS5:
                            case LogMessageChatType.CWLS6:
                            case LogMessageChatType.CWLS7:
                            case LogMessageChatType.CWLS8:
                                ValidateLocationNotified(utcNow, logMessageChatType, line);
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Failed to process log line: {args.originalLogLine}, Exception: {ex}");
            }


        }

        private bool ValidateZoneInstanceChange(string msg)
        {
            Regex reg = new Regex("(インスタンスエリア「|You are now in the instanced area |Vous avez été transportée vers la zone instanciée |Du bist nun in dem instanziierten Areal )(?<zone>.+)(?<instance>||||||||)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = reg.Match(msg);
            if (m.Success)
            {
                destinationZoneInstance = INSTANCE_STRING_MAP[m.Groups["instance"].Value];
                destinationZoneInstanceName = m.Groups["zone"].Value;
                Log(LogLevel.Info, String.Format("{0}, {1}", destinationZoneInstance, destinationZoneInstanceName));
            }
            return m.Success;
        }

        /// <summary>
        /// 「配下が湧きました」のメッセージに該当するか
        /// </summary>
        /// <param name="msg">メッセージ</param>
        /// <returns>該当 or 非該当</returns>
        /// <remarks>https://xivapi.com/LogMessage?columns=ID,Text_ja,Text_en,Text_de,Text_fr&page=94</remarks>
        private bool ValidateSSChallengeStarted(string msg)
        {
            string[] candidates = {
                "Die Helfer eines besonderen Hochwilds beginnen ihre Erkundung...",
                "The minions of an extraordinarily powerful mark are on the hunt for prey...",
                "Les sous-fifres du monstre d'élite ont commencé à vous espionner...",
                "特殊なリスキーモブの配下が、偵察活動を開始したようだ……"
            };
            return candidates.Contains(msg);
        }

        /// <summary>
        /// 「配下が帰還しました」のメッセージに該当するか
        /// </summary>
        /// <param name="msg">メッセージ</param>
        /// <returns>該当 or 非該当</returns>
        /// <remarks>https://xivapi.com/LogMessage?columns=ID,Text_ja,Text_en,Text_de,Text_fr&page=94</remarks>
        private bool ValidateSSChallengeFailed(string msg)
        {
            string[] candidates = {
                "Die Helfer eines besonderen Hochwilds haben ihre Erkundung beendet.",
                "The minions of an extraordinarily powerful mark have withdrawn...",
                "Les sous-fifres du monstre d'élite ont arrêté leur mission d'espionnage et ont déserté les lieux...",
                "特殊なリスキーモブの配下が、偵察活動を終えて帰還したようだ……"
            };
            return candidates.Contains(msg);
        }

        private (string, uint) ResolveZoneInstance(string name)
        {
            string zoneName = name;
            uint instance = 0;
            Regex reg = new Regex(@"(?<zone>.+)(?<instance>||||||||)$", RegexOptions.Singleline);
            Match m = reg.Match(name);
            if (m.Success)
            {
                zoneName = m.Groups["zone"].Value;
                instance = INSTANCE_STRING_MAP[m.Groups["instance"].Value];
            }
            return (zoneName, instance);
        }

        private static string GetWorldName(uint worldId) => WORLDID_WORLDNAME_MAP.TryGetValue(worldId, out string worldName) ? worldName : "Unknown";

        private string ResolveCharacterName(string name)
        {
            Regex reg = new Regex(@"(?<firstName>[A-Z]\S*)\s(?<familyName>[A-Z]\S*)(?<world>[A-Z]\S*)$", RegexOptions.Singleline);
            Match m = reg.Match(name);
            if (m.Success)
            {
                return $"{m.Groups["firstName"]} {m.Groups["familyName"]}@{m.Groups["world"]}";
            }
            string worldName = currentPlayer != null ? GetWorldName(currentPlayer.WorldID) : "Unknown";
            reg = new Regex(@"(?<firstName>[A-Z]\S*)\s(?<familyName>[A-Z]\S*)$", RegexOptions.Singleline);
            m = reg.Match(name);
            if (m.Success)
            {
                return $"{m.Groups["firstName"]} {m.Groups["familyName"]}@{worldName}";
            }
            return currentPlayer != null ? $"{currentPlayer.Name}@{worldName}" : "Unknown@Unknown";
        }

        private void ValidateLocationNotified(DateTime utcDateTime, LogMessageChatType logType, string[] logLine)
        {
            var message = logLine[4];
            Regex reg = new Regex(@"(?<zoneInstance>.+)\s+\(\s+(?<x>\d+\.\d)\s+,\s+(?<y>\d+\.\d)\s+\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = reg.Match(message);
            if (m.Success)
            {
                var zoneInstance = m.Groups["zoneInstance"].Value;
                var (zoneName, instance) = ResolveZoneInstance(zoneInstance);
                var playerName = ResolveCharacterName(logLine[3]);
                float x = Convert.ToSingle(m.Groups["x"].Value);
                float y = Convert.ToSingle(m.Groups["y"].Value);

                DispatchLocationNotifiedEvent(utcDateTime, logType, message, playerName, zoneName, instance, x, y);
            }
        }


        [Serializable]
        internal class CenturionPlayerLoggedInObject
        {
            public string type = CenturionPlayerLoggedInEvent;
            public string timestamp;
            public uint playerId;
            public string playerName;
            public uint playerWorldId;
            public uint worldId;
            public uint zoneId;
            public string zoneName;
        };

        [Serializable]
        internal class CenturionPlayerLoggedOutObject
        {
            public string type = CenturionPlayerLoggedOutEvent;
            public string timestamp;
            public uint playerId;
            public string playerName;
            public uint playerWorldId;
            public uint worldId;
            public uint zoneId;
            public string zoneName;
        }

        [Serializable]
        internal class CenturionZoneChangedObject
        {
            public string type = CenturionZoneChangedEvent;
            public string timestamp;
            public uint worldId;
            public uint zoneId;
            public string zoneName;
        }

        [Serializable]
        internal class CenturionZoneInstanceChangedObject
        {
            public string type = CenturionZoneInstanceChangedEvent;
            public string timestamp;
            public string zoneName;
            public uint instance;
        }

        [Serializable]
        internal class CenturionLocationNotifiedObject
        {
            public string type = CenturionLocationNotifiedEvent;
            public string timestamp;
            public uint logType;
            public string message;
            public string pc;
            public string zoneName;
            public uint instance;
            public float x;
            public float y;
        }

        [Serializable]
        internal class CenturionMobFAObject
        {
            public string type = CenturionMobFAEvent;
            public string timestamp;
            public string message;
            public uint id;
            public uint mobId;
            public string mobName;
            public string attacker;
            public string actionName;
        }

        [Serializable]
        internal class CenturionMobTriggerObject
        {
            public string type = CenturionMobTriggerEvent;
            public string timestamp;
            public string message;
            public string triggerType;
            public uint worldId;
            public uint zoneId;
        }

        [Serializable]
        internal class CenturionMobStateChangedObject
        {
            public string type = CenturionMobStateChangedEvent;
            public string timestamp;
            public string state;
            public uint worldId;
            public uint zoneId;
            public uint id;
            public uint mobId;
            public float x;
            public float y;
            public float z;
            public float distance;
            public float hpp;
        }

        [Serializable]
        internal class CenturionMobLocationObject
        {
            public string type = CenturionMobLocationEvent;
            public string timestamp;
            public uint worldId;
            public uint zoneId;
            public uint id;
            public uint mobId;
            public float x;
            public float y;
            public float z;
        }

        [Serializable]
        public class BaseCombatant
        {
            public uint ID;
            public string Name;
            public uint OwnerID;
            public uint TargetID;
            public uint BNpcID;
            public uint BNpcNameID;
            public byte type;

            public BaseCombatant(Combatant combatant)
            {
                ID = combatant.ID;
                Name = combatant.Name;
                OwnerID = combatant.OwnerID;
                TargetID = combatant.TargetID;
                BNpcID = combatant.BNpcID;
                BNpcNameID = combatant.BNpcNameID;
                type = combatant.type;
            }
        }

        [Serializable]
        public class PosCombatant : BaseCombatant
        {
            public float PosX;
            public float PosY;
            public float PosZ;
            public float Heading;
            public float Distance;
            public PosCombatant(Combatant combatant) : base(combatant)
            {
                PosX = combatant.PosX;
                PosY = combatant.PosY;
                PosZ = combatant.PosZ;
                Heading = combatant.Heading;
                Distance = combatant.EffectiveDistance;
            }
        }

        [Serializable]
        public class CharCombatant : PosCombatant
        {
            public int Job;
            public int Level;
            public uint WorldID;
            public string WorldName;
            public float CurrentHP;
            public float MaxHP;
            public float HPP;
            public CharCombatant(Combatant combatant) : base(combatant)
            {
                Job = combatant.Job;
                Level = combatant.Level;
                WorldID = combatant.WorldID;
                WorldName = combatant.WorldName;
                CurrentHP = combatant.CurrentHP;
                MaxHP = combatant.MaxHP;
                HPP = combatant.MaxHP > 0 ? (float)combatant.CurrentHP / (float)combatant.MaxHP : 0;
            }
        }


        [Serializable]
        internal class CenturionCombatDataObject
        {
            public string type = CenturionCombatData;
            public string timestamp;
            public CharCombatant self;
            public List<CharCombatant> party;
            public List<CharCombatant> pcs;
            public List<CharCombatant> targets;
            public uint zoneId;
            public string zoneName;
            public int countPC;
            public int countNPC;
            public int countChocobos;
            public int countPets;
            public int countTotal;
            public bool isCrowded;
            // public List<PosCombatant> others;
        }

        internal void DispatchPlayerLoggedInEvent(DateTime utcDateTime, Combatant player, uint zoneId, string zoneName)
        {
            var eventObject = new CenturionPlayerLoggedInObject();
            try
            {
                eventObject.timestamp = utcDateTime.ToString("s") + "Z";
                eventObject.playerId = player.ID;
                eventObject.playerName = player.Name;
                eventObject.playerWorldId = player.WorldID;
                eventObject.worldId = player.CurrentWorldID;
                eventObject.zoneId = zoneId;
                eventObject.zoneName = zoneName;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchPlayerLoggedInEvent: {0}", ex);
            }
            DispatchAndCacheEvent(JObject.FromObject(eventObject));
            logger.Log(LogLevel.Info, $"PlayerLoggedIn: {player.Name}@{player.WorldName}");
        }

        internal void DispatchPlayerLoggedOutEvent(DateTime utcDateTime, Combatant player, CenturionZone zone)
        {
            var eventObject = new CenturionPlayerLoggedOutObject();
            try
            {
                eventObject.timestamp = utcDateTime.ToString("s") + "Z";
                eventObject.playerId = player.ID;
                eventObject.playerName = player.Name;
                eventObject.playerWorldId = player.WorldID;
                eventObject.worldId = player.CurrentWorldID;
                eventObject.zoneId = zone.ZoneId;
                eventObject.zoneName = zone.ZoneName;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchPlayerLoggedOutEvent: {0}", ex);
            }
            DispatchAndCacheEvent(JObject.FromObject(eventObject));
            logger.Log(LogLevel.Info, $"PlayerLoggedOut: {player.Name}@{player.WorldName}");
        }

        internal void DispatchZoneChangedEvent(DateTime utcDateTime, uint worldId, uint zoneId, string zoneName)
        {
            var eventObject = new CenturionZoneChangedObject();
            try
            {
                eventObject.timestamp = utcDateTime.ToString("s") + "Z";
                eventObject.worldId = worldId;
                eventObject.zoneId = zoneId;
                eventObject.zoneName = zoneName;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchZoneChangedEvent: {0}", ex);
            }
            DispatchAndCacheEvent(JObject.FromObject(eventObject));
            logger.Log(LogLevel.Info, $"ZoneChanged: {WORLDID_WORLDNAME_MAP[worldId]}({worldId}) - {zoneName}({zoneId})");
        }

        internal void DispatchZoneInstanceChangedEvent(DateTime utcDateTime, uint instance, string zoneName)
        {
            var eventObject = new CenturionZoneInstanceChangedObject();
            try
            {
                eventObject.timestamp = utcDateTime.ToString("s") + "Z";
                eventObject.instance = instance;
                eventObject.zoneName = zoneName;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchZoneInstanceChangedEvent: {0}", ex);
            }
            DispatchAndCacheEvent(JObject.FromObject(eventObject));
            logger.Log(LogLevel.Info, $"ZoneInstanceChanged: {zoneName} {instance}");
        }

        internal void DispatchLocationNotifiedEvent(DateTime utcDateTime, LogMessageChatType logType, string message, string pc, string zoneName, uint instance, float x, float y)
        {
            var eventObject = new CenturionLocationNotifiedObject();
            try
            {
                eventObject.timestamp = utcDateTime.ToString("s") + "Z";
                eventObject.logType = (uint)logType;
                eventObject.message = message;
                eventObject.pc = pc;
                eventObject.zoneName = zoneName;
                eventObject.instance = instance;
                eventObject.x = x;
                eventObject.y = y;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchLocationNotifiedEvent: {0}", ex);
            }
            DispatchEvent(JObject.FromObject(eventObject));
            string logMessage = instance == 0
                ? $"LocationNotified: {pc} => {zoneName}({x},{y})"
                : $"LocationNotified: {pc} => {zoneName} {instance} ({x},{y})";
            logger.Log(LogLevel.Info, logMessage);
        }

        internal void DispatchMobFAEvent(DateTime utcDateTime, string attackerName, string actionName, CenturionMob mob, string mobName)
        {
            var mobFA = new CenturionMobFAObject();
            try
            {
                mobFA.timestamp = utcDateTime.ToString("s") + "Z";
                mobFA.message = $"FirstAttack: {attackerName} {actionName} -> {mobName}({mob.Id})";
                mobFA.id = mob.Id;
                mobFA.mobId = mob.MobId;
                mobFA.mobName = mobName;
                mobFA.attacker = attackerName;
                mobFA.actionName = actionName;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchMobFAEvent: {0}", ex);
            }
            DispatchEvent(JObject.FromObject(mobFA));
            logger.Log(LogLevel.Info, mobFA.message);
        }

        internal void DispatchMobTriggerEvent(DateTime utcDateTime, string triggerType, string message, CenturionZone zone)
        {
            var trigger = new CenturionMobTriggerObject();
            try
            {
                trigger.timestamp = utcDateTime.ToString("s") + "Z";
                trigger.triggerType = triggerType;
                trigger.message = message;
                trigger.worldId = zone.WorldId;
                trigger.zoneId = zone.ZoneId;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchMobTriggerEvent: {0}", ex);
            }
            DispatchEvent(JObject.FromObject(trigger));
            logger.Log(LogLevel.Info, trigger.message);
        }

        internal void DispatchMobStateChangedEvent(DateTime utcDateTime, string state, CenturionZone zone, Combatant combatant)
        {
            var stateChanged = new CenturionMobStateChangedObject();
            try
            {
                stateChanged.timestamp = utcDateTime.ToString("s") + "Z";
                stateChanged.state = state;
                stateChanged.worldId = zone.WorldId;
                stateChanged.zoneId = zone.ZoneId;
                stateChanged.id = combatant.ID;
                stateChanged.mobId = combatant.BNpcNameID;
                stateChanged.x = combatant.PosX;
                stateChanged.y = combatant.PosY;
                stateChanged.z = combatant.PosZ;
                stateChanged.distance = (float)combatant.EffectiveDistance;
                stateChanged.hpp = (float)combatant.CurrentHP / (float)combatant.MaxHP;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "MobStateChangedEvent: {0}", ex);
            }
            DispatchEvent(JObject.FromObject(stateChanged));
        }

        internal void DispatchMobLocationEvent(DateTime utcDateTime, CenturionZone zone, Combatant combatant)
        {
            var mobLocation = new CenturionMobLocationObject();
            try
            {
                mobLocation.timestamp = utcDateTime.ToString("s") + "Z";
                mobLocation.worldId = zone.WorldId;
                mobLocation.zoneId = zone.ZoneId;
                mobLocation.id = combatant.ID;
                mobLocation.mobId = combatant.BNpcNameID;
                mobLocation.x = combatant.PosX;
                mobLocation.y = combatant.PosY;
                mobLocation.z = combatant.PosZ;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchMobLocationEvent: {0}", ex);
            }
            DispatchEvent(JObject.FromObject(mobLocation));
            logger.Log(LogLevel.Info, $"MobLocation: {zone.ZoneId} {combatant.BNpcNameID}");
        }

        private bool IsPartyMember(Combatant combatant)
        {
            // The PartyTypeEnum was renamed in 2.6.0.0 to work around that, we use reflection and cast the result to int.
            return (int)combatant.GetType().GetMethod("get_PartyType").Invoke(combatant, new object[] { }) == 1;
        }

        internal void DispatchCombantantData(DateTime utcDateTime, CenturionZone zone, IList<Combatant> combatants, bool isCrowded)
        {
            var combatantData = new CenturionCombatDataObject();
            try
            {
                var partyCombatantIds = repository.GetCombatants().Skip(1).Where(c => c.type == 1 && IsPartyMember(c)).Select(c => c.ID);

                var countPC = combatants.Count(c => c.type == 1);
                var countNPC = combatants.Count(c => c.type == 2);
                var countChocobos = combatants.Count(c => c.type == 2 && c.OwnerID != 0 && c.BNpcID == 952);
                var countPets = combatants.Count(c => c.type == 2 && c.OwnerID != 0 && c.BNpcID != 952);

                combatantData.timestamp = utcDateTime.ToString("s") + "Z";
                combatantData.self = combatants.Select(c => new CharCombatant(c)).FirstOrDefault();
                combatantData.party = combatants.Skip(1).Where(c => IsPartyMember(c)).Select(c => new CharCombatant(c)).ToList();
                combatantData.pcs = combatants.Skip(1).Where(c => c.type == 1).Select(c => new CharCombatant(c)).ToList();
                combatantData.targets = zone.Mobs.Select(c => c.AsCharCombatant()).ToList();
                combatantData.zoneId = zone.ZoneId;
                combatantData.zoneName = zone.ZoneName;
                combatantData.countPC = countPC;
                combatantData.countNPC = countNPC;
                combatantData.countChocobos = countChocobos;
                combatantData.countPets = countPets;
                combatantData.countTotal = countPC + countNPC;
                combatantData.isCrowded = isCrowded;
                // combatantData.others = combatants.Where(c => c.BNpcNameID != 0).Select(c => new PosCombatant(c)).ToList();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DispatchCombantantData: {0}", ex);
            }
            DispatchEvent(JObject.FromObject(combatantData));
        }


        internal class CenturionMob
        {
            private readonly CenturionZone zone;

            private Combatant combatant;

            private DateTime? locationWatchStartTime = null;

            private bool locationDispatched = false;

            public CenturionMob(CenturionZone zone, DateTime utcTime, Combatant combatant)
            {
                this.zone = zone;
                this.combatant = combatant;

                zone.EventSource.DispatchMobStateChangedEvent(utcTime, "Found", zone, combatant);
            }
            public uint Id { get => combatant.ID; }
            public uint MobId { get => combatant.BNpcNameID; }

            public CharCombatant AsCharCombatant() => new CharCombatant(combatant);

            public bool CanBeRemoved
            {
                get => combatant.CurrentHP == 0 || (float)combatant.EffectiveDistance > 100;
            }

            public void Update(DateTime utcTime, Combatant target)
            {
                if (target.PosX == combatant.PosX && target.PosY == combatant.PosY && target.PosZ == combatant.PosZ && target.Heading == combatant.Heading && target.TargetID == 0 && (target.CurrentHP == target.MaxHP))
                {
                    // フリーかつ前回と同じ位置
                    if (locationWatchStartTime != null)
                    {
                        TimeSpan elapsedTime = utcTime - (DateTime)locationWatchStartTime;
                        if (elapsedTime.TotalSeconds > 2 && !locationDispatched)
                        {
                            // フリーで2秒以上止まってたら発行
                            zone.EventSource.DispatchMobLocationEvent(utcTime, zone, target);
                            locationDispatched = true;
                        }
                    }
                    else
                    {
                        locationWatchStartTime = utcTime;
                    }
                }
                else
                {
                    locationWatchStartTime = null;
                    locationDispatched = false;
                }

                if (combatant.CurrentHP > 0 && target.CurrentHP == 0)
                {
                    // HPが0になった
                    zone.EventSource.DispatchMobStateChangedEvent(utcTime, "Killed", zone, target);
                }
                else if (combatant.CurrentHP == combatant.MaxHP && target.CurrentHP < target.MaxHP)
                {
                    // HPが減り始めた
                    zone.EventSource.DispatchMobStateChangedEvent(utcTime, "StartCombat", zone, target);
                }
                combatant = target;
            }

            public void Dispose(DateTime utcTime)
            {
                // 削除(親Zoneを切り替えるときに呼ばれる)
                zone.EventSource.DispatchMobStateChangedEvent(utcTime, "Lost", zone, combatant);
            }
        }

        internal class CenturionZone
        {
            public CenturionEventSource EventSource { get; }
            public uint WorldId { get; }

            public uint ZoneId { get; }

            public string ZoneName { get; }

            public uint CurrentInstance { get; set; }

            private readonly uint[] mobIds = new uint[0];

            private readonly List<CenturionMob> mobs = new List<CenturionMob>();

            private int crowdedCount = 0;

            public CenturionZone(CenturionEventSource eventSource, uint worldId, uint zoneId, string zoneName)
            {
                EventSource = eventSource;
                WorldId = worldId;
                ZoneId = zoneId;
                ZoneName = zoneName;
                CurrentInstance = 0;

                if (!ZONE_MOBIDS_MAP.TryGetValue(zoneId, out mobIds))
                {
                    mobIds = new uint[0];
                }
                EventSource.DispatchZoneChangedEvent(DateTime.UtcNow, worldId, zoneId, zoneName);
            }

            public IEnumerable<CenturionMob> Mobs { get => mobs; }

            // PC と NPC の合計数が 5 Interval 連続で 150 を超えていた
            public bool IsCrowded { get => crowdedCount >= 5; }

            public void Update(DateTime utcDateTime, IList<Combatant> allCombatants)
            {
                // 混み具合の判定
                var countTotal = allCombatants.Count(c => c.type == 1 || c.type == 2);
                crowdedCount = countTotal > 150 ? (crowdedCount + 1) : 0;

                var targetCombatants = allCombatants.Where(c => mobIds.Contains(c.BNpcNameID)).ToList();

                for (int i = mobs.Count - 1; i >= 0; i--)
                {
                    var mob = mobs[i];
                    var targetCombatant = targetCombatants.FirstOrDefault(c => c.ID == mob.Id);
                    if (targetCombatant != null)
                    {
                        // Update with latest pos
                        mob.Update(utcDateTime, targetCombatant);
                        targetCombatants.Remove(targetCombatant);
                    }
                    else if (!IsCrowded || mob.CanBeRemoved)
                    {
                        mob.Dispose(utcDateTime);
                        mobs.RemoveAt(i);
                    }
                }

                targetCombatants.Where(c => c.CurrentHP > 0).ToList().ForEach(c =>
                    mobs.Add(new CenturionMob(this, utcDateTime, c))
                );

                EventSource.DispatchCombantantData(utcDateTime, this, allCombatants, IsCrowded);
            }

            public void Dispose(DateTime utcDateTime)
            {
                mobs.ForEach(mob => mob.Dispose(utcDateTime));
            }
        }
    }
}
