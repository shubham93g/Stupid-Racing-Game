using UnityEngine;
using System.Collections;

public class DestroyAfterAnimation : MonoBehaviour {

	public float SecondsBeforeDestroying;

	private bool called = false;

	void Update() {
		if (!called) {
			called = true;
			StartCoroutine(DestroyAfterAnimationPlayed());
		}
	}

	private IEnumerator DestroyAfterAnimationPlayed() {
		yield return new WaitForSeconds (SecondsBeforeDestroying);		
		Destroy (gameObject);
	}
}
