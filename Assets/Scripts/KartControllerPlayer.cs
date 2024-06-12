using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;

public class KartControllerPlayer : MonoBehaviour
{
   private SpawnPointManager _spawnPointManager;
   
   public Transform kartModel;
   public Transform kartNormal;
   public Rigidbody sphere;
   
   float speed, currentSpeed;
   float rotate, currentRotate;

   int driftDirection;
   float driftPower;

   [Header("Bools")]
   public bool drifting;

   [Header("Parameters")]
   public float acceleration = 30f;
   public float steering = 80f;
   public float gravity = 10f;
   public LayerMask layerMask;

   [Header("Model Parts")]
   public Transform frontWheels;
   public Transform backWheels;
   public Transform steeringWheel;

   public void Awake()
   {
      _spawnPointManager = FindObjectOfType<SpawnPointManager>();
   }

   public void AnimateKart(float input)
   {
      kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, 90 + (input * 15), kartModel.localEulerAngles.z), .2f);
      
      frontWheels.localEulerAngles = new Vector3(0, (input * 15), frontWheels.localEulerAngles.z);
      frontWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);
      backWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);
      
      steeringWheel.localEulerAngles = new Vector3(-25, 90, ((input * 45)));
   }
   
   public void Respawn()
   {
      Vector3 pos = _spawnPointManager.SelectRandomSpawnpoint();
      sphere.MovePosition(pos);
      transform.position = pos - new Vector3(0, 0.4f, 0);
   }

   void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float time = Time.timeScale == 1 ? .2f : 1;
            Time.timeScale = time;
        }

        //Follow Collider
        transform.position = sphere.transform.position - new Vector3(0, 0.4f, 0);

        //Steer
        if (Input.GetAxis("Horizontal") != 0)
        {
            //int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
            //float amount = Mathf.Abs((Input.GetAxis("Horizontal")));
            Steer(Input.GetAxis("Horizontal"));
        }

        //Drift
        if (Input.GetButtonDown("Jump") && !drifting && Input.GetAxis("Horizontal") != 0)
        {
            drifting = true;
            driftDirection = Input.GetAxis("Horizontal") > 0 ? 1 : -1;

            // foreach (ParticleSystem p in primaryParticles)
            // {
            //     p.startColor = Color.clear;
            //     p.Play();
            // }

            // kartModel.parent.DOComplete();
            // kartModel.parent.DOPunchPosition(transform.up * .2f, .3f, 5, 1);

        }

        if (drifting)
        {
            float control = (driftDirection == 1) ? ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, 0, 2) : ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, 2, 0);
            float powerControl = (driftDirection == 1) ? ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, .2f, 1) : ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, 1, .2f);
            SteerDrift(driftDirection, control);
            driftPower += powerControl;

           // ColorDrift();
        }

        if (Input.GetButtonUp("Jump") && drifting)
        {
          //  Boost();
          drifting = false;
        }

        //currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f); speed = 0f;
        //currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f); rotate = 0f;

       //Accelerate
      //   if (Input.GetButton("Fire1"))
      //       speed = acceleration;

        //float accValue = Input.GetButton("Fire1") ? 1f : 0;
        float accValue = Input.GetKey(KeyCode.W) ? 1f : 0;
        ApplyAcceleration(accValue);

        //Animations    

        //a) Kart
        if (!drifting)
        {
            kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, 90 + (Input.GetAxis("Horizontal") * 15), kartModel.localEulerAngles.z), .2f);
        }
        else
        {
            float control = (driftDirection == 1) ? ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, .5f, 2) : ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, 2, .5f);
            kartModel.parent.localRotation = Quaternion.Euler(0, Mathf.LerpAngle(kartModel.parent.localEulerAngles.y,(control * 15) * driftDirection, .2f), 0);
        }

        //b) Wheels
        frontWheels.localEulerAngles = new Vector3(0, (Input.GetAxis("Horizontal") * 15), frontWheels.localEulerAngles.z);
        frontWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude/2);
        backWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude/2);

        //c) Steering Wheel
        steeringWheel.localEulerAngles = new Vector3(-25, 90, ((Input.GetAxis("Horizontal") * 45)));

    }

   
   public void FixedUpdate()
   {
      // var steerVal = Input.GetAxis("Horizontal");
      // var speedVal = Input.GetKey(KeyCode.W) ? 1f : 0f;

      // ApplyAcceleration( speedVal);
      // Steer(steerVal);

      sphere.AddForce(-kartModel.transform.right * currentSpeed, ForceMode.Acceleration);

      //Gravity
      sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
      
      //Follow Collider
      transform.position = sphere.transform.position - new Vector3(0, 0.4f, 0);

      //Steering
      transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);
      
      Physics.Raycast(transform.position + (transform.up*.1f), Vector3.down, out RaycastHit hitOn, 1.1f,layerMask);
      Physics.Raycast(transform.position + (transform.up * .1f)   , Vector3.down, out RaycastHit hitNear, 2.0f, layerMask);

      //Normal Rotation
      kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
      kartNormal.Rotate(0, transform.eulerAngles.y, 0);
   }
   
   public void Steer(float steeringSignal)
   {
      int steerDirection = steeringSignal > 0 ? 1 : -1;
      float steeringStrength = Mathf.Abs(steeringSignal);
      
      rotate = (steering * steerDirection) * steeringStrength;
   }

    public void SteerDrift(int direction, float amount)
    {
        rotate = (steering * direction) * amount;
    }

     public void ApplyAcceleration(float input)
   {
      speed = acceleration * input;
      currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);
      speed = 0f;
      currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
      rotate = 0f;
   }

}
