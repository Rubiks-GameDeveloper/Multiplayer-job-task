using TMPro;
using UnityEngine;

namespace Gameplay.Entities.Coin
{
    public class WinScreenData : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winPlayerCoinCount;
        [SerializeField] private TextMeshProUGUI winPlayerName;
        public TextMeshProUGUI GetWinPlayerCoinCount()
        {
            return winPlayerCoinCount;
        }
        public TextMeshProUGUI GetWinPlayerName()
        {
            return winPlayerName;
        }
    }
}
