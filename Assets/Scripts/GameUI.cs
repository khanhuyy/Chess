using UnityEngine;
using System;
using Net;
using Net.Message;
using TMPro;

public enum CameraAngle
{
    Menu = 0,
    WhiteTeam = 1,
    BlackTeam = 2
}

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { private set; get; }

    public Server server;
    public Client client;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject generalLayout;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame;
    private static readonly int InGameMenu = Animator.StringToHash("InGameMenu");
    private static readonly int StartMenu = Animator.StringToHash("StartMenu");
    private static readonly int OnlineMenu = Animator.StringToHash("OnlineMenu");
    private static readonly int HostMenu = Animator.StringToHash("HostMenu");

    private void Awake()
    {
        Instance = this;
        RegisterEvents();
    }

    public void ChangeCamera(CameraAngle index)
    {
        foreach (var angle in cameraAngles)
        {
            angle.SetActive(false);
        }
        Debug.Log(index);
        cameraAngles[(int)index].SetActive(true);
    }

    public void SetGeneralLayoutActive(bool isActive)
    {
        generalLayout.SetActive(isActive);
    }

    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger(InGameMenu);
        SetLocalGame?.Invoke(true);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnCoOpGameButton()
    { // OnOnlineGameButton
        menuAnimator.SetTrigger(OnlineMenu);
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        menuAnimator.SetTrigger(HostMenu);
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        client.Init(addressInput.text, 8007);
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger(StartMenu);
    }

    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        menuAnimator.SetTrigger(OnlineMenu);
    }

    public void OnLeaveFromInGameMenu()
    {
        ChangeCamera(CameraAngle.Menu);
        menuAnimator.SetTrigger(StartMenu);
        client.Shutdown();
        server.Shutdown();
    }

    #region 
    private void RegisterEvents()
    {
        NetUtility.CStartGame += OnStartGameClient;
    }

    private void UnRegisterEvents()
    {
        NetUtility.CStartGame -= OnStartGameClient;
    }

    private void OnStartGameClient(NetMessage msg)
    {
        menuAnimator.SetTrigger(InGameMenu);
        generalLayout.gameObject.SetActive(false);
    }
    #endregion
}
