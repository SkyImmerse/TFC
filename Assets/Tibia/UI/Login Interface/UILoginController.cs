using Assets;
using Assets.Tibia.ClassicNetwork;
using Assets.Tibia.DAO;
using Assets.Tibia.UI;
using Assets.Tibia.UI.Login_Interface;
using Game.Graphics;
using GameClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIVisibleToggle))]
public class UILoginController : MonoBehaviour
{
    public bool Visible
    {
        get => GetComponent<UIVisibleToggle>().Visible;
        set => GetComponent<UIVisibleToggle>().Visible = value;
    }
    public UIVisibleToggle LoginForm;
    public TMP_InputField Login;
    public TMP_InputField Password;
    public LoadingCircle ConnecionProgress;

    public Toggle RememberPassword;

    public UIVisibleToggle LoginStatusForm;
    public TMP_Text LoginStatus;

    public UICharacterListController CharacterListController;

    public bool IsLoginSuccessfull = false;
    bool IsLoginProgress = false;
    MicroTask token = null;

    private void Start()
    {
        ResetController();

        LoginServer.LoginError += LoginServerConnection_LoginError;
        LoginServer.MOTD += LoginServerConnection_MOTD;
        LoginServer.ConnectionSuccussfull += LoginServerConnection_ConnectionSuccussfull;
    }

    private void LoginServerConnection_ConnectionSuccussfull()
    {
        LoginServer.Instance.Login(Login.text, Password.text);
        if (token != null) token.Running = false;
    }

    private void LoginServerConnection_MOTD(string motd)
    {
        var motdNumber = int.Parse(motd.Substring(0, motd.IndexOf("\n")));
        var motdMessage = motd.Substring(motd.IndexOf("\n") + 1);
        IsLoginProgress = false;

        LoginStatus.text = motdMessage;

        LoginStatusForm.Visible = true;
        LoginForm.Visible = false;


        ConnecionProgress.Visible = false;
        IsLoginSuccessfull = true;

        if(RememberPassword.isOn)
        {
            PlayerPrefs.SetString("login", LoginServer.Instance.CurrentLogin);
            PlayerPrefs.SetString("password", LoginServer.Instance.CurrentPassword);
        } else
        {
            PlayerPrefs.DeleteKey("login");
            PlayerPrefs.DeleteKey("password");
        }

        if (token != null) token.Running = false;

    }

    private void LoginServerConnection_LoginError(string obj)
    {
        IsLoginProgress = false;

        if (token != null) token.Running = false;

        LoginStatus.text = obj;

        LoginStatusForm.Visible = true;
        LoginForm.Visible = false;

        ConnecionProgress.Visible = false;
        IsLoginSuccessfull = false;
    }

    public void OnLoginContinue()
    {
        if (!LoginStatusForm.Visible) return;

        LoginStatusForm.Visible = false;
        LoginStatus.text = string.Empty;

        if (IsLoginSuccessfull)
        {
            CharacterListController.Visible = true;
            if (token != null) token.Running = false;

        } else
        {
            ResetController();
        }
    }

    public void ResetController()
    {
        LoginForm.Visible = true;
        LoginStatus.text = string.Empty;
        CharacterListController.Visible = false;

        IsLoginSuccessfull = false;
        IsLoginProgress = false;

        LoginStatusForm.Visible = false;

        ConnecionProgress.Visible = false;

        if(PlayerPrefs.HasKey("login")) {
            Login.text = PlayerPrefs.GetString("login");
            RememberPassword.isOn = true;
        }
        if (PlayerPrefs.HasKey("password"))
        {
            Password.text = PlayerPrefs.GetString("password");
            RememberPassword.isOn = true;
        }

        LoginServer.Instance?.Connection?.Disconnect();

        if (token != null) token.Running = false;
    }

    public void OnClickLogout()
    {
        UIInterfaceVisibility.HideAllGameInterface();
        UIInterfaceVisibility.ShowAllLoginInterface();


        if (GameServer.Instance != null && GameServer.Instance.Connection != null)
        {
            if (GameServer.Instance.Connection.IsConnected)
            {
                MapRenderer.RemoveAll();
                Map.Maps.Clear();
                Map.Maps.Add(new Map());
                LocalPlayer.Current = null;
                List<string> mask = new List<string>();
                mask.Add("PostProcessing");
                Camera.main.cullingMask = LayerMask.GetMask(mask.ToArray());

                GameServer.Instance.SendLogout();
                GameServer.Instance.Connection.Disconnect();
            }
        }

    }
    public void OnClickLogin()
    {
        if (IsLoginProgress) return;

        IsLoginProgress = true;

        LoginServer.Instance.Connect(Config.Host, Config.Port);

        ConnecionProgress.Visible = true;

        if (token != null) token.Running = false;
        token = MicroTasks.PeriodicTask((t) =>
        {
            if (!LoginServer.Instance.Connection.IsConnected && t.CallCount == Config.LoginCheckAttemptCount)
            {
                if (token != null) token.Running = false;

                IsLoginProgress = false;

                LoginStatus.text = "Login server unreachable";

                LoginStatusForm.Visible = true;
                LoginForm.Visible = false;

                ConnecionProgress.Visible = false;
                IsLoginSuccessfull = false;

                LoginServer.Instance?.Connection?.Disconnect();

            }
        }, Config.LoginCheckTimeout);
    }

}
