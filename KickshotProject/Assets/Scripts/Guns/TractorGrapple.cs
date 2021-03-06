﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorGrapple: GunBase {
    public Transform gunBarrelFront;
    private LineRenderer linerender;
    private bool hitSomething = false;
    private Transform hitPosition;
    private float hitDist;
    private float fade = 0f;
    public float fadeTime = 1f;
    public float range = 12f;
    private Vector3 missStart;
    private Vector3 missEnd;
    private AudioSource shotSound;
    private float saveMaxAirSpeed;
    void Start() {
        // Copy a transform for use.
        hitPosition = Transform.Instantiate (gunBarrelFront);
        linerender = GetComponent<LineRenderer> ();
        linerender.enabled = false;
        shotSound = GetComponent<AudioSource> ();
        //saveMaxAirSpeed = player.maxSpeed;
    }
    override public void OnEquip(GameObject Player) {
        base.OnEquip (Player);
        saveMaxAirSpeed = player.maxSpeed;
    }
    override public void OnUnequip(GameObject Player) {
        base.OnUnequip (Player);
        player.maxSpeed = saveMaxAirSpeed;
    }
    override public void Update() {
        base.Update ();
        if (!equipped) {
            return;
        }
        transform.rotation = view.rotation;
        if (hitSomething) {
            // Keep us busy so we don't reload during grappling.
            busy = 1f;
            //player.transform.position = hitPosition.position - player.view.forward * hitDist;
            Vector3 desiredPosition = hitPosition.position - view.forward * hitDist;
            player.velocity = (desiredPosition - player.transform.position) / Time.deltaTime;
            linerender.SetPosition (0, gunBarrelFront.position);
            linerender.SetPosition (1, hitPosition.position);
            fade = fadeTime;
            missStart = gunBarrelFront.position;
            missEnd = hitPosition.position;
        } else {
            player.maxSpeed = saveMaxAirSpeed;
            if (fade > 0) {
                linerender.SetPosition (0, missStart);
                linerender.SetPosition (1, missEnd);
                fade -= Time.deltaTime;
            } else {
                linerender.enabled = false;
            }
        }
    }
    public override void OnPrimaryFireRelease() {
        player.maxSpeed = saveMaxAirSpeed;
        hitSomething = false;
    }
    public override void OnPrimaryFire() {
        RaycastHit hit;
        // We ignore player collisions.
        if (Physics.Raycast (view.position, view.forward, out hit, range, ~(1 << LayerMask.NameToLayer ("Player")))) {
            hitPosition.SetParent (hit.collider.transform);
            hitPosition.position = hit.point;
            hitSomething = true;
            hitDist = hit.distance;
            linerender.SetPosition (0, gunBarrelFront.position);
            linerender.SetPosition (1, hit.point);
            player.maxSpeed = 1000f;
        } else {
            hitSomething = false;
            fade = fadeTime;
            missStart = gunBarrelFront.position;
            missEnd = view.position + view.forward*range;
            linerender.SetPosition (0, missStart);
            linerender.SetPosition (1, missEnd);
        }
        linerender.enabled = true;
        shotSound.Play ();
    }
}
