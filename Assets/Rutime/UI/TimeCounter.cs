using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LifeGame3D.UI
{
    public class TimeCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _tmp;
        private StringBuilder sb;

        private List<float> _fpsList = new(256);
        private float _startTime;
        private int _startFrame;
        private bool _isRecording;

        public void Awake()
        {
            sb = new StringBuilder();
        }

        private void OnEnable()
        {
            LifeGameBehaviour.OnPlay += StartRecord;
            LifeGameBehaviour.OnStopped += StopRecord;
        }

        private void OnDisable()
        {
            LifeGameBehaviour.OnPlay -= StartRecord;
            LifeGameBehaviour.OnStopped -= StopRecord;
        }

        private void StartRecord()
        {
            _startTime = Time.time;
            _startFrame = Time.frameCount;
            _isRecording = true;
        }

        private void StopRecord()
        {
            _isRecording = false;
            CopyToBuffer();
        }

        public void Update()
        {
            if(!_isRecording) return;

            var time = Time.time - _startTime;
            var frameCount = Time.frameCount - _startFrame;

            var averageFPS = frameCount / time;
            var fps = 1f / Time.deltaTime;
            _fpsList.Add(fps);
            
            sb.Clear();
            sb.Append($"Time : {time}");
            sb.Append("<br>");
            sb.Append($"avg.FPS : {averageFPS}");
            sb.Append("<br>");
            sb.Append($"FPS : {fps}");
            _tmp.SetText(sb);
        }

        [ContextMenu("コピー")]
        public void CopyToBuffer()
        {
            GUIUtility.systemCopyBuffer = string.Join('\n', _fpsList);
        }
    }
}
