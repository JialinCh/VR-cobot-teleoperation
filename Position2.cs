using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;
using UnityEngine.UI;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.EventSystems;


public class Position2 : MonoBehaviour
{
    public GameObject end_effector;
    public Text my_text;
    public OVRHand _OVRHand;
    public GameObject background_color;
    float time = 0;
    private InputDevice targetDevice;
    public Vector3 position;
    public Vector3 oriP;
    private int num = 0;
    public void Start()
    {
        GameObject.Find("GameObject").GetComponent<Position2>().enabled = false;
        UDPClientIP = "192.168.95.1";//服务端的IP.自己更改
        UDPClientIP = UDPClientIP.Trim();       
        InitSocket(); 
    }

    void Update()
    {
        /*if (OVRInput.Get(OVRInput.Button.Two))
        {
            GameObject.Find("GameObject").GetComponent<Position>().enabled = false;
        }*/
        time += Time.deltaTime;
        if (time >= 0.3f)
        {
            time = 0;


            position = _OVRHand.PointerPose.localPosition;
            //rotation = _OVRHand.PointerPose.localRotation;
            if (position != Vector3.zero)
            {
                if (num == 0)
                {
                    oriP = position;
                    num += 1;
                }
                Debug.LogFormat(MixedRealityPlayspace.Transform.TransformPoint(position - oriP).ToString() + (MixedRealityPlayspace.Rotation * _OVRHand.PointerPose.localRotation).ToString());
                SocketSend(MixedRealityPlayspace.Transform.TransformPoint(position - oriP).ToString() + (MixedRealityPlayspace.Rotation * _OVRHand.PointerPose.localRotation).ToString());
            }
            if (MixedRealityPlayspace.Transform.TransformPoint(position-oriP).y > -0.045)
            {
                end_effector.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                background_color.GetComponent<RawImage>().color = Color.blue;
            }
            if (MixedRealityPlayspace.Transform.TransformPoint(position-oriP).y <= -0.045 & MixedRealityPlayspace.Transform.TransformPoint(position-oriP).y >= -0.13)
            {
                end_effector.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                background_color.GetComponent<RawImage>().color = Color.green;
            }
            if (MixedRealityPlayspace.Transform.TransformPoint(position-oriP).y < -0.13)
            {
                end_effector.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                background_color.GetComponent<RawImage>().color = Color.red;
            }

        }
    }


    public string recvStr;
    private string UDPClientIP;
    Socket socket;
    EndPoint serverEnd;
    IPEndPoint ipEnd;
 
    byte[] sendData = new byte[1024];   
 
    
    public void InitSocket()
    {
        ipEnd = new IPEndPoint(IPAddress.Parse(UDPClientIP), 5500);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        print("等待连接");
        print("连接");
    }

    public void SocketSend(string sendStr)
    {
        //Empty
        sendData = new byte[1024];
        //Data transform
        sendData = Encoding.UTF8.GetBytes(sendStr);
        //Send to designated server port
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }

 
    //连接关闭
    public void SocketQuit()
    {
        //最后关闭socket
        if (socket != null)
            socket.Close();
    }
    void OnApplicationQuit()
    {
        SocketQuit();
    }

}