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
            outputP = error;
            outputI = integral;
            outputD = derivError;

            integral += error * timeStep;

            Clamp(ref integral, 1 / (ki*5));

            if (updateGains)
                AdaptGains(timeStep, error);
            else
                ZeroIntegral();

            return outputP * kp + outputI * ki + outputD * kd;
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
