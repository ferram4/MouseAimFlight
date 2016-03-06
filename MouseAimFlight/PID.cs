/*
 * Copyright (c) 2016 BahamutoD, ferram4, tetryds
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
*/

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
