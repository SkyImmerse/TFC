using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Tibia.UI.Login_Interface
{
    public class UICharacterItem : MonoBehaviour
    {
        public event Action<ClassicNetwork.Account.Character> Selected;

        public TMPro.TMP_Text Label;
        public TMPro.TMP_Text WorldName;
        public ClassicNetwork.Account.Character Character;
        public void OnClick()
        {
            Selected?.Invoke(Character);
        }
    }
}
