using Assets.Tibia.UI.GameInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.UI.Login_Interface
{
    public class UIInterfaceVisibility : MonoBehaviour
    {
        public UIVisibleToggle AllLogin;
        public UIVisibleToggle AllGame;

        private static UIInterfaceVisibility Static;
        private void Awake()
        {
            Static = this;
        }

        public static void HideAllLoginInterface()
        {
            Static.hideAllLoginInterface();
        }

        private void hideAllLoginInterface()
        {
            AllLogin.Visible = false;
            if (AllLogin.GetComponent<AudioSource>() != null)
            {
                AllLogin.GetComponent<AudioSource>().Stop();
                AllLogin.GetComponent<AudioSource>().enabled = false;
            }
            FindObjectOfType<UILoginController>().ResetController();
            FindObjectOfType<UICharacterListController>().ResetController();

            FindObjectOfType<UILoginController>().Visible = true;
            FindObjectOfType<UICharacterListController>().Visible = false;
        }

        public static void ShowAllLoginInterface()
        {
            Static.showAllLoginInterface();
        }

        private void showAllLoginInterface()
        {
            AllLogin.Visible = true;
            if (AllLogin.GetComponent<AudioSource>() != null)
            {
                AllLogin.GetComponent<AudioSource>().Play();
                AllLogin.GetComponent<AudioSource>().enabled = true;
            }
            FindObjectOfType<UILoginController>().ResetController();
            FindObjectOfType<UICharacterListController>().ResetController();

            FindObjectOfType<UILoginController>().Visible = true;
            FindObjectOfType<UICharacterListController>().Visible = false;
        }

        public static void HideAllGameInterface()
        {
            Static.hideAllGameInterface();
        }

        private void hideAllGameInterface()
        {
            AllGame.Visible = false;
            FindObjectOfType<UIConsole>().ResetController();
        }

        public static void ShowAllGameInterface()
        {
            Static.showAllGameInterface();
        }

        private void showAllGameInterface()
        {
            AllGame.Visible = true;
            FindObjectOfType<UIConsole>().ResetController();
        }
    }
}
