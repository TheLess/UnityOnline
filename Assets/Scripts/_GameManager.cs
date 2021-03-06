﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net.Sockets;

public class _GameManager : MonoBehaviour {
    public static _GameManager Instance { set; get; }

    public GameObject serverPrefab;
    public GameObject clientPrefab;
    private GameObject PlayerNumber;
    private InputField IpInput=null;
    private InputField PortInput=null;

    public int myOnlineCount;//need to be synchronized
    public string side = null;

    private GameObject Button=null;

    public bool isMyturn;
    private bool duringPing=false;
    bool isStartGame = false;

    private void Start()
    {
        Instance = this;
        myOnlineCount = 0;
        DontDestroyOnLoad(gameObject);
        
    }
    public void OnClickConnect()
    {
        string hostAddress = "192.168.0.107";//192.168.0.107，115.195.87.182
        int port=6321;
        IpInput = GameObject.Find("Canvas").transform.Find("IpInput").GetComponent<InputField>();
        PortInput = GameObject.Find("Canvas").transform.Find("PortInput").GetComponent<InputField>();
        if (IpInput.text != "")
        {
            hostAddress = IpInput.text;
        }
        if(PortInput.text!="")
        {
            int.TryParse(PortInput.text, out port);
        }

            try
        {
            if (GameObject.FindObjectOfType<Client>())
                return;
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = "client";            
            c.ConnectToServer(hostAddress, port);
            
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void OnClickHost()
    {
        if (GameObject.FindObjectOfType<Server>())
            return;
        duringPing = true;
        Invoke("setPingFalse", 1.0f);
        try
        {
            TcpClient c = new Client.TcpClientWithTimeout("192.168.0.107", 6321, 10).Connect();
            if(c!=null)
            {
                
                c.Close();
                //弹出一个窗口告诉他，已经有server了
                GameObject.Find("Canvas").transform.Find("Modal Dialog").gameObject.SetActive(true);
                
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = "host";
            c.ConnectToServer("192.168.0.107", 6321);//localhost
        }
    }
    public void setPingFalse()
    {
        duringPing = false;
    }
    private void Update()
    {
        PlayerNumber = GameObject.Find("Canvas").transform.Find("PlayerNumberText").gameObject;
        PlayerNumber.GetComponent<Text>().text = "PlayerNumber: " + myOnlineCount.ToString();
        //Debug.Log(isMyturn);
        if (Button == null&&GameObject.Find("Button") != null)
        {
            Debug.Log("find button");
            Button = GameObject.Find("Button");
            Button.GetComponent<Button>().onClick.AddListener(RunMyTurn);
        }
        if (myOnlineCount == 2 && !duringPing && !isStartGame)
        {
            myStartGame();
        }
    }

    public void myStartGame()
    {
        isStartGame = true;
        //choose if I am white or black 
        GameObject.Find("Canvas").transform.Find("Modal Dialog").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("Modal Dialog").Find("ConfirmButton").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("Modal Dialog").Find("ChooseSide").gameObject.SetActive(true);
        //SceneManager.LoadScene("Game");

        if (GameObject.FindObjectOfType<Server>())
            isMyturn = true;
        else
            isMyturn = false;
    }
    public void OnChooseSide()
    {
        GameObject.Find("Canvas").transform.Find("Modal Dialog").Find("ChooseSide").gameObject.SetActive(false);
        StartCoroutine(CountDown());
    }
    IEnumerator CountDown()
    {
        GameObject.Find("Canvas").transform.Find("Modal Dialog").Find("CountDownText").gameObject.SetActive(true);
        Text countdown = GameObject.Find("Canvas").transform.Find("Modal Dialog").Find("CountDownText").GetComponent<Text>();
        countdown.text = "You get " + side + " side!";
        yield return new WaitForSeconds(1f);
        countdown.text = "Game will Start in...";
        yield return new WaitForSeconds(0.5f);
        countdown.fontSize = 200;
        countdown.text = "3";
        yield return new WaitForSeconds(1f);
        countdown.text = "2";
        yield return new WaitForSeconds(1f);
        countdown.text = "1";
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Game");
    }


    public void RunMyTurn()
    {
            if (isMyturn)
            {
                Client c = GameObject.FindObjectOfType<Client>();
                c.Send("CRUN|"+c.clientName);
                Debug.Log("c.name :" + c.clientName);
                isMyturn = !isMyturn;
                GameObject.Find("ShowTurn").GetComponent<Text>().text = "you end up your turn";
            }
            else
            {
                GameObject.Find("ShowTurn").GetComponent<Text>().text = "it is not your turn";
                return;
            }
    }
    public void ItsYourTurn()
    {
        this.isMyturn = true;
        if (GameObject.Find("ShowTurn"))
            GameObject.Find("ShowTurn").GetComponent<Text>().text = "it is your turn now!";
    }
    
}
