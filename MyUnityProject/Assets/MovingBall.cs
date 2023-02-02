using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OctopusController;

public class MovingBall : MonoBehaviour
{
    public IK_Scorpion myScorpion;
    
    [SerializeField]
    IK_tentacles _myOctopus;


    //Movement speed in units per second
    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float movSpeed = 5.0f;
    public Transform blueTarget;
    private Vector3 vel;
    private float velY;
    private float time;
    float gravityF = -9.8f;
    float shootForce = 0;
    float maxForce;
    float minForce;
    Rigidbody rb;
    bool decreaseForce;

    Vector3 initVelGreyLine;
    Vector3 initVelBlueLine;
    Vector3 finalVelocity;

    //UI Elements
    public Slider forceSlider;
    public Slider magnusSlider;
    public GameObject greenArrow;
    public GameObject redArrow;
    public GameObject greyArrow;
    public Text rotVelocityTxt;
    public Text descriptionAngularTxt;
    public Text goalText;
    bool hasScored;

    //Magnus Variables
    float effectForce;
    Vector3 distBtRb;
    Vector3[] positionsBlue = new Vector3[41];
    Vector3[] positionsGrey = new Vector3[41];

    //LineRenderer
    public GameObject greyLineObject;
    LineRenderer greyLineRenderer;
    public GameObject blueLineObject;
    LineRenderer blueLineRenderer;

    //Magnus Effect
    Vector3 forceMagnus;
    float effectMagnus;
    Vector3 w;
    Vector3 distMagnus;
    Vector3 radius;
    Vector3 dirTarget;
    public GameObject yellowTarget;

    //Ball Parameters
    Vector3 ballInitialPosition;
    private float rotVelocity;
    bool showInformation;

    //Gradient
    float initVelocityGradient = 8;

    void Start()
    {
        showInformation = false;
        maxForce = 10f;
        minForce = 0.2f;
        forceSlider.value = minForce;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        blueLineRenderer = blueLineObject.GetComponent<LineRenderer>();
        greyLineRenderer = greyLineObject.GetComponent<LineRenderer>();
        ballInitialPosition = this.transform.position;
        yellowTarget.transform.position = new Vector3(ballInitialPosition.x + 0.22f, ballInitialPosition.y, ballInitialPosition.z + 0.22f);
        rotVelocity = 0.0f;
        hasScored = false;
    }

    void Update()
    {
        dirTarget = (transform.position - blueTarget.position);

        if(dirTarget.x > 0.0f)
        {
            yellowTarget.transform.position = new Vector3(ballInitialPosition.x + 0.22f, ballInitialPosition.y, ballInitialPosition.z + 0.22f);
        }
        else
        {
            yellowTarget.transform.position = new Vector3(ballInitialPosition.x - 0.22f, ballInitialPosition.y, ballInitialPosition.z + 0.22f);
        }
        myScorpion._myController.NotifyTailTarget(yellowTarget.transform);

        MagnusEffectBar();
        effectMagnus = magnusSlider.value;

        StrengthBar();
        shootForce = forceSlider.value;
        SetTailVelocity();


        if (Input.GetKeyDown(KeyCode.I))
        {
            showInformation = !showInformation;
            greenArrow.SetActive(showInformation);
            redArrow.SetActive(showInformation);
            greyArrow.SetActive(showInformation);
            rotVelocityTxt.enabled = showInformation;
            descriptionAngularTxt.enabled = showInformation;
            greyLineRenderer.enabled = showInformation;
            blueLineRenderer.enabled = showInformation;
        }

        if (showInformation)
        {
            Display();
        }

        greenArrow.transform.rotation = Quaternion.LookRotation(vel);
        redArrow.transform.rotation = Quaternion.LookRotation(forceMagnus);
        greyArrow.transform.rotation = Quaternion.LookRotation(initVelBlueLine);

        if (hasScored) 
        {
            goalText.enabled = true;
        }

        //transform.rotation = Quaternion.identity;
        finalVelocity = Calculatevelocity();
        transform.rotation = new Quaternion(forceMagnus.x, forceMagnus.y, forceMagnus.z, 0);

        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        //update the position
        blueTarget.position = blueTarget.position + new Vector3(-horizontalInput * movSpeed * Time.deltaTime, verticalInput * movSpeed * Time.deltaTime, 0);

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        //Calculate and show rotation velocity
        if (effectForce >= 0.5f)
        {
            rotVelocity = Vector3.Angle(rb.velocity, Vector3.Reflect(rb.velocity, Vector3.up)) / Time.fixedDeltaTime;
        }
        else
        {
            rotVelocity = Vector3.Angle(rb.velocity, Vector3.Reflect(rb.velocity, Vector3.right)) / Time.fixedDeltaTime;
        }

        rotVelocityTxt.text = rotVelocity.ToString();

    }


