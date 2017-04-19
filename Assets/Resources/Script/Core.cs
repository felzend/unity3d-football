using SocketIO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Core : MonoBehaviour {

    public SocketIOComponent Socket;
    public int PlayerId;
    public string PlayerName;    
    public int Room = 0;
    public string DefaultTag = "game";
    public int SecondsToStart = 0;

    public List<GameObject> Entities = new List<GameObject>();
        
    private Thread UpdateThread;
    private Thread WFSThread;
    
    void Start ()
    {
        Socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        Socket.On("retrieve_id", RetrieveId);
        Socket.On("entered_room", EnteredRoom);
        Socket.On("exit_room", ExitedRoom);
        Socket.On("start", Start);
        Socket.On("end", End);
        Socket.On("spawn", Spawn);
        Socket.On("respawn", Respawn);
        Socket.On("room_data", RefreshRoom);
        Socket.On("remove_object", RemoveObject);

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

    private void GameUpdate()
    {
        while (true)
        {
            Thread.Sleep(100);
            if( Room != 0 )
            {
                Dictionary<string, string> Data = new Dictionary<string, string>();
                Data.Add("room", this.Room.ToString());

                Socket.Emit("room_data", new JSONObject(Data));
            }
        }
    }
    
    protected void RefreshRoom(SocketIOEvent e)
    {
        JSONObject Json = e.data.GetField("json");

        foreach(JSONObject JO in Json.list)
        {
            string ObjName = JO.GetField("name").str;
            Vector3 Position = new Vector3(
                   JO.GetField("defaults").GetField("position").GetField("x").f,
                   JO.GetField("defaults").GetField("position").GetField("y").f,
                   JO.GetField("defaults").GetField("position").GetField("z").f
            );
            Vector3 Rotation = new Vector3(
                JO.GetField("defaults").GetField("rotation").GetField("x").f,
                JO.GetField("defaults").GetField("rotation").GetField("y").f,
                JO.GetField("defaults").GetField("rotation").GetField("z").f
            );
            Vector3 Scale = new Vector3(
                JO.GetField("defaults").GetField("scale").GetField("x").f,
                JO.GetField("defaults").GetField("scale").GetField("y").f,
                JO.GetField("defaults").GetField("scale").GetField("z").f
            );

            GameObject Obj = GameObject.Find(ObjName);        
            if( Obj == null )
            {
                Obj = Instantiate(Resources.Load(JO.GetField("model").str)) as GameObject;
            }

            Obj.name = ObjName;
            Obj.tag = DefaultTag;
            Obj.transform.position = Position;
            Obj.transform.localRotation = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
            Obj.transform.localScale = Scale;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        UpdateThread = new Thread(GameUpdate);
        UpdateThread.Start();
    }

    void OnApplicationQuit()
    {
        if(this.UpdateThread != null && this.UpdateThread.IsAlive) this.UpdateThread.Abort();
    }

    // Update is called once per frame
    void Update ()
    {
        
	}    

    protected void RetrieveId(SocketIOEvent e)
    {
        this.PlayerId = (int)e.data["id"].f;
    }

    protected void EnteredRoom(SocketIOEvent e)
    {
        JSONObject Json = e.data;
        int Room = (int)e.data["id"].f;

        switch(Room)
        {
            case -1: Debug.Log("Requested Room does not exists. Returning to the Lobby."); return;
            case 0: Debug.Log("Request Room is full. Returning to the Lobby."); return;
        }       

        this.Room = Room;
        //this.RoomThread = new Thread(RoomUpdate);        
        //this.RoomThread.Start();

        StartCoroutine(RequestSpawn());

        Debug.Log("Entered Room " + Room);
     }

    protected void ExitedRoom(SocketIOEvent e)
    {
        this.UpdateThread.Abort();
    }
    
    protected void WaitForStart()
    {
        while(SecondsToStart > 0)
        {
            Debug.Log("Starting match in " + SecondsToStart);
            Thread.Sleep(1000);
            --SecondsToStart;
        }
    }

    protected void Start(SocketIOEvent e)
    {
        JSONObject Json = e.data;
        this.WFSThread = new Thread(WaitForStart);
        this.SecondsToStart = (int)Json.GetField("seconds").f;
        this.WFSThread.Start();
    }

    protected void End(SocketIOEvent e)
    {
        Debug.Log("The match is over on Room "+this.Room);
    }

    IEnumerator RequestSpawn()
    {
        yield return new WaitForSeconds(1f);
        Dictionary<string, string> D = new Dictionary<string, string>();
        D.Add("player", this.PlayerId.ToString());
        D.Add("room", this.Room.ToString());

        Socket.Emit("spawn", new JSONObject(D));
    }

    protected void RemoveObject(SocketIOEvent e)
    {
        GameObject Obj = GameObject.Find(e.data.GetField("name").str);
        if (Obj != null)
        {
            GameObject.DestroyImmediate(Obj);
        }
    }

    protected void Spawn(SocketIOEvent e)
    {
        JSONObject Json = e.data.GetField("json");

        foreach (JSONObject Obj in Json.list)
        {
            Vector3 Position = new Vector3(
                Obj.GetField("defaults").GetField("position").GetField("x").f,
                Obj.GetField("defaults").GetField("position").GetField("y").f,
                Obj.GetField("defaults").GetField("position").GetField("z").f
            );
            Vector3 Rotation = new Vector3(
                Obj.GetField("defaults").GetField("rotation").GetField("x").f,
                Obj.GetField("defaults").GetField("rotation").GetField("y").f,
                Obj.GetField("defaults").GetField("rotation").GetField("z").f
            );
            Vector3 Scale = new Vector3(
                Obj.GetField("defaults").GetField("scale").GetField("x").f,
                Obj.GetField("defaults").GetField("scale").GetField("y").f,
                Obj.GetField("defaults").GetField("scale").GetField("z").f
            );

            GameObject G = Instantiate(Resources.Load(Obj.GetField("model").str)) as GameObject;
            G.tag = this.DefaultTag;
            G.name = Obj.GetField("name").str;
            G.transform.position = Position;
            G.transform.localRotation = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
            G.transform.localScale = Scale;            
        }

        UpdateThread = new Thread(GameUpdate);
        UpdateThread.Start();
    }

    protected void Respawn(SocketIOEvent e)
    {

    }
}
