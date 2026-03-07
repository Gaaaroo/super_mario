using UnityEngine;

public class Mario : MonoBehaviour
{
    public Transform shootPoint;
    public GameObject marioBulletPrefab;

    public bool isSpreadShot = false;
    public float fireCooldown = 0.5f;
    private float nextFireTime = 0f;

    private void Update()
    {
        // Bấm phím J (hoặc chuột trái) để bắn
        if (Input.GetKeyDown(KeyCode.J) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void Shoot()
    {
        if (!isSpreadShot)
        {
            // Bắn 1 tia thẳng
            Instantiate(marioBulletPrefab, shootPoint.position, shootPoint.rotation);
        }
        else
        {
            // Bắn 3 tia (Spread Shot)
            // Tia thẳng
            Instantiate(marioBulletPrefab, shootPoint.position, shootPoint.rotation);

            // Tia hướng lên (xoay trục Z lên 15 độ)
            Instantiate(marioBulletPrefab, shootPoint.position, shootPoint.rotation * Quaternion.Euler(0, 0, 15f));

            // Tia hướng xuống (xoay trục Z xuống -15 độ)
            Instantiate(marioBulletPrefab, shootPoint.position, shootPoint.rotation * Quaternion.Euler(0, 0, -15f));
        }
    }
}