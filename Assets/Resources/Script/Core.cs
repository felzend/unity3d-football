using SocketIO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

public class Core : MonoBehaviour {

    public SocketIOComponent Socket;    
    public int PlayerId;
    public string PlayerName;    
    public int Room = 0;
    public string DefaultTag = "game";
    public int SecondsToStart = 0;

    public List<GameObject> Entities = new List<GameObject>();    

    private Thread GameThread;
    
    void Start ()
    {
        Socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        Socket.On("player_id", RetrievePlayerId);
        Socket.On("room_data", RefreshGame);
        Socket.On("update_ball", BallUpdate);
        Socket.On("enter_room", EnterRoom);
        Socket.On("exit_room", ExitRoom);

        Socket.On("is_pivot", IsPivot);
        Socket.On("update_pivot", IsPivot);

        Socket.On("delete", DeleteHandler);
        Socket.On("message", MessageHandler);
        /*Socket.On("exit_room", ExitedRoom);
        Socket.On("start", Start);
        Socket.On("end", End);        
        Socket.On("respawn", Respawn);
        Socket.On("room_data", RefreshRoom);
        Socket.On("remove_object", RemoveObject);*/

        StartCoroutine(Connect());

        Debug.Log("Socket: "+Socket.IsConnected);
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

        Data.Add("player", PlayerId.ToString());        
    }

    protected void RetrievePlayerId(SocketIOEvent e)
    {
        JSONObject Json = e.data;
        this.PlayerId = (int)Json.GetField("player_id").f;
        this.PlayerName = Json.GetField("player_name").str;        
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

    protected void IsPivot(SocketIOEvent e)
    {        
        GameObject Player = GameObject.Find(PlayerName);
        bool Pivot = e.data.GetField("is_pivot").b;
        Player.GetComponent<Character>().IsPivot = Pivot;
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
            Thread.Sleep(5);

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
        GameObject Player = GameObject.Find(PlayerName);        

        foreach(JSONObject Obj in Json.GetField("entities").list )
        {
            GameObject Object = GameObject.Find(Obj.GetField("name").str);

            // Object data.
            int Id = (int)Obj.GetField("id").f;
            string Type = Obj.GetField("type").str;

            if (Object == null)
            {
                Object = Instantiate(Resources.Load(Obj.GetField("model").str)) as GameObject;
                Object.name = Obj.GetField("name").str;
                Object.tag = DefaultTag;               

                switch (Type)
                {
                    case "ball":
                        {
                            Object.AddComponent<Rigidbody>();
                            Object.AddComponent<CapsuleCollider>();

                            Object.GetComponent<Rigidbody>().mass = 20;
                            Object.GetComponent<CapsuleCollider>().center = new Vector3(0, 0.3f, 0.9f);
                            Object.GetComponent<CapsuleCollider>().radius = 0.8f;
                            Object.GetComponent<CapsuleCollider>().height = 0;

                            Object.AddComponent<Ball>();
                            Object.GetComponent<Ball>().Socket = this.Socket;

                            break;
                        }
                    case "character":
                        {
                            Object.AddComponent<Rigidbody>();
                            Object.AddComponent<BoxCollider>();

                            Object.GetComponent<Rigidbody>().mass = 100;
                            Object.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

                            Object.GetComponent<BoxCollider>().center = new Vector3(-0.1832194f, 1.998269f, 0.3340735f);
                            Object.GetComponent<BoxCollider>().size = new Vector3(1.988663f, 5.999579f, 2.019472f);

                            break;
                        }
                }

                if ( Id == PlayerId ) // Caso seja o personagem do Client, adiciona Script de Character.
                {
                    Dictionary<string, string> Data = new Dictionary<string, string>();
                    GameObject.Find(PlayerName).AddComponent<Character>();
                    GameObject.Find(PlayerName).GetComponent<Character>().Socket = this.Socket;

                    Data.Add("player", PlayerId.ToString());
                    Data.Add("room", Room.ToString());

                    Socket.Emit("is_pivot", new JSONObject(Data));
                }

                // Spawn Point.
                Vector3 SPosition = new Vector3(
                    Obj.GetField("position").GetField("x").f,
                    Obj.GetField("position").GetField("y").f,
                    Obj.GetField("position").GetField("z").f
                );
                Vector3 SRotation = new Vector3(
                    Obj.GetField("rotation").GetField("x").f,
                    Obj.GetField("rotation").GetField("y").f,
                    Obj.GetField("rotation").GetField("z").f
                );
                Vector3 SScale = new Vector3(
                    Obj.GetField("scale").GetField("x").f,
                    Obj.GetField("scale").GetField("y").f,
                    Obj.GetField("scale").GetField("z").f
                );

                Debug.Log("Spawn " + Obj.GetField("type").str + ": " + SPosition);

                Object.GetComponent<Rigidbody>().position = SPosition;
                Object.GetComponent<Rigidbody>().rotation = Quaternion.Euler(SRotation.x, SRotation.y, SRotation.z);
                Object.transform.localScale = SScale;
            }

            if ( ( ! Type.Equals("ball") && PlayerId != Id ) || ( ! Player.GetComponent<Character>().IsPivot && PlayerId != Id ) )
            {
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

                Object.GetComponent<Rigidbody>().position = Position;
                Object.GetComponent<Rigidbody>().rotation = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
                Object.transform.localScale = Scale;
            }
        }
    }    

    private void BallUpdate(SocketIOEvent e)
    {
        /*string ObjName = e.data.GetField("name").str;
        GameObject Obj = GameObject.Find(ObjName);
        if (Obj == null) return;

        Vector3 Position = new Vector3(
            e.data.GetField("position").GetField("x").f, 
            e.data.GetField("position").GetField("y").f, 
            e.data.GetField("position").GetField("z").f
        );
        Quaternion Rotation = Quaternion.Euler(
            e.data.GetField("rotation").GetField("x").f,
            e.data.GetField("rotation").GetField("y").f,
            e.data.GetField("rotation").GetField("z").f
        );

        Obj.transform.position = Position;
        Obj.transform.rotation = Rotation;*/
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
