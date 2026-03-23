using UnityEngine;

namespace MountainBackgroundsPixelArt
{
    [ExecuteInEditMode]
    public class MainCamera : MonoBehaviour
    {
        private Transform player;

        public bool smoothCamera = true;
        public bool lockVerticalAxis = false;
        public bool lockCameraSize = false;
        public float cameraSize = 5f;

        private void Start()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        }

        private void Update()
        {
            Camera.main.orthographicSize = lockCameraSize ? 5f : cameraSize;

            float playerDistanceY = Camera.main.orthographicSize * 0.7f;
            float smoothSpeed = 5.0f;

            Vector3 desiredPosition = new Vector3(player.position.x, lockVerticalAxis ? playerDistanceY : player.position.y + playerDistanceY, -10f);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothCamera ? smoothedPosition : desiredPosition;
        }
    }
}
