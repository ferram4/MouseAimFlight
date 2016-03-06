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
using System.Threading.Tasks;

namespace MouseAimFlight
{
    class AdaptivePID
    {
        public PID pitchPID;
        public PID rollPID;
        public PID yawPID;

        float pitchP = 0.2f, pitchI = 0.1f, pitchD = 0.08f;
        float rollP = 0.01f, rollI = 0.001f, rollD = 0.005f;
        float yawP = 0.035f, yawI = 0.1f, yawD = 0.04f;
        float upWeighting = 3f; //TODO: update external upweighting

        float pIntLimt = 0.2f, rIntLimit = 0.2f, yIntLimit = 0.2f; //initialize integral limits at 0.2

        public AdaptivePID()
        {
            pitchPID = new PID(pitchP, pitchI, pitchD);
            rollPID = new PID(rollP, rollI, rollD);
            yawPID = new PID(yawP, yawI, yawD);
        }

        //The constructor below is an abomination and will be nuked as soon as the GUI as it is gets removed.
        public AdaptivePID(float pP, float pI, float pD, float rP, float rI, float rD, float yP, float yI, float yD)
        {
            pitchPID = new PID(pP, pI, pD);
            rollPID = new PID(rP, rI, rD);
            yawPID = new PID(yP, yI, yD);
        }

        public float UpWeighting(float terrainAltitude, float dynPress, float velocity)
        {
            if (terrainAltitude < 50)
                return (10 - 0.18f * terrainAltitude) * upWeighting;

            return upWeighting;
        }

        public Steer Simulate(float pitchError, float rollError, float yawError, UnityEngine.Vector3 angVel, float terrainAltitude, float timestep, float dynPress, float vel)
        {
            float speedFactor = vel / dynPress / 16; //More work needs to be done to sanitize speedFactor

            if (speedFactor > 2)
                speedFactor = 2;

            float trimFactor = (float)Math.Sqrt(speedFactor);

            float steerPitch = pitchPID.Simulate(pitchError, angVel.x, pIntLimt * trimFactor, timestep, speedFactor);
            float steerRoll = rollPID.Simulate(rollError, angVel.y, rIntLimit, timestep, speedFactor);
            if (pitchPID.IntegralZeroed)        //yaw integrals should be zeroed at the same time that pitch PIDs are zeroed, because that happens in large turns
                yawPID.ZeroIntegral();
            float steerYaw = yawPID.Simulate(yawError, angVel.z, yIntLimit, timestep, speedFactor);

            Steer steer = new Steer (steerPitch, steerRoll, steerYaw);

            return steer;
        }
        
        void AdaptGains(float timeStep, float speedFactor)
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
