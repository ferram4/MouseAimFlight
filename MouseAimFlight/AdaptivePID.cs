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
        float outputP, outputI, outputD;
        float initKp, initKi, initKd;

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
            integral += error * timeStep;

            Clamp(ref integral, 1);

            if (updateGains)
                AdaptGains(timeStep, error);
            else
                ZeroIntegral();

            outputP = error * kp;
            outputI = integral * ki;
            outputD = derivError * kd;
            //Now we can work with the outputs
            Clamp(ref outputI, 0.2f);

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
            debugString += "p: " + outputP.ToString("N7") + "\ti: " + outputI.ToString("N7") + "\td: " + outputD.ToString("N8") + "\n";
            debugString += name + " gains:\n";
            debugString += "p: " + kp.ToString("N7") + "\ti: " + ki.ToString("N7") + "\td: " + kd.ToString("N7") + "\n";
            debugString += name + " error*gains:\n";
            debugString += "p: " + (kp * outputP).ToString("N7") + "\ti: " + (ki * outputI).ToString("N7") + "\td: " + (kd * outputD).ToString("N8");
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
