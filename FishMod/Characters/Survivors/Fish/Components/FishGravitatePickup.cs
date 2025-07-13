using System;
using System.Collections.Generic;
using System.Text;
using FishMod.Survivors.Fish;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace FishMod.Characters.Survivors.Fish.Components
{
    public class FishGravitatePickup : MonoBehaviour
    {
        public Transform gravitateTarget = null;
        public new Rigidbody rigidbody;
        public TeamFilter teamFilter;

        public float acceleration;
        public float maxSpeed;

        public void OnTriggerEnter(Collider other)
        {
            if (NetworkServer.active)
            {
                StartGravitate(other);
            }
        }
        public void FixedUpdate()
        {
            if (gravitateTarget)
            {
                rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, (gravitateTarget.transform.position - transform.position).normalized * maxSpeed, acceleration);
            }
        }

        private void StartGravitate(Collider other)
        {
            if (!gravitateTarget && teamFilter.teamIndex != TeamIndex.None)
            {
                if (other.gameObject.TryGetComponent(out HealthComponent hc))
                {
                    if (hc.body && BodyCatalog.FindBodyIndex(hc.body) == BodyCatalog.FindBodyIndex(FishSurvivor.instance.bodyPrefab))
                    {
                        if (!hc.body.TryGetComponent(out FishWeaponController fwc) || fwc.MaxAmmo())
                        {
                            return;
                        }

                        if (TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex)
                        {
                            gravitateTarget = other.gameObject.transform;
                        }
                    }
                }
            }
        }

        public void ForceGravitate(Collider targetCollider)
        {
            if (targetCollider)
            {
                StartGravitate(targetCollider);
            }
        }
    }
}
