﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : Controller
{
    [SerializeField]
    private CharacterType characterType;
    public CharacterType CharacterType { get { return characterType; } }

    public BodyPart[] BodyParts { get; private set; }

    public Transform Parent { get; private set; }

    [SerializeField]
    private Rigidbody[] spines;
    public Rigidbody[] Spines { get { return spines; } }


    private void Start()
    {
        Parent = transform.parent;
        BodyParts = GetComponentsInChildren<BodyPart>();
    }

    public void ResetParent()
    {
        transform.parent = Parent;
    }

    public BodyPart GetBodyPart(BodyPartType bodyPartType)
    {
        foreach(var bodyPart in BodyParts)
        {
            if (bodyPart.ItemSlotType == bodyPartType)
            {
                return bodyPart;
            }
        }

        throw new System.Exception("Missing " + bodyPartType.ToString() + " on " + name);
    }
}