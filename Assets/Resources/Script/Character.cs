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

        }

        else if (Input.GetKey(KeyCode.S))
        {

        }

        else if (Input.GetKey(KeyCode.A))
        {

        }

        else if (Input.GetKey(KeyCode.D))
        {

        }

        /*if (Input.GetKey(KeyCode.W))
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
        }*/
    }
}
