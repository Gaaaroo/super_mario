//using UnityEngine;
//using UnityEngine.UIElements;
//using System.Collections.Generic;

//public class UIManager : MonoBehaviour
//{
//    public static UIManager Instance; // Để các script khác gọi UIManager.Instance

//    private VisualElement root;
//    private Label coinLabel;
//    private List<VisualElement> hearts = new List<VisualElement>();

//    public AudioSource audioSource;
//    public AudioClip coinSound; // Kéo file mp3 vào đây
//    public AudioClip stompSound; 

//    public void PlayStompSound()
//    {
//        if (audioSource != null && stompSound != null)
//        {
//            audioSource.PlayOneShot(stompSound);
//        }
//    }

//    // Thêm hàm này để các script khác gọi
//    public void PlayCoinSound()
//    {
//        if (audioSource != null && coinSound != null)
//        {
//            // Dùng PlayOneShot để nếu ăn nhiều xu liên tiếp, tiếng nhạc sẽ chồng lên nhau nghe rất sướng tai
//            audioSource.PlayOneShot(coinSound);
//        }
//    }

//    private void OnEnable()
//    {
//        Instance = this;
//        root = GetComponent<UIDocument>().rootVisualElement;

//        // 1. Tìm Label hiện tiền (nhớ đặt tên trong UI Builder là CoinCount)
//        coinLabel = root.Q<Label>("CoinCount");

//        // 2. Tìm danh sách 5 trái tim (nhớ đặt tên trong UI Builder là Heart)
//        hearts = root.Query<VisualElement>("Heart").ToList();

//        //GameData.coinsAtLevelStart = GameData.coins;

//        // Cập nhật số liệu ngay khi vào màn chơi
//        UpdateUI();
//    }

//    public void UpdateUI()
//    {
//        // Cập nhật chữ hiện tiền
//        if (coinLabel != null) coinLabel.text = "x " + GameData.coins.ToString();

//        // Cập nhật trái tim
//        for (int i = 0; i < hearts.Count; i++)
//        {
//            // Nếu vị trí tim lớn hơn số mạng thì ẩn đi
//            hearts[i].style.display = (i < GameData.lives) ? DisplayStyle.Flex : DisplayStyle.None;
//        }
//    }

//    private void Awake()
//    {
//        // Nếu đã có một Instance rồi và nó không phải là cái script này
//        if (Instance != null && Instance != this)
//        {
//            // Xóa cái GameObject dư thừa này đi ngay lập tức
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;

//        // Nếu ông muốn UI này tồn tại xuyên suốt các màn chơi
//        // DontDestroyOnLoad(gameObject); 
//    }


//}


using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private VisualElement root;
    private Label coinLabel;
    private List<VisualElement> hearts = new List<VisualElement>();

    [Header("SFX (Tiếng động ngắn)")]
    public AudioSource sfxSource;    // Loa này dùng cho tiếng ăn xu, giẫm quái
    public AudioClip coinSound;
    public AudioClip stompSound;

    [Header("Music (Nhạc nền/Power-up)")]
    public AudioSource musicSource;  // Loa này dùng để phát nhạc Starman
    public AudioClip starmanMusic;   // Kéo file "tô tô tồ tô" vào đây

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        coinLabel = root.Q<Label>("CoinCount");
        hearts = root.Query<VisualElement>("Heart").ToList();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (coinLabel != null) coinLabel.text = "x " + GameData.coins.ToString();
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].style.display = (i < GameData.lives) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // --- PHÁT TIẾNG ĐỘNG NGẮN ---
    public void PlayCoinSound()
    {
        if (sfxSource != null && coinSound != null) sfxSource.PlayOneShot(coinSound);
    }

    public void PlayStompSound()
    {
        if (sfxSource != null && stompSound != null) sfxSource.PlayOneShot(stompSound);
    }

    // --- PHÁT NHẠC TÀNG HÌNH (TÔ TÔ TỒ TÔ TỐ) ---
    public void StartStarmanMusic()
    {
        if (musicSource != null && starmanMusic != null)
        {
            musicSource.clip = starmanMusic;
            musicSource.loop = true; // Nhạc tàng hình phải lặp lại
            musicSource.Play();
            Debug.Log("Nhạc Starman đang phát!");
        }
    }

    public void StopStarmanMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            Debug.Log("Dừng nhạc Starman.");
        }
    }
}