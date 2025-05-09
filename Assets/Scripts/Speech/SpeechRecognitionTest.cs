using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Mirror;
using HuggingFace.API;

 public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] public TextMeshProUGUI text;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartRecording);  
        stopButton.onClick.AddListener(StopRecording);
    }

    // Update is called once per frame
    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples) 
        { 
            StopRecording();
        }
    }

    private void StartRecording()
    {
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
        startButton.interactable = false;
    
        stopButton.interactable = true;
    }

    private void StopRecording() 
    {
        if (!recording) return;

        var position = Microphone.GetPosition(null);
        Microphone.End(null);

        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;

        File.WriteAllBytes(Application.dataPath + "/test.wav", bytes);
        SendRecording();
        
        stopButton.interactable = false;
    }

    private void SendRecording()
    {
        text.color = Color.yellow;
        text.text = "Sending...";
        stopButton.interactable = false;
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
            text.color = Color.white;
            text.text = response.ToLower();
            startButton.interactable = true;
        }, error => {
            text.color = Color.red;
            text.text = error;
            startButton.interactable = true;
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}
