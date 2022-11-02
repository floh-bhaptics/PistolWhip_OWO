using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;
using OWOHaptic;
//using MyOWOSensations;

namespace MyOWOVest
{
    public class TactsuitVR
    {
        /* A class that contains the basic functions for the bhaptics Tactsuit, like:
         * - A Heartbeat function that can be turned on/off
         * - A function to read in and register all .tact patterns in the bHaptics subfolder
         * - A logging hook to output to the Melonloader log
         * - 
         * */
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        public Dictionary<String, ISensation> FeedbackMap = new Dictionary<String, ISensation>();


        /*
        //public static ISensation Explosion => new Sensation(100, 1f, 80, 100f, 500f, 0f);
        public static Sensation Explosion = Sensation.Create(100, 1f, 80, 100f, 500f, 0f);
        public static ISensation ExplosionBelly = Sensation.CreateWithMuscles(Explosion, Muscle.Lumbar_L, Muscle.Lumbar_R, Muscle.Abdominal_L, Muscle.Abdominal_R);
        //public static OWOSensationWithMuscles ExplosionBelly = new OWOSensationWithMuscles(Explosion, OWOMuscle.Abdominal_Left, OWOMuscle.Abdominal_Right, OWOMuscle.Lumbar_Left, OWOMuscle.Lumbar_Right);

        public static Sensation Healing = Sensation.Create(70, 0.5f, 65, 300f, 200f, 0f);
        public static ISensation HealingBody = Sensation.CreateWithMuscles(Healing, Muscle.AllMuscles);

        
        public static Sensation Reload1 = Sensation.Create(100, 0.3f, 50, 100f, 100f, 0f);
        public static Sensation Reload2 = Sensation.Create(100, 0.2f, 40, 0f, 100f, 0f);
        public static ISensation Reloading = Reload1.ContinueWith(Reload2);
        */

        public TactsuitVR()
        {
            LOG("Initializing suit");
            //OWO.AutoConnect();
            OWO.Connect("192.168.1.183");
            Thread.Sleep(100);
            RegisterAllTactFiles();
            if (OWO.IsConnected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
            }
            if (suitDisabled) LOG("Owo is not enabled?!?!"); 

            LOG("Starting up...");
            PlayBackFeedback("Start");
        }

        ~TactsuitVR()
        {
            LOG("Destructor called");
            DisconnectOwo();
        }

        public void DisconnectOwo()
        {
            LOG("Disconnecting Owo skin.");
            OWO.Disconnect();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
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
                    ISensation test = Sensation.FromCode(tactFileStr);
                    //bHaptics.RegisterFeedback(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.ToString()); }

            }
            
            systemInitialized = true;
        }


        public void PlayBackHit()
        {
            Sensation sensation = Sensation.ShotEntry;
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            OWO.Send(sensation, Muscle.Pectoral_L, Muscle.Pectoral_R);
        }

        public void Recoil(bool isRightHand, bool isTwoHanded = false)
        {
            if (isTwoHanded)
            {
                PlayBackFeedback("Recoil_L");
                PlayBackFeedback("Recoil_R");
                //OWO.Send(Sensation.GunRecoil, Muscle.Arm_R.WithIntensity(70), Muscle.Arm_L.WithIntensity(70));
                return;
            }
            if (isRightHand) PlayBackFeedback("Recoil_R");
            else PlayBackFeedback("Recoil_L");
        }

        public void GunReload(bool isRightHand, bool reloadHip, bool reloadShoulder, bool reloadTrigger)
        {
            if (reloadTrigger) return;
            if (!FeedbackMap.ContainsKey("Reloading")) { LOG("Reloading pattern not found!!!"); return; }
            if (reloadHip)
            {
                if (isRightHand) OWO.Send(FeedbackMap["Reloading"], Muscle.Abdominal_R);
                else OWO.Send(FeedbackMap["Reloading"], Muscle.Abdominal_L);
            }
            if (reloadShoulder)
            {
                if (isRightHand) OWO.Send(FeedbackMap["Reloading"], Muscle.Pectoral_R);
                else OWO.Send(FeedbackMap["Reloading"], Muscle.Pectoral_L);
            }
        }

        public void PlayBackFeedback(string feedback)
        {
            if (FeedbackMap.ContainsKey(feedback))
            {
                OWO.Send(FeedbackMap[feedback]);
            }
            else LOG("Feedback not registered: " + feedback);
        }

    }
}
