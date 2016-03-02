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

        float integral;
        public bool IntegralZeroed
        {
            get { return integral == 0; }
        }

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

        public void UpdateGains(float kp, float ki, float kd)
        {
            this.kp = kp;
            this.ki = ki;
            this.kd = kd;
        }

        public float Simulate(float error, float derivError, float integralLimit, float timeStep, float speedFactor)
        {
            //Setup
            integral += error * timeStep;

            if (ki != 0)
                Clamp(ref integral, integralLimit / ki); //limits outputI to integralLimit
            else
                ZeroIntegral();

            //Computing the outputs
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
            //-----------------------

            output *= speedFactor;
            Clamp(ref output, 1);

            return output;
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
    }
}
