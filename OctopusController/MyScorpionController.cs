﻿using System;
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
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        private Vector3[] copy;
        private Vector3[] copyTail;
        private Vector3[] copyTailOffset;
        private Vector3[] backwardPos;
        private Vector3[] forwardPos;
        private float[] angles;
        private float offset = 0.33f;
        private float dist = 0.05f;
        private int velocityGradient = 18;
        private int iterations = 4;
        float[] distances;


        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            legFutureBases = new Transform[LegFutureBases.Length];
            legTargets = new Transform[LegTargets.Length];
            //Legs init
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                legTargets[i] = LegTargets[i];
                legFutureBases[i] = LegFutureBases[i];
                
            }
            copy = new Vector3[_legs[0].Bones.Length];
            backwardPos = new Vector3[_legs[0].Bones.Length];
            forwardPos = new Vector3[_legs[0].Bones.Length];

            //Guardamos distancia entre huesos
            distances = new float[_legs[0].Bones.Length];
            
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
            copyTail = new Vector3[_tail.Bones.Length];
            copyTailOffset = new Vector3[_tail.Bones.Length];
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
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            for (int i = 0; i < _legs.Length; i++) 
            {
                if (Vector3.Distance(_legs[i].Bones[0].transform.position, legFutureBases[i].transform.position) > 1f)
                {
                    _legs[i].Bones[0].transform.position = legFutureBases[i].transform.position;
                    
                }
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
                Vector3 nextPoint = lastPoint + rot * copyTailOffset[i] * offset;

                lastPoint = nextPoint;
            }
            return lastPoint;
        }

        public float PartialGradient(Vector3 target, float[] angles, int i)
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
            if (Vector3.Distance(_tail.Bones[_tail.Bones.Length - 1].position, tailTarget.position) < 4.75f && DistanceTarget(tailTarget.position, angles) > 0.55f)
            {
                for (int i = 0; i < _tail.Bones.Length; i++)
                {
                    angles[i] -= PartialGradient(tailTarget.transform.position, angles, i);
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

                for (int j = 0; j < iterations; j++)
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

