using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using SocketIO;

public class Character : MonoBehaviour {

    public GameObject GameCore;
    public GameObject Camera;
    public SocketIOComponent Socket;
    public Dictionary<string, System.Object> CameraData = new Dictionary<string, System.Object>();

    public float Speed = 0.6f;
    public float RotationSpeed = 1.5f;

    private int Player;
    private int Room;
    private Rigidbody Rb;

	void Start () {
        GameCore = GameObject.Find("GameCore");

        this.Player = GameCore.GetComponent<Core>().PlayerId;
        this.Room = GameCore.GetComponent<Core>().Room;
        this.Rb = gameObject.GetComponent<Rigidbody>();

        Camera = this.gameObject.transform.FindChild("Camera").gameObject;
        Camera.AddComponent<Camera>();
        Camera.GetComponent<Camera>().targetDisplay = 0;
        Camera.GetComponent<Camera>().fieldOfView = 70;

        Camera.transform.localPosition = new Vector3(0, 2, 0);
        Camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Camera.transform.localScale = new Vector3(0, 0, 0);
    }    

    void Update()
    {
        Movement();

        Dictionary<string, string> Data = new Dictionary<string, string>();
        Data.Add("player", Player.ToString());
        Data.Add("room", Room.ToString());

        Data.Add("pos_x", gameObject.transform.position.x.ToString());
        Data.Add("pos_y", gameObject.transform.position.y.ToString());
        Data.Add("pos_z", gameObject.transform.position.z.ToString());

        Data.Add("rot_x", gameObject.transform.rotation.eulerAngles.x.ToString());
        Data.Add("rot_y", gameObject.transform.rotation.eulerAngles.y.ToString());
        Data.Add("rot_z", gameObject.transform.rotation.eulerAngles.z.ToString());

        Socket.Emit("update_player", new JSONObject(Data));
    }

    void Movement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            gameObject.transform.position = new Vector3((gameObject.transform.forward.x * Speed) + gameObject.transform.position.x, (gameObject.transform.forward.y * Speed) + gameObject.transform.position.y, (gameObject.transform.forward.z * Speed) + gameObject.transform.position.z);            
        }

        else if (Input.GetKey(KeyCode.S))
        {
            gameObject.transform.position = new Vector3((gameObject.transform.forward.x * Speed * -1) + gameObject.transform.position.x, (gameObject.transform.forward.y * Speed * -1) + gameObject.transform.position.y, (gameObject.transform.forward.z * Speed * -1) + gameObject.transform.position.z);
        }

        else if (Input.GetKey(KeyCode.A))
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, ( gameObject.transform.right.z * Speed * -1 ) + gameObject.transform.position.z);
        }

        else if (Input.GetKey(KeyCode.D))
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, (gameObject.transform.right.z * Speed) + gameObject.transform.position.z);
        }

        if( Input.GetKey(KeyCode.LeftArrow) )
        {
            gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y + (RotationSpeed * -1), gameObject.transform.rotation.eulerAngles.z);
        }

        else if (Input.GetKey(KeyCode.RightArrow))
        {
            gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y + (RotationSpeed), gameObject.transform.rotation.eulerAngles.z);
        }
    }
}
