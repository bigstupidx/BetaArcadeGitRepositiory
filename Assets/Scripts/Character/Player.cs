﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Player : Character {

    [SerializeField]
    [Tooltip("Use the direction of the main camera as forward when moving.")]
    private bool UseCameraDir = true;

    [SerializeField]
    [Range(10.0f, 30.0f)]
    [Tooltip("The speed at which the player will rotate to align with the camera.")]
    private float RotateSpeed = 20.0f;

    [SerializeField]
    private Animator PlayerAnimator;

    [SerializeField]
    private Transform VelocityTiltTransform;

    [SerializeField]
    private float MaxVelocityTiltAngle = 20f;

    [SerializeField]
    private float VelocityTiltSpeed = 2f;

    [SerializeField]
    private bool IgnoreInput = false;

    private float mRotationThisFrame = 0f;


    //-------------------------------------------Unity Functions-------------------------------------------

    protected override void Update()
    {
        base.Update();

        // Reseting the 'mRotationThisFrame' variable.
        mRotationThisFrame = 0f;

        // Checking whether to bother getting player input.
        if (!IgnoreInput)
        {
            // Checking for user input.
            var playerMovement = new Vector2(Input.GetAxis("Horizontal"),
                                             Input.GetAxis("Vertical"));

            // Moving the player if an input was provided.
            Move(playerMovement);

            // Making the player jump if the jump button is pressed.
            if (Input.GetButtonDown("Jump"))
                Jump();

            // Reseting the push pull animation bool.
            PlayerAnimator.SetBool("PushPullActive", false);

            // Updating the animator 'DidJump' parameter.
            PlayerAnimator.SetBool("DidJump", Input.GetButtonDown("Jump"));
        }

        // Updating the animator 'IsGrounded' parameter.
        PlayerAnimator.SetBool("IsGrounded", IsGrounded());

        // Applying/reseting the players velocity tilt.
        ApplyVelocityTilt();
    }

    private void LateUpdate()
    {
        // Updating the animator 'Speed' parameter.
        var finalVelocity = mAccurateVelocity;
        finalVelocity.y = 0.0f;
        PlayerAnimator.SetFloat("Speed", finalVelocity.magnitude);
    }


    //-------------------------------------------Public Functions------------------------------------------

    public void ToggleIgnoreInput()
    {
        // Toggling the value of 'IgnoreInput'.
        SetIgnoreInput(!IgnoreInput);
    }

    public void SetIgnoreInput(bool value)
    {
        // Setting the value of 'IgnoreInput'.
        IgnoreInput = value;
    }

    public void AddKnockForce(Vector3 force)
    {
        this.GetComponent<Rigidbody>().AddForce(force);
        IgnoreInput = true;
        StartCoroutine(DelayPlayerInput());
    }


    //-----------------------------------------Protected Functions-----------------------------------------

    protected sealed override void Move(Vector2 moveDir, Transform movementSpaceTrans = null)
    {
        // Aligning the player to the camera direction if necessary.
        if (UseCameraDir && moveDir.magnitude > Mathf.Epsilon)
            AlignToCamera();

        // Setting the move direction.
        movementSpaceTrans = (UseCameraDir) ? this.transform : Camera.main.transform;

        // Calling the base version of the 'Move' function.
        base.Move(moveDir, movementSpaceTrans);

        // Aligning the player to its velocity if necessary.
        if(!UseCameraDir)
            AlignToVelocity();
    }


    //------------------------------------------Private Functions------------------------------------------

    private void AlignToCamera()
    {
        // Rotating the player to align with the current direction of the main camera.
        if (Camera.main == null) return;
        var camForwardVector = Camera.main.transform.forward.normalized;
        var targetPos = this.transform.position + camForwardVector;
        var targetRot = Quaternion.LookRotation(new Vector3(targetPos.x, this.transform.position.y, targetPos.z) - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRot, Time.deltaTime * RotateSpeed);
    }

    private void AlignToVelocity()
    {
        // Calculating the new rotation of the player.
        var targetPos = transform.position + pVelocity.normalized;
        var targetVector = new Vector3(targetPos.x, this.transform.position.y, targetPos.z) - this.transform.position;
        if (targetVector.magnitude <= 0.1f) return;
        var targetRot = Quaternion.LookRotation(targetVector);
        var newRot = Quaternion.Slerp(this.transform.rotation, targetRot, Time.deltaTime * RotateSpeed);

        // Tilting the player.
        mRotationThisFrame = newRot.eulerAngles.y - this.transform.eulerAngles.y;

        // Applying the new rotation.
        this.transform.rotation = newRot;
    }

    private IEnumerator DelayPlayerInput()
    {
        yield return new WaitForSeconds(0.5f);
        while (!IsGrounded())
            yield return new WaitForSeconds(0.1f);
        IgnoreInput = false;
    }

    private void ApplyVelocityTilt()
    {
        var currentRot = VelocityTiltTransform.eulerAngles;
        float targetAngle = Mathf.Clamp(-mRotationThisFrame * mAccurateVelocity.magnitude, -MaxVelocityTiltAngle, MaxVelocityTiltAngle);
        var targetRot = new Vector3(currentRot.x, currentRot.y, targetAngle);
        var newRot = Quaternion.Slerp(Quaternion.Euler(currentRot), Quaternion.Euler(targetRot), Time.deltaTime * VelocityTiltSpeed);
        VelocityTiltTransform.rotation = newRot;
    }
}
