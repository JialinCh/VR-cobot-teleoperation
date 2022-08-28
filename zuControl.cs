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



public class zuControl : MonoBehaviour
{
    Socket socket; //目标socket
    EndPoint clientEnd; //客户端
    IPEndPoint ipEnd; //侦听端口
    string recvStr; //接收的字符串
    byte[] recvData = new byte[1024]; //接收的数据，必须为字节
    int recvLen; //接收的数据长度
    Thread connectThread; //连接线程

    public Transform Joint1;
    public Transform Joint2;
    public Transform Joint3;
    public Transform Joint4;
    public Transform Joint5;
    public Transform Joint6;
    float j1 = -1.3752307154872732f;
    float j2 = 11.87874489085523f;
    float j3 = 89.38403457660473f;
    float j4 = 179.9999969086381f;
    float j5 = -78.73721831559877f;
    float j6 = -1.3752319188249416f;
    // Start is called before the first frame update
    void Start()
    {
        InitSocket();
        
    }

    void InitSocket()
    {
        //定义侦听端口,侦听任何IP
        ipEnd = new IPEndPoint(IPAddress.Any, 5501);
        //定义套接字类型,在主线程中定义
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //服务端需要绑定ip
        socket.Bind(ipEnd);
        //定义客户端
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        clientEnd = (EndPoint)sender;
        print("waiting for UDP dgram");

        //开启一个线程连接，必须的，否则主线程卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
        print("开始线程！");
    }
    void SocketReceive()
    {
        //进入接收循环
        while (true)
        {
            //对data清零
            recvData = new byte[1024];
            //获取客户端，获取客户端数据，用引用给客户端赋值
            recvLen = socket.ReceiveFrom(recvData, ref clientEnd);
            //print("message from: " + clientEnd.ToString()); //打印客户端信息
                                                            //输出接收到的数据
            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            string[] values = recvStr.Split(' ');
            print("我是服务器，接收到客户端的数据" + values[2]);
            print("我是服务器，接收到客户端的数据" + values[1]);
            j1 = float.Parse(values[0]);
            j2 = float.Parse(values[1]);
            j3 = float.Parse(values[2]);
            j4 = float.Parse(values[3]);
            j5 = float.Parse(values[4]);
            j6 = float.Parse(values[5]);
            /*Joint1.rotation = Quaternion.Euler(0, -j1, 0f);
            Joint2.rotation = Quaternion.Euler(0, -90f, j2-90f);
            Joint3.rotation = Quaternion.Euler(0, 0, j3 + 90f);
            Joint4.rotation = Quaternion.Euler(j4 - 90f, -90f, 0f);
            Joint5.rotation = Quaternion.Euler(-j5, 90f, 0f);
            Joint6.rotation = Quaternion.Euler(0, 0, j6);*/
        }
    }
    void Update()
    {
        /*Joint1.localEulerAngles = new Vector3(0, -j1, 0);
        Joint2.localEulerAngles = new Vector3(0, -90, j2-90);
        Joint3.localEulerAngles = new Vector3(0, 0, j3 + 90);
        Joint4.localEulerAngles = new Vector3(j4 - 90, -90, 0);
        Joint5.localEulerAngles = new Vector3(-j5, 90, 0);
        Joint6.localEulerAngles = new Vector3(0, 0, j6);*/
        Joint1.localEulerAngles = new Vector3(0, -j1, 0);
        Joint2.localEulerAngles = new Vector3(j2, 0, 0);
        Joint3.localEulerAngles = new Vector3(j3, 0, 0);
        Joint4.localEulerAngles = new Vector3(0, -j4 + 180, 0);
        Joint5.localEulerAngles = new Vector3(-j5, 0, 0);
        Joint6.localEulerAngles = new Vector3(0, -j6 - 180, 0);
    }

    //连接关闭
    void SocketQuit()
    {
        //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最后关闭socket
        if (socket != null)
            socket.Close();
        print("disconnect");
    }
    void OnDestroy()
    {
        SocketQuit();
    }
}
