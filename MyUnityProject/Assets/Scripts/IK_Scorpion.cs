using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;

public class IK_Scorpion : MonoBehaviour
{
    public MyScorpionController _myController= new MyScorpionController();

    public IK_tentacles _myOctopus;

    [Header("Body")]
    float animTime;
    public float animDuration = 5;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;

    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;

    [Header("Targets")]
    public Transform[] targets;

    [Header("Cameras")]
    public GameObject primaryCamera;
    public GameObject secondaryCamera;

    int iterator = 0;
    bool isPathing;
    bool isGoingToShoot;


    // Start is called before the first frame update
    void Start()
    {
        _myController.InitLegs(legs,futureLegBases,legTargets);
        _myController.InitTail(tail);
        primaryCamera.SetActive(true);
        secondaryCamera.SetActive(false);
        isGoingToShoot = false;
    }

    // Update is called once per frame
    void Update()
    {

        if(animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();


        if (Input.GetKeyUp(KeyCode.Space))
        {
            isGoingToShoot = true;
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            isPathing = true;
        }

        if (isGoingToShoot) 
        {
            if (animTime < animDuration)
            {
                Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
            }
            else if (animTime >= animDuration && animPlaying)
            {
                Body.position = EndPos.position;
                animPlaying = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.F1))
        {
            primaryCamera.SetActive(true);
            secondaryCamera.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.F2)) 
        {
            primaryCamera.SetActive(false);
            secondaryCamera.SetActive(true);
        }

        if (isPathing) 
        {
            if (iterator < targets.Length)
            {
                Body.position = Vector3.MoveTowards(Body.position, targets[iterator].position, Time.deltaTime);
                Body.Rotate(new Vector3(0, Body.transform.position.y - targets[iterator].position.y, 0), Time.deltaTime);
                //Body.LookAt(new Vector3(targets[iterator].transform.position.x, targets[iterator].transform.position.y, targets[iterator].transform.position.z));
                if (Body.position == targets[iterator].position)
                {
                    iterator++;
                }
            }
        }
        _myController.UpdateIK();
    }

    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController.NotifyTailTarget(tailTarget);
    }

    //Trigger Function to start the walk animation
    public void NotifyStartWalk()
    {

        _myController.NotifyStartWalk();
    }
}
