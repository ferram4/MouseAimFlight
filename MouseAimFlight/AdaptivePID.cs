using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseAimFlight
{
    class AdaptivePID
    {
        PID pitchPID;
        PID rollPID;
        PID yawPID;

        float pitchP = 0.2f, pitchI = 0.1f, pitchD = 0.08f;
        float yawP = 0.035f, yawI = 0.1f, yawD = 0.04f;
        float rollP = 0.01f, rollI = 0.001f, rollD = 0.005f;
        float upWeighting = 3f;

        float pIntLimt = 0.2f, rIntLimit = 0.2f, yIntLimit = 0.2f; //initialize integral limits at 0.2

        float adaptationCoefficient;

        public AdaptivePID()
        {
            pitchPID = new PID(pitchP, pitchI, pitchD);
            rollPID = new PID(rollP, rollI, rollD);
            yawPID = new PID(yawP, yawI, yawD);
        }

        public Steer Simulate(float pitchError, float rollError, float yawError, UnityEngine.Vector3 angVel, float altitude, float timestep)
        {
            float steerPitch = pitchPID.Simulate(pitchError, angVel.x, pIntLimt, timestep);
            float steerRoll = rollPID.Simulate(rollError, angVel.y, rIntLimit, timestep);
            float steerYaw = yawPID.Simulate(yawError, angVel.z, yIntLimit, timestep);

            Steer steer = new Steer (steerPitch, steerRoll, steerYaw);

            return steer;
        }

        //public void DebugString(ref string debugString, string name)
        //{
        //    debugString += name + " errors:\n";
        //    debugString += "p: " + errorP.ToString("N7") + "\ti: " + errorI.ToString("N7") + "\td: " + errorD.ToString("N8") + "\n";
        //    debugString += name + " gains:\n";
        //    debugString += "p: " + kp.ToString("N7") + "\ti: " + ki.ToString("N7") + "\td: " + kd.ToString("N7") + "\n";
        //    debugString += name + " error*gains:\n";
        //    debugString += "p: " + outputP.ToString("N7") + "\ti: " + outputI.ToString("N7") + "\td: " + outputD.ToString("N8") + "\n";
        //    debugString += "Output: " + output.ToString("N7");
        //}
        
        void AdaptGains(float timeStep)
        {
            //There will be some cool code in here in the future.
        }

    }
    public struct Steer
    {
        public float pitch;
        public float roll;
        public float yaw;

        public Steer(float p, float r, float y)
        {
            pitch = p;
            roll = r;
            yaw = y;
        }
    }
}
