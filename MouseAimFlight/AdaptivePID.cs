using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseAimFlight
{
    class AdaptivePID
    {
        public float kp, ki, kd;
        float outputP, outputI, outputD, output;
        float initKp, initKi, initKd;

        float errorP, errorI, errorD; //FOR DEBUGGING PURPOSES

        float adaptationCoefficient;
        float integral;

        public AdaptivePID(float initKp, float initKi, float initKd)
        {
            this.initKp = initKp;
            this.initKi = initKi;
            this.initKd = initKd;
            kp = initKp;
            ki = initKi;
            kd = initKd;

            adaptationCoefficient = 0.005f;
            integral = 0;
        }

        public float Simulate(float error, float derivError, float timeStep, bool updateGains)
        {
            //Setup
            integral += error * timeStep;

            Clamp(ref integral, 0.2f/ki); //limits outputI to 0.2

            if (updateGains)
                AdaptGains(timeStep, error);

            //Working with the outputs
            outputP = error * kp;
            if (outputP >= 1)
                ZeroIntegral();
            outputI = integral * ki;
            outputD = derivError * kd;

            //Set values for debugging - avoid using them anywhere else
            errorP = error;
            errorI = integral;
            errorD = derivError;
            output = outputP + outputI + outputD;
            Clamp(ref output, 1);
            //-----------------------

            return outputP + outputI + outputD;
        }

        public void Clamp(ref float value, float limit)
        {
            if (value > limit)
                value = limit;
            if (value < -limit)
                value = -limit;
        }

        public void ZeroIntegral()
        {
            integral = 0;
        }

        public void DebugString(ref string debugString, string name)
        {
            debugString += name + " errors:\n";
            debugString += "p: " + errorP.ToString("N7") + "\ti: " + errorI.ToString("N7") + "\td: " + errorD.ToString("N8") + "\n";
            debugString += name + " gains:\n";
            debugString += "p: " + kp.ToString("N7") + "\ti: " + ki.ToString("N7") + "\td: " + kd.ToString("N7") + "\n";
            debugString += name + " error*gains:\n";
            debugString += "p: " + outputP.ToString("N7") + "\ti: " + outputI.ToString("N7") + "\td: " + outputD.ToString("N8") + "\n";
            debugString += "Output: " + output.ToString("N7");
        }

        void AdaptGains(float timeStep, float error)
        {
            kp = initKp;
            if (kp <= 0)
                kp = 0;

            ki = initKi;
            if (ki <= 0)
                ki = 0;

            kd = initKd * (0.5f + 0.1f / (Math.Abs(error) + 0.2f));
            if (kd <= 0)
                kd = 0;
        }
    }
}
