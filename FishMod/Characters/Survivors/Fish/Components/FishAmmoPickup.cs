using FishMod.Survivors.Fish;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace FishMod.Characters.Survivors.Fish.Components
{
    public class FishAmmoPickup : MonoBehaviour
    {
        public GameObject baseObject;
        public TeamFilter teamFilter;
        public GameObject pickupEffect;
        private bool alive = true;

        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && alive)
            {
                if (TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex)
                {
                    CharacterBody body = other.GetComponent<CharacterBody>();
                    if (BodyCatalog.FindBodyIndex(body) == BodyCatalog.FindBodyIndex(FishSurvivor.instance.bodyPrefab))
                    {
                        if (body.TryGetComponent(out FishWeaponController fwc))
                        {
                            Debug.Log("FishAmmoPickup.OnTriggerStay : Applying ammopack");
                            alive = false;
                            fwc.GiveAmmoPack();
                            EffectManager.SimpleEffect(pickupEffect, transform.position, Quaternion.identity, true);
                            Destroy(baseObject);
                        }
                    }
                }
            }
        }
    }
}
