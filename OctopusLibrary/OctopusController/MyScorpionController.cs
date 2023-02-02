using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OctopusController
{
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        MyTentacleController _tail;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        //BODY
        GameObject body;
        Vector3 previousBodyPosition;
        float legsBodyOffset;

        private Vector3[] copy;
        private Vector3[] copyTail;
        private Vector3[] copyTailOffset;
        private Vector3[] backwardPos;
        private Vector3[] forwardPos;
        private float[] angles;
        private float dist = 0.05f;
        float velocityGradient = 8;
        bool stopTail = false;
        private int iterations = 4;
        float[] distances;

        //Lerp
        float[] lerpCounter;
        bool[] isLerp;


        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            legFutureBases = new Transform[LegFutureBases.Length];
            legTargets = new Transform[LegTargets.Length];

            body = GameObject.FindGameObjectWithTag("Body");

            previousBodyPosition = body.transform.position;
            

            lerpCounter = new float[_legs.Length];
            isLerp = new bool[_legs.Length];

            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                legTargets[i] = LegTargets[i];
                legFutureBases[i] = LegFutureBases[i];

                lerpCounter[i] = 0;
                isLerp[i] = false;

                if (i == 0)
                {
                    copy = new Vector3[_legs[i].Bones.Length];
                    backwardPos = new Vector3[_legs[i].Bones.Length];
                    forwardPos = new Vector3[_legs[i].Bones.Length];
                    distances = new float[_legs[i].Bones.Length];
                }
            }

            legsBodyOffset = body.transform.position.y - _legs[1].Bones[0].position.y;
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
            copyTailOffset = new Vector3[_tail.Bones.Length];
            copyTail = new Vector3[_tail.Bones.Length];
            angles = new float[_tail.Bones.Length];

            for (int i = 0; i < copyTail.Length; i++)
            {
                if (i == 0)
                {
                    copyTail[i] = new Vector3(1, 0, 0);
                }
                else
                {
                    copyTail[i] = new Vector3(0, 0, 1);
                }
                copyTailOffset[i] = _tail.Bones[i].localPosition;
                angles[i] = 0;

            }
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            updateLegs();
            updateLegPos();
            updateTail();
        }

        public void SetTailVelocity(float velocity)
        {
            velocityGradient = velocity;
            //Debug.Log(velocityGradient);
        }
        public void SetStopTail(bool stop)
        {
            stopTail = stop;
        }


        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        RaycastHit hit;
        private void updateLegPos()
        {
            for (int i = 0; i < legFutureBases.Length; i++)
            {
                float dist = Vector3.Distance(_legs[i].Bones[0].transform.position, legFutureBases[i].transform.position);
                if (dist > 0.5f) 
                {
                    Vector3 raycasPos = legFutureBases[i].position + new Vector3(0, 10, 0);
                    if (Physics.Raycast(raycasPos, new Vector3(0, -1, 0), out hit))
                    {
                        legFutureBases[i].position = hit.point;
                    }
                    isLerp[i] = true;
                }
                if (lerpCounter[i] >= 1)
                {
                    lerpCounter[i] = 0;
                    isLerp[i] = false;
                }
                if (isLerp[i] == true)
                {
                    Vector3 pos = legFutureBases[i].position;
                    _legs[i].Bones[0].position = Vector3.Lerp(_legs[i].Bones[0].transform.position, pos, lerpCounter[i]);
                    
                    lerpCounter[i] += 0.1f;
                }
            }
            UpdateBodyPosition();
        }

        private void UpdateBodyPosition()
        {
            float averageLegPosition = 0;
            for (int i = 0; i < _legs.Length; i++)
            {
                averageLegPosition += _legs[i].Bones[0].position.y;
            }

            averageLegPosition /= _legs.Length;
            averageLegPosition += legsBodyOffset;
            if (averageLegPosition > previousBodyPosition.y)
            {
                body.transform.position = new Vector3(body.transform.position.x, averageLegPosition, body.transform.position.z);
                previousBodyPosition.y = body.transform.position.y;
            }
            if(averageLegPosition < previousBodyPosition.y)
            { 
                previousBodyPosition.y = body.transform.position.y; 
            }
        }   

        //TODO: implement Gradient Descent method to move tail if necessary
        public float DistanceTarget(Vector3 target, float[] angles)
        {

            Vector3 p = ForwardKinematics(angles);
            return Vector3.Distance(p, target);
        }

        public Vector3 ForwardKinematics(float[] angles)
        {
            Vector3 lastPoint = _tail.Bones[0].transform.position;
            Quaternion rot = Quaternion.identity;
            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                rot *= Quaternion.AngleAxis(angles[i - 1], copyTail[i - 1]);
                Vector3 nextPoint = lastPoint + rot * copyTailOffset[i];

                lastPoint = nextPoint;
            }
            return lastPoint;
        }

        public float GradientDescent(Vector3 target, float[] angles, int i)
        {
            float d = DistanceTarget(target, angles);
            angles[i] += dist;
            float d2 = DistanceTarget(target, angles);

            angles[i] = d;
            float result = ((d2 - d) / dist) * velocityGradient;
            
            
            return result;
        }
        private void updateTail()
        {
            if (!stopTail)
            {
                if (Vector3.Distance(_tail.Bones[_tail.Bones.Length - 1].position, tailTarget.position) < 4.75f && DistanceTarget(tailTarget.position, angles) > 0.09f)
                {
                    for (int i = 0; i < _tail.Bones.Length; i++)
                    {
                        angles[i] -= GradientDescent(tailTarget.transform.position, angles, i);
                        if (i == 0)
                        {
                            _tail.Bones[i].localEulerAngles = new Vector3(angles[i], _tail.Bones[i].rotation.y, _tail.Bones[i].rotation.z);
                        }
                        else
                        {
                            _tail.Bones[i].localEulerAngles = new Vector3(_tail.Bones[i].rotation.x, _tail.Bones[i].rotation.y, angles[i]);
                        }
                    }
                }
            }
        }


        //TODO: implement fabrik method to move legs 
        private Vector3[] ForwardPositions(Vector3[] backwardPos, int j)
        {
            for (int i = 0; i < backwardPos.Length; i++)
            {
                if (i == 0)
                {
                    forwardPos[i] = _legs[j].Bones[i].position;
                }
                else
                {
                    Vector3 actualPos = backwardPos[i];
                    Vector3 lastPos = forwardPos[i - 1];
                    Vector3 direction = (actualPos - backwardPos[i - 1]).normalized;

                    float length = distances[i - 1];
                    forwardPos[i] = lastPos + direction * length;
                }
            }
            return forwardPos;
        }

        private Vector3[] BackwardPositions(Vector3[] forwardPos, int j)
        {
            for (int i = (forwardPos.Length - 1); i >= 0; i--)
            {
                if (i == forwardPos.Length - 1)
                {
                    backwardPos[i] = legTargets[j].position;
                }
                else
                {
                    Vector3 nextPos = backwardPos[i + 1];
                    Vector3 posActualBase = forwardPos[i];
                    Vector3 direction = (posActualBase - nextPos).normalized;

                    float length = distances[i];
                    backwardPos[i] = nextPos + direction * length;
                }
            }
            return backwardPos;
        }

        private void updateLegs()
        {
            for (int i = 0; i < _legs.Length; i++)
            {
                for (int j = 0; j < _legs[i].Bones.Length; j++)
                {
                    copy[j] = _legs[i].Bones[j].position;

                    if (j == _legs[i].Bones.Length - 1)
                    {
                        distances[j] = 0;
                    }
                    else
                    {
                        distances[j] = (_legs[i].Bones[j + 1].position - _legs[i].Bones[j].position).magnitude;
                    }
                }

                for (int j = 0; j <= iterations; j++)
                {
                    copy = BackwardPositions(ForwardPositions(copy, i), i); 
                }

                for (int j = 0; j <= _legs[i].Bones.Length - 2; j++)
                {
                    _legs[i].Bones[j].Rotate((Vector3.Cross((_legs[i].Bones[j + 1].position - _legs[i].Bones[j].position), (copy[j + 1] - copy[j])).normalized),
                        (Mathf.Acos(Vector3.Dot((_legs[i].Bones[j + 1].position - _legs[i].Bones[j].position).normalized, (copy[j + 1] - copy[j]).normalized)))
                        * Mathf.Rad2Deg, Space.World);
                }
            }
        }
    }
    #endregion
}