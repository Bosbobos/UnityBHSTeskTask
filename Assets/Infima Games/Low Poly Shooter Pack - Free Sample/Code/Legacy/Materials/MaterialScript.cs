using UnityEngine;

public class MaterialScript : MonoBehaviour, IHaveProjectileReaction
{
    public Transform[] impactPrefabs;

    public void React(Collision collision)
    {
        Instantiate(impactPrefabs[Random.Range
            (0, impactPrefabs.Length)], collision.contacts[0].point,
            Quaternion.LookRotation(collision.contacts[0].normal));
    }
}
