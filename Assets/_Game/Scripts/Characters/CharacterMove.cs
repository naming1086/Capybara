﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationState
{
    Run,
    Idle,
    Walk
}

public class CharacterMove : MonoBehaviour
{
    [SerializeField]
    private Transform[] bones;
    public Transform[] Bones { get { return bones; } }

    [SerializeField]
    private Transform rootBone;

    [Space()]
    [SerializeField]
    private AnimationKeyFrame walkAnimation;
    [SerializeField]
    private AnimationKeyFrame runAnimation;
    [SerializeField]
    private AnimationKeyFrame idleAnimation;

    [SerializeField]
    private float transitionDuration = 0.5f;

    [Space()]
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float rotationSpeed = 10;
    [SerializeField]
    private bool faceMoveDirection = true;

    private AnimationKeyFrame previousAnimation;
    private AnimationKeyFrame currentAnimation;

    private float normalizedTime = 0;
    private float timer = 0;

    private float animationDurationMultiplier; //this is modified based on input vector

    private bool transitioning = false;

    private Vector3 originalRootBonePosition;
    private Vector3 moveVector;

    private AnimationKeyFrame.Frame[] previousFrames;
    private Vector3 previousRootBonePosition;

    protected Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        originalRootBonePosition = rootBone.localPosition;

        SetIdle();
    }

    protected virtual void LateUpdate()
    {
        if (currentAnimation)
        {
            //get time
            timer += Time.deltaTime;
            normalizedTime = timer / (transitioning ? transitionDuration : currentAnimation.Duration / animationDurationMultiplier);

            if (transitioning && normalizedTime > 0.99f)
            {
                transitioning = false;
                timer = 0;
                normalizedTime = timer / currentAnimation.Duration;
            }

            float increment = 1 / (float)(currentAnimation.AnimationFrames.Length);
            float counterFloat = ((currentAnimation.AnimationFrames.Length) * normalizedTime);
            counterFloat = Mathf.Clamp(counterFloat, 0, currentAnimation.AnimationFrames.Length);

            int counter = Mathf.CeilToInt(counterFloat);
            if (counter == 0)
            {
                counter = 1;
            }

            float removalIncrement = increment * (counter - 1);

            float newValue = normalizedTime - removalIncrement;
            float offset = (newValue * 100) / increment;
            offset /= 100;

            if (normalizedTime > 0.99f)
            {
                timer = 0;
            }

            for (int i = 0; i < currentAnimation.CharacterMove.Bones.Length; i++)
            {
                int firstCounter = counter - 1;
                int secondCounter = counter >= currentAnimation.AnimationFrames.Length ? 0 : counter;

                //transitioning
                if (transitioning && previousAnimation != null)
                {
                    PosAndRot from = currentAnimation.GetBonePosAndRot(0, i, 0);

                    currentAnimation.CharacterMove.Bones[i].localPosition = Vector3.Lerp(previousFrames[i].Position, from.Position, normalizedTime);
                    currentAnimation.CharacterMove.Bones[i].localRotation = Quaternion.Lerp(previousFrames[i].Rotation, from.Rotation, normalizedTime);
                }
                else //normal
                {
                    PosAndRot from = currentAnimation.GetBonePosAndRot(firstCounter, i, normalizedTime);
                    PosAndRot target = currentAnimation.GetBonePosAndRot(secondCounter, i, normalizedTime);

                    currentAnimation.CharacterMove.Bones[i].localPosition = Vector3.Lerp(from.Position, target.Position, offset);
                    currentAnimation.CharacterMove.Bones[i].localRotation = Quaternion.Lerp(from.Rotation, target.Rotation, offset);
                }
            }

            animationDurationMultiplier = moveVector.magnitude;

            //move if in move animation
            if (currentAnimation.AnimState == AnimationState.Run)
            {
                rigidbody.velocity = new Vector3(moveVector.x * moveSpeed, rigidbody.velocity.y, moveVector.z * moveSpeed);
                RotateTowardsVelocity();
            }
            else if (currentAnimation.AnimState == AnimationState.Walk)
            {
                rigidbody.velocity = new Vector3(moveVector.x * (moveSpeed / 2), rigidbody.velocity.y, moveVector.z * moveSpeed);
                RotateTowardsVelocity();
            }
            else if (currentAnimation.AnimState == AnimationState.Idle)
            {
                rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
            }

            //modify height of spine
            if (transitioning)
            {
                Vector3 targetPosition = new Vector3(originalRootBonePosition.x, originalRootBonePosition.y + currentAnimation.HeightCurve.Evaluate(0), originalRootBonePosition.z);
                rootBone.localPosition = Vector3.Lerp(previousRootBonePosition, targetPosition, normalizedTime);
            }
            else
            {
                float heightCurve = currentAnimation.HeightCurve.Evaluate(normalizedTime);
                rootBone.localPosition = new Vector3(originalRootBonePosition.x, originalRootBonePosition.y + heightCurve, originalRootBonePosition.z);
            }
        }
    }

    private void RotateTowardsVelocity()
    {
        if (faceMoveDirection)
        {
            Quaternion toRotation = Quaternion.LookRotation(rigidbody.velocity, transform.up);

            Quaternion rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);

            transform.rotation = rotation;
        }
    }

    private void SetAnimation(AnimationKeyFrame anim)
    {
        moveVector = Vector3.zero;

        if (currentAnimation == anim)
        {
            return;
        }

        previousAnimation = currentAnimation;
        if (previousAnimation != null)
        {
            previousFrames = new AnimationKeyFrame.Frame[previousAnimation.CharacterMove.Bones.Length];
            for (int i = 0; i < previousFrames.Length; i++)
            {
                previousFrames[i].Bone = previousAnimation.CharacterMove.Bones[i];
                previousFrames[i].Position = previousAnimation.CharacterMove.Bones[i].localPosition;
                previousFrames[i].Rotation = previousAnimation.CharacterMove.Bones[i].localRotation;
            }
        }

        previousRootBonePosition = rootBone.localPosition;
        currentAnimation = anim;
        transitioning = true;
        timer = 0;
    }

    protected void SetRunning(Vector3 moveVector)
    {
        SetAnimation(runAnimation);
        this.moveVector = moveVector;
    }

    protected void SetWalking(Vector3 moveVector)
    {
        SetAnimation(walkAnimation);
        this.moveVector = moveVector;
    }

    protected void SetIdle()
    {
        SetAnimation(idleAnimation);
    }
}