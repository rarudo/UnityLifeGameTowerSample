using System;
using UnityEngine;

namespace LifeGame3D.Camera
{
    public class LifeGameCameraRig : MonoBehaviour
    {
        public int timing = 30;
        private bool _enable;
        private Vector3 _position;

        private void OnEnable()
        {
            LifeGameBehaviour.OnPlay += () => _enable = true;
            LifeGameBehaviour.OnStopped += () => _enable = false;

        }

        private void Start()
        {
            var pos = FindObjectOfType<LifeGameBehaviour>().size;
            _position = new Vector3(pos.x / 2f, pos.x / 2f, pos.z / 2f);
        }

        private void Update()
        {
            if(!_enable) return;
            
            var frameCount = Time.frameCount;
            if (frameCount % timing != 0) return;
            
            var pos = transform.position;
            transform.position = new Vector3(_position.x, _position.y + frameCount, _position.z);
        }
    }
}
