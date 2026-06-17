using UnityEngine;
using UnityEngine.UI;
using SkyloftGame.Audio;

namespace SkyloftGame.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        private void Awake()
            => GetComponent<Button>().onClick.AddListener(() => AudioEvents.Play(AudioCue.UIClick));
    }
}
