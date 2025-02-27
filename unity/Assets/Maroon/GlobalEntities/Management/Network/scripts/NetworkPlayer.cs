﻿using Maroon.GlobalEntities;
using Mirror;
using TMPro;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject firstPersonCharacter;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject colouredBody;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private CharacterController _cc;
    private AudioSource _as;
    private ModeFirstPerson _fpc;
    private GameManager _gm;

    private Color _noControlColor = new Color(0.5f, 0, 0);
    private Color _inControlColor = new Color(0, 0.5f, 0);

    [SyncVar(hook = "OnChangeName")] private string _name;
    
    // Start is called before the first frame update
    void Start()
    {
        if(isServer && isLocalPlayer) //only spawn once on server
            Maroon.NetworkManager.Instance.ServerSpawnControlHandlingUi();
        
        if(isLocalPlayer)
            CmdClientLoadedScene();
        
        if (firstPersonCharacter == null) //not in Laboratory
            return;
        _cc = GetComponent<CharacterController>();
        _as = GetComponent<AudioSource>();
        _fpc = GetComponent<ModeFirstPerson>();
        _gm = FindObjectOfType<GameManager>();

        if (isLocalPlayer)
        {
            firstPersonCharacter.SetActive(true);
            _cc.enabled = true;
            _as.enabled = true;
            _fpc.enabled = true;

            body.SetActive(false);
            
            _gm.RegisterNetworkPlayer(gameObject);
        }
        else
        {
            InvokeRepeating(nameof(UpdateControlColor), 0, 0.5f);
        }
    }

    [Command]
    private void CmdClientLoadedScene()
    {
        var sync = FindObjectOfType<ExperimentNetworkSync>();
        if (sync != null)
        {
            sync.ClientLoadedScene(connectionToClient);
        }
    }

    private void OnChangeName(string oldName, string newName)
    {
        if (isLocalPlayer && Maroon.NetworkManager.Instance.PlayerName == null)
        {
            Maroon.NetworkManager.Instance.PlayerName = newName;
        }
        else if(firstPersonCharacter != null) //in Laboratory
        {
            playerNameText.text = newName;
        }
    }

    private void UpdateControlColor()
    {
        if (_name == Maroon.NetworkManager.Instance.ClientInControl)
        {
            SetColor(_inControlColor);
        }
        else
        {
            SetColor(_noControlColor);
        }
    }

    private void SetColor(Color color)
    {
        foreach (var rend in colouredBody.GetComponentsInChildren<Renderer>())
        {
            rend.material.color = color;
        }
    }

    [Server]
    public void SetName(string newName)
    {
        _name = newName;
        if (isLocalPlayer && Maroon.NetworkManager.Instance.PlayerName == null)
            Maroon.NetworkManager.Instance.PlayerName = newName;
    }

    private void OnDisable()
    {
        if (isServer)
        {
            Maroon.NetworkManager.Instance.RemovePlayerForConnection(connectionToClient);
        }
        if (firstPersonCharacter == null) //not in Laboratory
            return;
        if (isLocalPlayer)
        {
            _gm.UnregisterNetworkPlayer();
        }
    }
}
