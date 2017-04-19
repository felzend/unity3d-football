using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour {

    public SocketIOComponent Socket;
    public int PlayerId;
    public int Room;

    void Start ()
    {
        Socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        Socket.On("retrieve_id", RetrieveId);
        Socket.On("entered_room", EnteredRoom);

        StartCoroutine(SocketHandler());        
    }

    IEnumerator SocketHandler()
    {
        Dictionary<string, string> Room = new Dictionary<string, string>();
        Room["id"] = "25";

        yield return new WaitForSeconds(0.1f);
        Socket.Emit("request_id");
        yield return new WaitForSeconds(0.1f);
        Socket.Emit("enter_room", new JSONObject(Room));
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    protected void CheckRooms()
    {

    }

    protected void RetrieveId(SocketIOEvent e)
    {
        this.PlayerId = (int)e.data["id"].f;
    }

    protected void EnteredRoom(SocketIOEvent e)
    {
        JSONObject Json = e.data;

        if (e.data["error"] != null) Debug.Log(e.data["error"].str);
        else
        {
            int Room = (int)e.data["id"].f;
            this.Room = Room;
            Debug.Log("Entered Room " + Room);
        }
    }
}
