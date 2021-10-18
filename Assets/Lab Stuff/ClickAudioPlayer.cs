using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAudioPlayer : MonoBehaviour
{
    private AudioSource _audiosource;

    private void Awake() {
        _audiosource = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        PlayerController.zipLineAction += PlayAudio;
        CreateLevel.generateAction += PlayAudio;
    }

    private void OnDisable() {
        PlayerController.zipLineAction -= PlayAudio;
        CreateLevel.generateAction -= PlayAudio;
    }

    private void PlayAudio(){
        _audiosource.Play();
    }
}
