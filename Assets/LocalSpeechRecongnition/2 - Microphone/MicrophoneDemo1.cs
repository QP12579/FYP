using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class MicrophoneTestDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public PlayerSpeechSkill _SpeechKeywords;
        public bool streamSegments = true;
        
        [Header("UI")] 
 
        public TMP_Text outputText;
        public TMP_Text timeText;
        
        private string _buffer;

        private void Awake()
        {
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            
            microphoneRecord.OnRecordStop += OnRecordStop;

            whisper.language = "en";

        }

      

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                OnKeyDown();
            }
        }

        private void OnVadChanged(bool vadStop)
        {
            microphoneRecord.vadStop = vadStop;
        }

        private void OnKeyDown()
        {
            if (!microphoneRecord.IsRecording)
            {
                microphoneRecord.StartRecord();
                outputText.text = "Recording";
            }
            else
            {
                microphoneRecord.StopRecord();
                outputText.text = $"<color=yellow>Sending...</color>";
            }
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || !outputText) 
                return;

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);
            timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

            var text = res.Result.ToLower();

            outputText.text = text;
            _SpeechKeywords.CheckForKeyword();
        }

        public string GetOutputText() 
        {
            return outputText.text;
        }

        private void OnTranslateChanged(bool translate)
        {
            whisper.translateToEnglish = translate;
        }

        private void OnProgressHandler(int progress)
        {
            if (!timeText)
                return;
            timeText.text = $"Progress: {progress}%";
        }
        
        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments || !outputText)
                return;

            _buffer += segment.Text.ToLower();
            outputText.text = _buffer + "...";
        }
    }
}