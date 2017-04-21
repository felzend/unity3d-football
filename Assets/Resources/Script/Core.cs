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
        
    private Thread GameThread;
    private Thread WFSThread;
    
    void Start ()
    {
        Socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        Socket.On("player_id", RetrievePlayerId);
        Socket.On("room_data", RefreshGame);
        Socket.On("enter_room", EnterRoom);
        Socket.On("exit_room", ExitRoom);

        Socket.On("delete", DeleteHandler);
        Socket.On("message", MessageHandler);
        /*Socket.On("exit_room", ExitedRoom);
        Socket.On("start", Start);
        Socket.On("end", End);        
        Socket.On("respawn", Respawn);
        Socket.On("room_data", RefreshRoom);
        Socket.On("remove_object", RemoveObject);*/

        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        Dictionary<string, string> Data = new Dictionary<string, string>();
        Data.Add("room", "25");
        yield return new WaitForSeconds(0.1f);

        GameThread = new Thread(Game_Thread);
        GameThread.Start();

        Socket.Emit("player_id");
        yield return new WaitForSeconds(0.3f);
        Socket.Emit("enter_room", new JSONObject(Data));
    }

    protected void RetrievePlayerId(SocketIOEvent e)
    {
        JSONObject Json = e.data;
        this.PlayerId = (int)Json.GetField("player_id").f;
    }

    protected void EnterRoom(SocketIOEvent e)
    {
        JSONObject Json = e.data;
        int Room = (int)e.data["room_id"].f;
        this.Room = Room;

        Debug.Log("Entered Room " + Room);
    }

    protected void ExitRoom(SocketIOEvent e)
    {
        this.PlayerId = -1;
        this.Room = -1;
    }

    protected void MessageHandler(SocketIOEvent e)
    {
        JSONObject Json = e.data;
        Debug.Log(Json.GetField("message").str);
    }
    
    protected void DeleteHandler(SocketIOEvent e)
    {
        GameObject Object = GameObject.Find(e.data.GetField("object").str);
        if ( Object != null)
        {
            GameObject.DestroyImmediate(Object);
        }
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

    protected void Game_Thread()
    {
        while (true)
        {
            Thread.Sleep(100);

            if (PlayerId > 0 && Room > 0 )
            {
                Dictionary<string, string> Data = new Dictionary<string, string>();
                Data.Add("room", this.Room.ToString());
                Socket.Emit("room_data", new JSONObject(Data));                
            }
        }
    }

    protected void RefreshGame(SocketIOEvent e)
    {
        JSONObject Json = e.data.GetField("data");

        foreach(JSONObject Obj in Json.GetField("entities").list )
        {
            GameObject Object = GameObject.Find(Obj.GetField("name").str);
            if( Object == null)
            {
                Object = Instantiate(Resources.Load(Obj.GetField("model").str)) as GameObject;
                Object.name = Obj.GetField("name").str;
                Object.tag = DefaultTag;
            }

            Vector3 Position = new Vector3(
                Obj.GetField("position").GetField("x").f,
                Obj.GetField("position").GetField("y").f,
                Obj.GetField("position").GetField("z").f
            );
            Vector3 Rotation = new Vector3(
                Obj.GetField("rotation").GetField("x").f,
                Obj.GetField("rotation").GetField("y").f,
                Obj.GetField("rotation").GetField("z").f
            );
            Vector3 Scale = new Vector3(
                Obj.GetField("scale").GetField("x").f,
                Obj.GetField("scale").GetField("y").f,
                Obj.GetField("scale").GetField("z").f
            );

            Object.transform.position = Position;
            Object.transform.localRotation = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
            Object.transform.localScale = Scale;
        }
    }    

    void Awake()
    {
		Application.runInBackground = true;
        DontDestroyOnLoad(transform.gameObject);       
    }

    void OnApplicationQuit()
    {
        this.GameThread.Abort();
    }

    // Update is called once per frame
    void Update ()
    {
        
	}    

    protected void RetrieveId(SocketIOEvent e)
    {
        this.PlayerId = (int)e.data["id"].f;
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

    protected void RemoveObject(SocketIOEvent e)
    {
        GameObject Obj = GameObject.Find(e.data.GetField("name").str);
        if (Obj != null)
        {
            GameObject.DestroyImmediate(Obj);
        }
    }   

    protected void Respawn(SocketIOEvent e)
    {

    }
}
