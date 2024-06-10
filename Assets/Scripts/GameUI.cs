using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public enum CameraAngle
{
    Menu = 0,
    WhiteTeam = 1,
    BlackTeam = 2
}

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }

    public Server server;
    public Client client;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame;

    private void Awake()
    {
        Instance = this;
        // RegisterEvents();
    }

    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }
        cameraAngles[(int)index].SetActive(true);
    }

    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnCoOpGameButton()
    { // OnOnlineGameButton
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        client.Init(addressInput.text, 8007);
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnLeaveFromInGameMenu()
    {
        ChangeCamera(CameraAngle.Menu);
        menuAnimator.SetTrigger("StartMenu");
        client.Shutdown();
        server.Shutdown();
    }

    #region 
    private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient; ;
    }

    private void UnRegisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient; ;
    }

    private void OnStartGameClient(NetMessage msg)
    {
        menuAnimator.SetTrigger("InGameMenu");
    }
    #endregion
}
