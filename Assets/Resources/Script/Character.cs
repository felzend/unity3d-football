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

    private int Player;
    private int Room;

	void Start () {
        GameCore = GameObject.Find("GameCore");

        this.Player = GameCore.GetComponent<Core>().PlayerId;
        this.Room = GameCore.GetComponent<Core>().Room;                

        Camera = this.gameObject.transform.FindChild("Camera").gameObject;
        Camera.AddComponent<Camera>();
        Camera.GetComponent<Camera>().targetDisplay = 0;
        Camera.GetComponent<Camera>().fieldOfView = 70;
    }    

    void Update()
    {
        Movement();

        /*Vector3 Rotation = (Vector3)CameraData["rotation"];
        Camera.transform.localPosition = new Vector3(0, 4, 0);
        Camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Camera.transform.localScale = (Vector3)CameraData["scale"];

        Dictionary<string, string> Data = new Dictionary<string, string>();
        Data.Add("player", Player.ToString());
        Data.Add("room", Room.ToString());
        Data.Add("x", gameObject.GetComponent<Rigidbody>().position.x.ToString());
        Data.Add("y", gameObject.GetComponent<Rigidbody>().position.y.ToString());
        Data.Add("z", gameObject.GetComponent<Rigidbody>().position.z.ToString());

        Socket.Emit("move_player", new JSONObject(Data));*/

        Debug.Log(gameObject.GetComponent<Rigidbody>().position);
    }

    void Movement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            gameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(
                gameObject.GetComponent<Rigidbody>().transform.forward.x * Speed,
                gameObject.GetComponent<Rigidbody>().position.y,
                gameObject.GetComponent<Rigidbody>().position.z
            ));
        }        
        else if(Input.GetKey(KeyCode.S))
        {
            gameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(
                gameObject.GetComponent<Rigidbody>().transform.forward.x * Speed * -1,
                gameObject.GetComponent<Rigidbody>().position.y,
                gameObject.GetComponent<Rigidbody>().position.z
            ));
        }
        else if(Input.GetKey(KeyCode.A))
        {
            gameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(
                gameObject.GetComponent<Rigidbody>().position.x,
                gameObject.GetComponent<Rigidbody>().position.y,
                gameObject.GetComponent<Rigidbody>().transform.right.z * Speed * -1
            ));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            gameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(
                 gameObject.GetComponent<Rigidbody>().position.x,
                 gameObject.GetComponent<Rigidbody>().position.y,
                 gameObject.GetComponent<Rigidbody>().transform.right.z * Speed
            ));
        }        
    }
}