    Vector3 Calculatevelocity()
    {
        if (shootForce < minForce)
        {
            shootForce = minForce;
        }
        distBtRb = blueTarget.position - rb.position;
        vel = distBtRb.normalized * (Mathf.Sqrt((new Vector3(0, 0, shootForce).magnitude) * distBtRb.magnitude * 2));
        time = distBtRb.magnitude / (Mathf.Sqrt((new Vector3(0, 0, shootForce).magnitude) * distBtRb.magnitude * 2));

        velY = Mathf.Abs(time * gravityF / 2) + vel.y;
        vel = new Vector3(vel.x, velY, vel.z);

        initVelGreyLine = (vel * -Mathf.Sign(gravityF));

        //MagnusEffect
        distMagnus = new Vector3(rb.position.x - yellowTarget.transform.position.x, rb.position.y - yellowTarget.transform.position.y, rb.position.z - yellowTarget.transform.position.z);

        radius = Vector3.Cross(distMagnus, vel);
        w = Vector3.Cross(radius, vel);
        forceMagnus = effectMagnus * (Vector3.Cross(w, vel));

        initVelBlueLine = vel + forceMagnus;

        return initVelBlueLine;
    }

    void Display()
    {
        for (int i = 0; i <= 40; i++)
        {
            float elapsedTime = (i / 40.0f) * time;
            Vector3 offsetBlue = initVelBlueLine * elapsedTime + (Vector3.up * gravityF * Mathf.Pow(elapsedTime, 2)) / 2.0f;
            Vector3 offsetGrey = initVelGreyLine * elapsedTime + (Vector3.up * gravityF * Mathf.Pow(elapsedTime, 2)) / 2.0f;
            positionsBlue[i] = rb.position + offsetBlue;
            positionsGrey[i] = rb.position + offsetGrey;
        }

        //Magnus
        blueLineRenderer.positionCount = positionsBlue.Length;
        blueLineRenderer.SetPositions(positionsBlue);

        //Default Shoot
        greyLineRenderer.positionCount = positionsGrey.Length;
        greyLineRenderer.SetPositions(positionsGrey);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _myOctopus.NotifyShoot();
        rb.isKinematic = false;
        Physics.gravity = Vector3.up * gravityF;
        rb.useGravity = true;
        rb.velocity = finalVelocity;
        SetStopTail();
    }

    public void MagnusEffectBar()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            magnusSlider.value -= 0.0000025f;
        }
        if (Input.GetKey(KeyCode.X))
        {
            magnusSlider.value += 0.0000025f;
        }
    }

    public void StrengthBar()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (shootForce >= maxForce)
            {
                decreaseForce = true;
            }
            if (decreaseForce)
            {
                forceSlider.value -= 0.035f;
                initVelocityGradient -= forceSlider.value * 0.025f;
            }
            if (!decreaseForce)
            {
                forceSlider.value += 0.035f;
                initVelocityGradient += forceSlider.value * 0.025f;
            }
            if (shootForce <= minForce)
            {
                decreaseForce = false;
            }
        }
    }
    void ResetGame()
    {
        SceneManager.LoadScene("octopus_landscape_with_ball_v2");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoalRegion"))
        {
            hasScored = true;
            Debug.Log("GOAAAAAAAAAL");
        }
    }

    public void SetTailVelocity() 
    {
        myScorpion._myController.SetTailVelocity(initVelocityGradient);
    }

    public void SetStopTail()
    {
        myScorpion._myController.SetStopTail(true);
    }

}

