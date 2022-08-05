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
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);

        public static IOWOSensation Explosion => new OWOSensation(100, 1f, 80, 100f, 500f, 0f);
        public static OWOSensationWithMuscles ExplosionBelly = new OWOSensationWithMuscles(Explosion, OWOMuscle.Abdominal_Left, OWOMuscle.Abdominal_Right, OWOMuscle.Lumbar_Left, OWOMuscle.Lumbar_Right);

        public static IOWOSensation Healing => new OWOSensation(70, 0.5f, 65, 300f, 200f, 0f);
        public static OWOSensationWithMuscles Healing1 = new OWOSensationWithMuscles(Healing, OWOMuscle.Abdominal_Left, OWOMuscle.Dorsal_Right);
        public static OWOSensationWithMuscles Healing2 = new OWOSensationWithMuscles(Healing, OWOMuscle.Abdominal_Right, OWOMuscle.Dorsal_Left);
        public static OWOSensationWithMuscles Healing3 = new OWOSensationWithMuscles(Healing, OWOMuscle.Lumbar_Right, OWOMuscle.Pectoral_Left);
        public static OWOSensationWithMuscles Healing4 = new OWOSensationWithMuscles(Healing, OWOMuscle.Lumbar_Left, OWOMuscle.Pectoral_Right);
        public static OWOSensationsChain HealingBody = new OWOSensationsChain(Healing1, Healing2, Healing3, Healing4);

        public static IOWOSensation Reload1 => new OWOSensation(100, 0.3f, 50, 100f, 100f, 0f);
        public static IOWOSensation Reload2 => new OWOSensation(100, 0.2f, 40, 0f, 100f, 0f);
        public static OWOSensationsChain Reloading = new OWOSensationsChain(Reload1, Reload2);

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                OWO.Send(OWOSensation.HeartBeat, OWOMuscle.Pectoral_Left);
                Thread.Sleep(600);
            }
        }

        public TactsuitVR()
        {
            LOG("Initializing suit");
            OWO.AutoConnect();
            //OWO.Connect("192.168.1.248");
            Thread.Sleep(800);
            if (OWO.IsConnected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
            }
            if (suitDisabled) LOG("Owo is not enabled?!?!"); 

            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
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



        public void PlayBackHit()
        {
            OWOSensation sensation = OWOSensation.ShotEntry;
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            OWO.Send(sensation, OWOMuscle.Pectoral_Left, OWOMuscle.Pectoral_Right);
        }

        public void Recoil(bool isRightHand, bool isTwoHanded = false)
        {
            if (isTwoHanded)
            {
                OWO.Send(OWOSensation.GunRecoil, OWOMuscle.Arm_Right, OWOMuscle.Arm_Left);
                return;
            }
            if (isRightHand) OWO.Send(OWOSensation.GunRecoil, OWOMuscle.Arm_Right);
            else OWO.Send(OWOSensation.GunRecoil, OWOMuscle.Arm_Left);
        }

        public void GunReload(bool isRightHand, bool reloadHip, bool reloadShoulder, bool reloadTrigger)
        {
            if (reloadTrigger) return;
            if (reloadHip)
            {
                if (isRightHand) OWO.Send(Reloading, OWOMuscle.Abdominal_Right);
                else OWO.Send(Reloading, OWOMuscle.Abdominal_Left);
            }
            if (reloadShoulder)
            {
                if (isRightHand) OWO.Send(Reloading, OWOMuscle.Pectoral_Right);
                else OWO.Send(Reloading, OWOMuscle.Pectoral_Left);
            }
        }

        public void PlayHeal()
        {
            OWO.Send(HealingBody);
        }

        public void PlayExplosion()
        {
            OWO.Send(ExplosionBelly);
        }

        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StopThreads()
        {
            // Yes, looks silly here, but if you have several threads like this, this is
            // very useful when the player dies or starts a new level
            StopHeartBeat();
        }


    }
}
