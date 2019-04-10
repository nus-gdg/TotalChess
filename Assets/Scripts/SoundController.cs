using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Sound controller to handle sound. This is a singleton class; put it into the scene (Load screen ideally) and access through SoundController.theController.
 * Play sound by running playSound(SoundController.clip), where clip is a AudioClip variable that can be set in Unity. Supports mute that persists across sessions.
 */
public class SoundController : MonoBehaviour {

	public static string playerPrefName = "soundMute";

	public AudioClip hurt;
	public AudioClip ding;
	public AudioClip pass;
	public AudioClip fail;

	public AudioSource theSource;

	public static SoundController theController;

	public bool muted;
	public Sprite muteSprite;
	public Sprite nonMutedSprite;

	void Awake() {
		cascadeDontDestroyOnLoad();
		SoundController.theController = this;
		theSource = gameObject.GetComponent<AudioSource>();

		if(PlayerPrefs.HasKey(playerPrefName)) {
			muted = PlayerPrefs.GetInt(playerPrefName) != 0;
		} else {
			muted = false;
			PlayerPrefs.SetInt(playerPrefName, 0);
		}

		gameObject.GetComponent<ButtonBehavior>().addCallback(this.toggleMute); 		
		updateSprite(gameObject);
	}

	public void playSound(AudioClip theSound) {
		if(!muted) {
			theSource.clip = theSound;
			theSource.Play();
		}
	}

	public void toggleMute(GameObject imageToUpdate) {
		muted = !muted;
		PlayerPrefs.SetInt(playerPrefName, muted ? 1 : 0);

		if(imageToUpdate != null) {
			updateSprite(imageToUpdate);
		}

		if(muted) {
			theSource.Stop();
		}
	}

	public void updateSprite(GameObject who) {
		who.GetComponent<SpriteRenderer>().sprite = muted ? muteSprite : nonMutedSprite;
	}

	public void cascadeDontDestroyOnLoad() {
		Transform parentTransform = gameObject.transform;
 
		// If this object doesn't have a parent then its the root transform.
		while (parentTransform.parent != null)
		{
			// Keep going up the chain.
			parentTransform = parentTransform.parent;
		}
		GameObject.DontDestroyOnLoad(parentTransform.gameObject);
	}
}