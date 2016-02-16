using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseAimFlight
{
    class PID
    {
        public float kp, ki, kd;
        float outputP, outputI, outputD, output;
        float initKp, initKi, initKd;

        float errorP, errorI, errorD; //FOR DEBUGGING PURPOSES

        float adaptationCoefficient;
        float integral;

        public PID(float initKp, float initKi, float initKd)
        {
            this.initKp = initKp;
            this.initKi = initKi;
            this.initKd = initKd;
            kp = initKp;
            ki = initKi;
            kd = initKd;

            integral = 0;
        }
        public float Simulate(float error, float derivError, float integralLimit, float timeStep)
        {
            //Setup
            integral += error * timeStep;

            if (ki != 0)
                Clamp(ref integral, integralLimit / ki); //limits outputI to integralLimit
            else
                ZeroIntegral();

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
    }
}
