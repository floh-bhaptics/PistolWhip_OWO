using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading;
using MelonLoader;
using OWOGame;

namespace MyOWOVest
{
    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();


        public TactsuitVR()
        {
            RegisterAllTactFiles();
            InitializeOWO();
        }

        private async void InitializeOWO()
        {
            LOG("Initializing suit");

            // New auth.
            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("71587680");

            OWO.Configure(gameAuth);
            string[] myIPs = getIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == ConnectionState.Connected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
            }
            if (suitDisabled) LOG("Owo is not enabled?!?!");
        }

        public string[] getIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\Mods\\" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    IPAddress address;
                    if (IPAddress.TryParse(line, out address)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }


        ~TactsuitVR()
        {
            LOG("Destructor called");
            DisconnectOwo();
        }

        private BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is not BakedSensation baked)
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
                LOG("Registered baked sensation: " + baked.name);
                result.Add(baked);
            }
            return result.ToArray();
        }

        public void DisconnectOwo()
        {
            LOG("Disconnecting Owo skin.");
            OWO.Disconnect();
        }

        public void LOG(string logStr)
        {
            MelonLogger.Msg(logStr);
        }

        void RegisterAllTactFiles()
        {

            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.ToString()); }

            }

            systemInitialized = true;
        }


        public void PlayBackHit()
        {
            PlayBackFeedback("ShotEntry", new Muscle[2] { Muscle.Pectoral_L, Muscle.Pectoral_R });
        }

        public void Recoil(bool isRightHand, bool isTwoHanded = false)
        {
            if (isTwoHanded)
            {
                var myMuscle = new Muscle[2] { Muscle.Arm_L, Muscle.Arm_R };
                PlayBackFeedback("Recoil", myMuscle);
                return;
            }
            if (isRightHand) PlayBackFeedback("Recoil", Muscle.Arm_R);
            else PlayBackFeedback("Recoil", Muscle.Arm_L);
            
        }

        public void GunReload(bool isRightHand, bool reloadHip, bool reloadShoulder, bool reloadTrigger)
        {
            if (reloadTrigger) return;
            string pattern = "Reload";
            Muscle myMuscle = Muscle.Abdominal_R;
            if (reloadHip)
            {
                if (isRightHand) myMuscle = Muscle.Abdominal_R;
                else myMuscle = Muscle.Abdominal_L;
            }
            if (reloadShoulder)
            {
                if (isRightHand) myMuscle = Muscle.Pectoral_R;
                else myMuscle = Muscle.Pectoral_L;
            }
            PlayBackFeedback(pattern, myMuscle);
        }

        public void PlayBackFeedback(string feedback)
        {
            if (FeedbackMap.ContainsKey(feedback))
            {
                OWO.Send(FeedbackMap[feedback]);
            }
            else LOG("Feedback not registered: " + feedback);
        }

        public void PlayBackFeedback(string feedback, Muscle onMuscle)
        {
            if (FeedbackMap.ContainsKey(feedback))
            {
                OWO.Send(FeedbackMap[feedback].WithMuscles(onMuscle));
            }
            else LOG("Feedback not registered: " + feedback);
        }

        public void PlayBackFeedback(string feedback, Muscle[] onMuscles)
        {
            if (FeedbackMap.ContainsKey(feedback))
            {
                OWO.Send(FeedbackMap[feedback].WithMuscles(onMuscles));
            }
            else LOG("Feedback not registered: " + feedback);
        }

    }
}
