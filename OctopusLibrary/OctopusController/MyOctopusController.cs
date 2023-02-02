using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController
    {

        MyTentacleController[] _tentacles = new MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];

        static float angleConstraint = 3.5f;

        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin { set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }

        [SerializeField]
        private int maxIterations = 10;
        [SerializeField]
        float rotationAngle;

        bool done = false;

        readonly float errorRange = 0.1f;

        [SerializeField]
        float _theta, _sin, _cos;

        public void TestLogging(string objectName)
        {


            Debug.Log("hello, I am initializing my Octopus Controller in object " + objectName);


        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for (int i = 0; i < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i], TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd

            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region
        }


        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot()
        {
            //TODO. what happens here?
            Debug.Log("Shoot");
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }

        #endregion

        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd()
        {
            for (int i = 0; i < _tentacles.Length; i++)
            {
                bool done = false;
                int iterations = 0;
                if (!done && iterations < maxIterations)
                {
                    _theta = 0f;
                    for (int j = _tentacles[i].Bones.Length - 1; j >= 0; j--)
                    {

                        
                        //Vector form ith joint  to the end effector
                        Vector3 r1 = _tentacles[i]._endEffectorSphere.transform.position - _tentacles[i].Bones[j].transform.position;

                        //Vector from ith joint to target
                        Vector3 r2 = _randomTargets[i].transform.position - _tentacles[i].Bones[j].transform.position;

                        _theta = Mathf.Acos(Vector3.Dot(r1.normalized, r2.normalized));

                        Vector3 axis = Vector3.Cross(r1, r2).normalized;

                        _theta *= Mathf.Rad2Deg;

                        if (_theta > angleConstraint)
                        {
                            _theta = angleConstraint;
                        }


                        Quaternion finalRotQuat = Quaternion.AngleAxis(_theta, axis) * _tentacles[i].Bones[j].transform.rotation;


                        _tentacles[i].Bones[j].transform.rotation = finalRotQuat;

                        Quaternion finalRotQuatTwist = new Quaternion(0, _tentacles[i].Bones[j].transform.localRotation.y, 0, _tentacles[i].Bones[j].transform.localRotation.w).normalized;
                        Quaternion finalRotQuatSwing = Quaternion.Inverse(finalRotQuatTwist) * _tentacles[i].Bones[j].transform.localRotation;
                        finalRotQuatSwing.Normalize();
                        Quaternion QuatSwingClamped = LimitRotationOnXAxis(finalRotQuatSwing);

                        _tentacles[i].Bones[j].transform.localRotation = QuatSwingClamped;
                    }
                    iterations++;
                }
                float dist = Vector3.Distance(_randomTargets[i].transform.position, _tentacles[i].Bones[_tentacles[i].Bones.Length - 1].transform.position);

                if (dist < errorRange)
                {
                    done = true;
                }

                else
                {
                    done = false;
                }
            }


        }

        //Revisar
        Quaternion LimitRotationOnXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y = 0f;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, -22, 22);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            angleZ = Mathf.Clamp(angleZ, -4, 4);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            return q;
        }

        #endregion
    }
}
