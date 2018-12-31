using Assets.Tibia.ClassicNetwork;
using Assets.Tibia.DAO;
using Game.DAO;
using GameClient;
using GameClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.Login_Interface
{
    [RequireComponent(typeof(UIVisibleToggle))]
    public class UICharacterListController : MonoBehaviour
    {
        public bool Visible
        {
            get => GetComponent<UIVisibleToggle>().Visible;
            set => GetComponent<UIVisibleToggle>().Visible = value;
        }

        public Transform List;
        public GameObject ItemPrefab;
        public Button ConfirmButton;

        public LoadingCircle ConnecionProgress;

        public TMP_Text PremiumDays;
        public TMP_Text Status;

        bool IsLoginProgress = false;

        private ClassicNetwork.Account.Character Selected;

        public UILoginController LoginController;
        MicroTask token = null;
        private void Start()
        {
            ResetController();
            GameServer.ConnectionSuccussfull += GameServer_OnConnect;
            LoginServer.CharacterList += LoginServerConnection_CharacterList;
            GameServer.EnterGame += GameServerConnection_EnterGame;
        }

        private void GameServer_OnConnect()
        {
            LoginServer.Instance.Connection.Disconnect();
        }

        private void GameServerConnection_EnterGame()
        {
            UIInterfaceVisibility.HideAllLoginInterface();
            UIInterfaceVisibility.ShowAllGameInterface();

            // synchronize fight modes with the server
            //GameServer.Instance.SendChangeFightModes(Game.FightMode, _mChaseMode, _mSafeFight, _mPvpMode);

        }

        private void LoginServerConnection_CharacterList(List<ClassicNetwork.Account.Character> characters, ClassicNetwork.Account.Account account)
        {
            if (account.Status != AccountStatus.Ok)
            {
                PremiumDays.text = account.Status.ToString();
            }
            else if (account.SubStatus == SubscriptionStatus.Free)
            {
                PremiumDays.text = "No premium";
            }
            else
            {
                PremiumDays.text = "Premium expires in: " + account.PremDays.ToString() + " days";
            }
            ClearList();
            foreach (var item in characters)
            {
                AddCharacter(item);
            }
        }

        private void ClearList()
        {
            foreach (Transform item in List)
            {
                item.GetComponent<UICharacterItem>().Selected -= UICharacterListController_Selected;
                GameObject.Destroy(item.gameObject);
            }
        }

        public void ResetController()
        {
            ConfirmButton.interactable = false;
            Selected = null;
            ClearList();
            if (GameServer.Instance != null)
                GameServer.Instance.CurrentCharacterName = null;
        }

        void AddCharacter(ClassicNetwork.Account.Character c)
        {
            var character = Instantiate(ItemPrefab, List);
            character.GetComponent<UICharacterItem>().Label.text = c.Name;
            character.GetComponent<UICharacterItem>().WorldName.text = "(" + c.WorldName + ")";
            character.GetComponent<UICharacterItem>().Character = c;
            character.GetComponent<UICharacterItem>().Selected += UICharacterListController_Selected;
        }

        private void UICharacterListController_Selected(ClassicNetwork.Account.Character obj)
        {
            Selected = obj;
            ConfirmButton.interactable = true;
        }


        public void OnCharacterConfirm()
        {
            if (Selected == null) return;
            if (IsLoginProgress) return;

            try
            {
                GameServer.Instance.CurrentCharacterName = Selected.Name;
                LocalPlayer.Current = new LocalPlayer();
                LocalPlayer.Current.Name = Creature.FormatCreatureName(Selected.Name);
                GameServer.Instance.Connect(Config.Host, Selected.WorldPort);

                ConnecionProgress.Visible = true;

                if (token != null) token.Running = false;
                token = MicroTasks.PeriodicTask((t) =>
                {
                    if (!GameServer.Instance.Connection.IsConnected && t.CallCount == Config.LoginCheckAttemptCount)
                    {
                        token.Running = false;

                        IsLoginProgress = false;

                        LoginController.LoginStatus.text = "Game server unreachable";

                        LoginController.LoginStatusForm.Visible = true;
                        LoginController.LoginForm.Visible = false;
                        LoginController.IsLoginSuccessfull = false;

                        ConnecionProgress.Visible = false;

                        Visible = false;

                        ResetController();

                    }
                }, Config.LoginCheckTimeout);

            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

        }
    }
}
