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

        float adaptationCoefficient;
        float integral;

        public AdaptivePID(float initKp, float initKi, float initKd)
        {
            kp = initKp;
            ki = initKi;
            kd = initKd;

            adaptationCoefficient = 0.005f;
            integral = 0;
        }

        public float Simulate(float error, float derivError, float timeStep, bool updateGains)
        {
            outputP = error;
            outputI = integral;
            outputD = derivError;

            integral += error * timeStep;

            //if (updateGains)
            //    AdaptGains(timeStep, error);

            return outputP * kp + outputI * ki + outputD * kd;
        }

        public void ClampIntegral(float limit)
        {
            if (integral > limit)
                integral = limit;
            if (integral < -limit)
                integral = -limit;
        }

        public void ZeroIntegral()
        {
            integral = 0;
        }

        void AdaptGains(float timeStep, float error)
        {
            kp += timeStep * adaptationCoefficient * outputP * error;
            if (kp <= 0)
                kp = 0;

            ki += timeStep * adaptationCoefficient * outputI * error;
            if (ki <= 0)
                ki = 0;

            kd += timeStep * adaptationCoefficient * outputD * error;
            if (kd <= 0)
                kd = 0;
        }
    }
}
