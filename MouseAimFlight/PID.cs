/*
Copyright (c) 2016, BahamutoD, ferram4, tetryds
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
 
* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MouseAimFlight
{
    class PID
    {
        public float kp, ki, kd;
        float initKp, initKi, initKd;

        float outputP, outputD, outputI;


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
            float output = 0;

            integral += error * timeStep;

            if (ki != 0)
                Clamp(ref integral, integralLimit / (ki * speedFactor)); //limits outputI to integralLimit
            else
                ZeroIntegral();

            //Computing the outputs
            outputP = error * kp; //Proportional
            outputD = derivError * kd; //Derivative

            output = outputP + outputD;

            output *= speedFactor; //Speed factor

            if (output >= 1)
                ZeroIntegral();

            outputI = integral * ki * speedFactor; //Integral with speed factor

            output += outputI;

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
    }
}
