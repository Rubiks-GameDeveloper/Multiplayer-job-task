using Gameplay.Entities.Coin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class CanvasController : MonoBehaviour
    {
        [SerializeField] private Joystick joystick;
        [SerializeField] private Image healthBar;
        [SerializeField] private Button shootButton;
        [SerializeField] private TextMeshProUGUI coinCount;
        [SerializeField] private GameObject winDisplay;
        public Joystick GetJoystick()
        {
            return joystick;
        }
        public Image GetHealthBar()
        {
            return healthBar;
        }
        public GameObject GetWinDisplay()
        {
            return winDisplay;
        }
        public Button GetShootButton()
        {
            return shootButton;
        }
        public TextMeshProUGUI GetCoinCountText()
        {
            return coinCount;
        }
        public WinScreenData GetWinScreenData()
        {
            return GetComponentInChildren<WinScreenData>();
        }
    }
}

