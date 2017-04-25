using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SocketIO;

public class Ball : MonoBehaviour {

    private Rigidbody Rb;
    private GameObject GameCore;

    public int Room;
    public SocketIOComponent Socket;   

	// Use this for initialization
	void Start () {
        GameCore = GameObject.Find("GameCore");

        this.Room = GameCore.GetComponent<Core>().Room;
        this.Rb = gameObject.GetComponent<Rigidbody>();

        StartCoroutine(BallUpdate());
	}

    IEnumerator BallUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds((25f / 1000f));
            GameObject Obj = GameObject.Find(GameCore.GetComponent<Core>().PlayerName);

            if (Obj != null)
            {
                if (  Obj.GetComponent<Character>().IsPivot)
                {
                    Vector3 Speed = Rb.velocity;
                    Vector3 AngularSpeed = Rb.angularVelocity;

                    Dictionary<string, string> Data = new Dictionary<string, string>();
                    Data.Add("room", this.Room.ToString());
                    Data.Add("except_player", this.GameCore.GetComponent<Core>().PlayerId.ToString());

                    Data.Add("pos_x", gameObject.transform.position.x.ToString());
                    Data.Add("pos_y", gameObject.transform.position.y.ToString());
                    Data.Add("pos_z", gameObject.transform.position.z.ToString());

                    Data.Add("rot_x", gameObject.transform.rotation.eulerAngles.x.ToString());
                    Data.Add("rot_y", gameObject.transform.rotation.eulerAngles.y.ToString());
                    Data.Add("rot_z", gameObject.transform.rotation.eulerAngles.z.ToString());

                    Socket.Emit("update_ball", new JSONObject(Data));
                }                
            }
        }
    }        

    // Update is called once per frame
    void Update ()
    {
        
    }    
}
