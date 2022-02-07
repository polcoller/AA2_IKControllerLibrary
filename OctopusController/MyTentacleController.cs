using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{

    
    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        public Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones
            tentacleMode = mode;

            switch (tentacleMode){
                case TentacleMode.LEG:
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    _bones = new Transform[4];
                    root = root.GetChild(0);

                    for (int i = 0; i < 3; i++)
                    {
                        _bones[i] = root;
                        root = root.GetChild(1);
                    }
                    _bones[3] = root;
                    _endEffectorSphere = root;

                    break;
                case TentacleMode.TAIL:
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 
                    break;
                case TentacleMode.TENTACLE:
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector
                    _bones = new Transform[50];
                    root = root.GetChild(0);

                    for (int i = 0; i < 50; i++) 
                    {
                        _bones[i] = root;
                        root = root.GetChild(0);
                    }

                    _endEffectorSphere = root;
                    break;
            }
            return Bones;
        }
    }
}
