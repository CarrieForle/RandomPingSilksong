using BepInEx;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace RandomPing;

[BepInAutoPlugin(id: "io.github.carrieforle.randomping")]
public partial class RandomPingPlugin : BaseUnityPlugin
{
	private AudioClip? pingAudioClip = null;
	private AudioSource pingAudioSource;
	private GameObject go;
	private const float volume = .4f;

	private void Awake()
	{
		go = new GameObject("RandomPing");
		DontDestroyOnLoad(go);
		pingAudioSource = go.AddComponent<AudioSource>();
		pingAudioSource.volume = volume;
	}

	private IEnumerator Start()
	{
		yield return StartCoroutine(GetAudioFile());
		yield return StartCoroutine(RandomlyPing());
	}

	private IEnumerator GetAudioFile()
	{
		var targetAudioFile = Path.Combine(Path.GetDirectoryName(Info.Location), "assets", "ping.mp3");

		using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(new Uri(targetAudioFile), AudioType.MPEG);

		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Logger.LogError($"Failed to read audio at \"{targetAudioFile}\": {www.error}");
		}
		else
		{
			pingAudioClip = DownloadHandlerAudioClip.GetContent(www);
			if (pingAudioClip == null)
			{
				Logger.LogError($"Failed to read audio at \"{targetAudioFile}\"");
			}
			else
			{
				pingAudioClip.LoadAudioData();
				pingAudioSource.clip = pingAudioClip;
			}
		}
	}

	private IEnumerator RandomlyPing()
	{
		if (pingAudioClip == null)
		{
			yield break;
		}

		while (true)
		{
			var waitSec = UnityEngine.Random.Range(1 * 60, 45 * 60);
			var playTwice = UnityEngine.Random.RandomRangeInt(0, 5) == 0;

			Logger.LogDebug($"Next ping is scheduled in the next {waitSec:F0} seconds (playTwice: {playTwice})");
			yield return new WaitForSecondsRealtime(waitSec);
			pingAudioSource.PlayOneShot(pingAudioClip, volume);

			if (playTwice)
			{
				pingAudioSource.PlayDelayed(UnityEngine.Random.Range(2, 5));
			}
		}
	}
}
