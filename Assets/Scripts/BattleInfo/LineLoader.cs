using UnityEngine;
using System.Collections;
//播放line上的效果用的class
public class LineLoader : MonoBehaviour {
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
