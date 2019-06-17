using Godot;
using System;
using System.Text;

public class WebSocketThread : Node {
    private WebSocketClient client = new WebSocketClient();
    private mainScript main;
    public bool IsPolling {get; set;}
    private string foo;
    private int constructorCount = 0;

    private static NetworkedMultiplayerPeer.ConnectionStatus lastConnStatus;

    // Constructor
    public WebSocketThread(mainScript m) {
        client.Connect("connection_established", this, nameof(SignalClientConnected));
        client.Connect("connection_error", this, nameof(SignalClientError));
        client.Connect("connection_closed", this, nameof(SignalClientClosed));
        client.Connect("server_close_request", this, nameof(SignalServerCloseRequest));
        client.Connect("data_received", this, nameof(SignalPacketReceived));

        main = m;
        constructorCount += 1;
        lastConnStatus = Godot.NetworkedMultiplayerPeer.ConnectionStatus.Disconnected;
        GD.Print(string.Format("TEST {0}", constructorCount));
    }

    // signal callbacks
    private void SignalClientConnected(string msg) {
        main.Println("Thread: INFO: Client connected, connection status: " + msg);
    }

    private void SignalServerCloseRequest() {
        main.Println("Thread: WARN: Closing connection by request of server");
        StopThread();
    }

    private void SignalClientClosed(bool cleanClose) {
        main.Println("Thread: INFO: Connection Closed.");
        StopThread();
    }

    private void SignalClientError() {
        main.Println("Thread: Warn: Connection error.");
        StopThread();
    }

    public void StopThread() {
        IsPolling = false;
    }

    private void SignalPacketReceived() {
        Encoding toText = Encoding.UTF8;
        Boolean isString = false;
        byte[] packet;

        try {
            packet = client.GetPeer(1).GetPacket(); 
        } catch (Exception e) {
            main.Println(String.Format("Thread: ERROR: Could not GetPacket(): {0}", e.ToString()));
            return;
        }

        // not sure if this is necessary, as WasStringPacket returns a bool
        try {
            isString = client.GetPeer(1).WasStringPacket();
        } catch (Exception e) {
            main.Println(String.Format("Thread: ERROR: Couldn't tell type of packet: {0}", e.ToString()));
        }

        if (isString) {
            string msg;
            msg = toText.GetString(packet);
            main.Println(msg);
        }
    }

    public void Init() {
        Godot.NetworkedMultiplayerPeer.ConnectionStatus status;

        var err = ConnectToWs();
        if (err != Godot.Error.Ok) {
            main.Println(String.Format("Thread: ERROR: {0}",  err));
            main.Println("Thread: stopped");
            return;
        }

        IsPolling = true;
        main.CallBack();

        while (IsPolling) {
            //main.Println("wat");
            try {
                status = client.GetConnectionStatus();
            } catch (Exception e) {
                main.Println(String.Format("ERROR: Unhandled exception when calling GetConnectionStatus(): {0}", e.ToString()));
                main.Println("THREAD: OK EXITING");
                StopThread();
                break;
            }

            if (status != lastConnStatus) {
                lastConnStatus = status;
                main.Println("INFO: ConnectionStatus Change: " + Enum.GetName(typeof(WebSocketClient.ConnectionStatus), status));
            }

            try {
                client.Poll();
            } catch (Exception e) {
                main.Println(String.Format("ERROR: Exception polling ws: {0}", e.ToString()));
                StopThread();
                break;
            }
        }

        main.isConnected = false;
        main.Println("Thread: Stopped");
    }

	public Error ConnectToWs() {
        Error connectErr;
        string uri;

        IsPolling = true;
        main.CallBack();

        uri = main.GetConnectionURI();
        main.Println(String.Format("Thread: INFO: Connecting to {0}", uri));
        try {
            connectErr = client.ConnectToUrl(uri, null, false);
        } catch (Exception e) {
            main.Println(String.Format("Thread: ERROR: Could not connect to chat!: {0}", e.ToString()));
            return Error.Failed;
        }

        if (connectErr != Error.Ok) {
            main.Println("Thread: ERROR: Connection error: " + connectErr);
            return connectErr;
        }
		return Error.Ok;
    }
}
