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
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        private Vector3[] copy;
        private Vector3 axis;
        int iterations = 0;
        float[] distances;


        float targetRootDist = 0;

        bool done = true;


        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                copy = new Vector3[_legs[i].Bones.Length];
            }
            legFutureBases = new Transform[LegFutureBases.Length]; 
            legFutureBases = LegFutureBases;

            legTargets = new Transform[LegTargets.Length];
            legTargets = LegTargets;


            //Guardamos distancia entre huesos
            distances = new float[3];
            for (int i = 0; i < distances.Length; i++)
            {

                //a = _legs[1].Bones[i].position;
                //debug.log(a + "....");
                //b = _legs[1].Bones[i + 1].position;
                //debug.log(b);
                distances[i] = Vector3.Distance(_legs[1].Bones[i].position, _legs[1].Bones[i + 1].position);


                //debug.log(i + " : " + distances[i]);
                //debug.log(_legs[1].bones.length);
            }
            Debug.Log(_legs[1].Bones[3]);
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {

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
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            // 
            for (int i = 0; i < _legs.Length; i++) 
            {
                if (Vector3.Distance(_legs[i].Bones[0].transform.position, legFutureBases[i].transform.position) > 0.4f)
                {
                    _legs[i].Bones[0].transform.position = legFutureBases[i].transform.position;
                    //updateLegs();
                }
            }
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {

        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {

            for (int i = 0; i < _legs.Length; i++)
            {
                // copy = new Vector3[_legs[i].Bones.Length];
                for (int j = 0; j < _legs[i].Bones.Length; j++)
                {

                    copy[j] = _legs[i].Bones[j].transform.position;
                }

                //distances[0] = Vector3.Distance(copy[0], copy[1]);
                //distances[1] = Vector3.Distance(copy[1], copy[2]);
                //distances[2] = Vector3.Distance(copy[2], copy[3]);

                done = Vector3.Distance(_legs[i].Bones[_legs[i].Bones.Length - 1].position, legTargets[i].position) < 0.05f;


                if (!done)
                {

                    float targetRootDist = Vector3.Distance(copy[0], legTargets[i].position);
                    for (int k = 0; k < distances.Length; k++)
                    {

                        distances[k] = Vector3.Distance(copy[0], legTargets[i].position);
                    }

                    if (targetRootDist > distances.Sum())
                    {
                        // The target is unreachable

                    }
                    else
                    {

                        while (!done)
                        {

                            // STAGE 1: FORWARD REACHING
                            copy[copy.Length - 1] = legTargets[i].position;

                            for (int m = copy.Length - 2; m >= 0; m--)
                            {

                                Vector3 direction = Vector3.Normalize(copy[m] - copy[m + 1]);
                                copy[m] = direction * distances[m] + copy[m + 1];
                            }


                            // STAGE 2: BACKWARD REACHING
                            copy[0] = _legs[i].Bones[0].position;


                            for (int n = 1; n < copy.Length; n++)
                            {

                                Vector3 direction = Vector3.Normalize(copy[n] - copy[n - 1]);

                                copy[n] = direction * distances[n - 1] + copy[n - 1];

                            }
                            done = true;
                        }
                    }

                    Vector3 axis;
                    axis.x = 90;
                    axis.y = 0;
                    axis.z = 0;

                    for (int j = 0; j <= _legs[i].Bones.Length - 2; j++)
                    {
                        
                        Vector3 vec = _legs[i].Bones[j + 1].transform.position - _legs[i].Bones[j].transform.position;
                        Vector3 vec2 = copy[j + 1] - copy[j];

                        // Debug.Log("vec  " + vec);
                        //Debug.Log("Vec2  " + vec2);


                        float angle = Mathf.Acos(Vector3.Dot(vec.normalized, vec2.normalized));
                        Vector3 vec3 = Vector3.Cross(vec, vec2).normalized;

                        angle = angle * Mathf.Rad2Deg;  // amb aixo giran molt rapid

                        //Debug.Log("Angle  " + angle);
                        //Debug.Log("Axis  " + vec3);
                        //_legs[i].Bones[j+1].rotation = new Quaternion(0, 0, 0, 1);
                        
                        //_legs[i].Bones[j].position = copy[j];
                        
                        //_legs[i].Bones[j].LookAt(copy[j + 1]);
                        _legs[i].Bones[j].Rotate(vec3, angle, Space.Self);
                        //_legs[i].Bones[j].Rotate(vec3, Space.Self);
                    }
                    //_legs[i].Bones[_legs[i].Bones.Length - 1].position = copy[_legs[i].Bones.Length - 1];
                }
            }
        }

    }
    #endregion
}

