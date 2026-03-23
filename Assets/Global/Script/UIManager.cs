using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Để các script khác gọi UIManager.Instance

    private VisualElement root;
    private Label coinLabel;
    private List<VisualElement> hearts = new List<VisualElement>();

    public AudioSource audioSource;
    public AudioClip coinSound; // Kéo file mp3 vào đây
    public AudioClip stompSound; 

    public void PlayStompSound()
    {
        if (audioSource != null && stompSound != null)
        {
            audioSource.PlayOneShot(stompSound);
        }
    }

    // Thêm hàm này để các script khác gọi
    public void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
        {
            // Dùng PlayOneShot để nếu ăn nhiều xu liên tiếp, tiếng nhạc sẽ chồng lên nhau nghe rất sướng tai
            audioSource.PlayOneShot(coinSound);
        }
    }

    private void OnEnable()
    {
        Instance = this;
        root = GetComponent<UIDocument>().rootVisualElement;

        // 1. Tìm Label hiện tiền (nhớ đặt tên trong UI Builder là CoinCount)
        coinLabel = root.Q<Label>("CoinCount");

        // 2. Tìm danh sách 5 trái tim (nhớ đặt tên trong UI Builder là Heart)
        hearts = root.Query<VisualElement>("Heart").ToList();

        //GameData.coinsAtLevelStart = GameData.coins;

        // Cập nhật số liệu ngay khi vào màn chơi
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Cập nhật chữ hiện tiền
        if (coinLabel != null) coinLabel.text = "x " + GameData.coins.ToString();

        // Cập nhật trái tim
        for (int i = 0; i < hearts.Count; i++)
        {
            // Nếu vị trí tim lớn hơn số mạng thì ẩn đi
            hearts[i].style.display = (i < GameData.lives) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void Awake()
    {
        // Nếu đã có một Instance rồi và nó không phải là cái script này
        if (Instance != null && Instance != this)
        {
            // Xóa cái GameObject dư thừa này đi ngay lập tức
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Nếu ông muốn UI này tồn tại xuyên suốt các màn chơi
        // DontDestroyOnLoad(gameObject); 
    }


}