using UnityEngine;
using System.Collections;

public class LineController : MonoBehaviour {
    private ParticleSystem hitParticle;
    private Transform hitParticleObject;
    // Use this for initialization
    void Awake () {
        hitParticleObject = transform.FindChild("ParticleExplosion");
        hitParticle = hitParticleObject.GetComponent<ParticleSystem>();

    }

    public void playHitParticle(float x){
        Vector3 hitParticlePosition = new Vector3(x,
        transform.position.y,0);
        hitParticleObject.position = hitParticlePosition;
        hitParticle.Play();
    }
}
