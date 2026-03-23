using UnityEngine;

namespace MountainBackgroundsPixelArt
{
    public class ParallaxEffect : MonoBehaviour
    {
        private Transform mainCamera;
        private Transform player;

        public float parallaxIntensityX;
        public float parallaxIntensityY;
        public float independantSpeed;

        private float cameraSize;
        private float spriteWidth;
        private Vector2 initialPos;
        private float translationOffset = 0;

        private void Start()
        {
            mainCamera = Camera.main.transform;
            cameraSize = Camera.main.orthographicSize;
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
            spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x / 3;

            transform.position = new Vector2(mainCamera.position.x, player.position.y - 3f);
            initialPos = transform.position;
        }

        private void LateUpdate()
        {
            translationOffset += independantSpeed * Time.deltaTime * parallaxIntensityX;

            float parallaxOffsetX = (mainCamera.position.x * (1 - (parallaxIntensityX / 2))) + translationOffset;
            float parallaxOffsetY = ((mainCamera.position.y / cameraSize) / 0.7f) * (1 - parallaxIntensityY);

            transform.position = new Vector2(initialPos.x + parallaxOffsetX, initialPos.y + parallaxOffsetY);

            float cameraOffsetX = mainCamera.position.x - transform.position.x;

            if (cameraOffsetX > spriteWidth / 2)
                initialPos.x += spriteWidth;
            else if (cameraOffsetX < -spriteWidth / 2)
                initialPos.x -= spriteWidth;
        }
    }
}

