using BoundfoxStudios.CommunityProject.Infrastructure.Events.ScriptableObjects;
using BoundfoxStudios.CommunityProject.Systems.AudioSystem.ScriptableObjects;
using UnityEngine;

namespace BoundfoxStudios.CommunityProject.Systems.AudioSystem
{
	[AddComponentMenu(Constants.MenuNames.Audio + "/" + nameof(AudioCuePlayer))]
	public class AudioCuePlayer : MonoBehaviour
	{
		[field: SerializeField]
		private AudioCueEventChannelSO AudioCueEventChannel { get; set; } = default!;

		[field: SerializeField]
		private SoundEmitter SoundEmitterPrefab { get; set; } = default!;

		private void OnEnable()
		{
			AudioCueEventChannel.Raised += Play;
		}

		private void OnDisable()
		{
			AudioCueEventChannel.Raised -= Play;
		}

		private void Play(AudioCueSO audioCue)
		{
			var audioClip = audioCue.AudioClip;

			var emitter = Instantiate(SoundEmitterPrefab, Vector3.zero, Quaternion.identity);

			emitter.PlayAudioClip(audioClip);

			emitter.Finished += () =>
			{
				Destroy(emitter.gameObject);
			};
		}
	}
}