using UnityEngine;
using System.Collections;

/*
 * Music controller to handle music. This is a singleton class; put it into the scene (Load screen ideally) and access through MusicController.theController.
 * Play music by running playMusic(MusicController.clip), where clip is a AudioClip variable that can be set in Unity. Alternatively, setup a generic 
 * AudioClip[] array and you can choose to play a random music track there if you'd like. Supports mute that persists across sessions.
 */
public class MusicController : MonoBehaviour 
{
	public static string playerPrefName = "musicMute";

	public AudioClip[] genericMusic;

	public AudioSource theSource;

	public static MusicController theController;

	public bool muted;
	public Sprite muteSprite;
	public Sprite nonMutedSprite;

	void Awake()
	{
		cascadeDontDestroyOnLoad();
		MusicController.theController = this;
		theSource = gameObject.GetComponent<AudioSource>();

		if(PlayerPrefs.HasKey(playerPrefName)) {
			muted = PlayerPrefs.GetInt(playerPrefName) != 0;
		} else {
			muted = false;
			PlayerPrefs.SetInt(playerPrefName, 0);
		}

		gameObject.GetComponent<ButtonBehavior>().addCallback(this.toggleMute);
		updateSprite(gameObject);

		//playRandomMusic();
	}

	public void playMusic(AudioClip music, float volume = 1.0f)
	{
		if(!muted) {
			theSource.Stop();
			theSource.clip = music;
			theSource.Play();
		}
	}

	public void playRandomMusic(float volume = 1.0f) {
		int randomNumber = Random.Range(0, genericMusic.Length);
		Debug.Log(genericMusic.Length + " - " + randomNumber);
		playMusic(genericMusic[randomNumber], volume);
	}

	public void toggleMute(GameObject imageToUpdate) {
		muted = !muted;
		PlayerPrefs.SetInt(playerPrefName, muted ? 1 : 0);

		if(imageToUpdate != null) {
			updateSprite(imageToUpdate);
		}

		if(muted) {
			theSource.Stop();
		} else {
			playRandomMusic();
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