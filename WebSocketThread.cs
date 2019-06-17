using Godot;
using System;
using System.Text;

public class WebSocketThread : Node {
    private WebSocketClient client = new WebSocketClient();
    public bool IsPolling {get; set;}
    private string wsUri;

    private static NetworkedMultiplayerPeer.ConnectionStatus lastConnStatus;

    // This signal is used to print out to the parent thread
    [Signal]
    public delegate string SignalSendMessage();

    [Signal]
    public delegate string SignalReturnStringPacket();

    [Signal]
    public delegate byte[] SignalReturnBytePacket();

    // Constructor
    public WebSocketThread(string uri) {
        client.Connect("connection_established", this, nameof(SignalClientConnected));
        client.Connect("connection_error", this, nameof(SignalClientError));
        client.Connect("connection_closed", this, nameof(SignalClientClosed));
        client.Connect("server_close_request", this, nameof(SignalServerCloseRequest));
        client.Connect("data_received", this, nameof(SignalPacketReceived));

        lastConnStatus = Godot.NetworkedMultiplayerPeer.ConnectionStatus.Disconnected;
        wsUri = uri;
    }

    // signal callbacks
    private void SignalClientConnected(string msg) {
        EmitSignal(nameof(SignalSendMessage), "INFO: Client connected, connection status: " + msg);
    }

    private void SignalServerCloseRequest() {
        EmitSignal(nameof(SignalSendMessage), "WARN: Closing connection by request of server");
        StopThread();
    }

    private void SignalClientClosed(bool cleanClose) {
        EmitSignal(nameof(SignalSendMessage), "INFO: Connection Closed.");
        StopThread();
    }

    private void SignalClientError() {
        EmitSignal(nameof(SignalSendMessage), "Warn: Connection error.");
        StopThread();
    }

    public void StopThread() {
        IsPolling = false;
    }

    private void SignalPacketReceived() {
        Encoding toText = Encoding.UTF8;
        byte[] packet;

        try {
            packet = client.GetPeer(1).GetPacket();
        } catch (Exception e) {
            EmitSignal(nameof(SignalSendMessage), String.Format("ERROR: Could not GetPacket(): {0}", e.ToString()));
            return;
        }

        // not sure if this try is necessary, as WasStringPacket returns a bool
        try {
            if (client.GetPeer(1).WasStringPacket()) {
                string msg;
                msg = toText.GetString(packet);
                EmitSignal(nameof(SignalReturnStringPacket), msg);
            } else {
                EmitSignal(nameof(SignalReturnBytePacket), packet);
            }
        } catch (Exception e) {
            EmitSignal(nameof(SignalSendMessage), String.Format("ERROR: Couldn't tell type of packet: {0}", e.ToString()));
        }

    }

    public void Init() {
        Godot.NetworkedMultiplayerPeer.ConnectionStatus status;

        var err = ConnectToWs();
        if (err != Godot.Error.Ok) {
            EmitSignal(nameof(SignalSendMessage), String.Format("ERROR: Init: {0} -- stopped",  err));
            return;
        }

        IsPolling = true;

        while (IsPolling) {
            //SignalSendMessage("wat");
            try {
                status = client.GetConnectionStatus();
            } catch (Exception e) {
                EmitSignal(nameof(SignalSendMessage),String.Format("ERROR: Unhandled exception when calling GetConnectionStatus(): {0}", e.ToString()));
                EmitSignal(nameof(SignalSendMessage),"THREAD: OK EXITING");
                StopThread();
                break;
            }

            if (status != lastConnStatus) {
                lastConnStatus = status;
                EmitSignal(nameof(SignalSendMessage),"INFO: ConnectionStatus Change: " + Enum.GetName(typeof(WebSocketClient.ConnectionStatus), status));
            }

            try {
                client.Poll();
            } catch (Exception e) {
                EmitSignal(nameof(SignalSendMessage),String.Format("ERROR: Exception polling ws: {0}", e.ToString()));
                StopThread();
                break;
            }
        }

        EmitSignal(nameof(SignalSendMessage),"Stopped");
    }

	private Error ConnectToWs() {
        Error connectErr;

        IsPolling = true;

        EmitSignal(nameof(SignalSendMessage),String.Format("INFO: Connecting to {0}", wsUri));
        try {
            connectErr = client.ConnectToUrl(wsUri, null, false);
        } catch (Exception e) {
            EmitSignal(nameof(SignalSendMessage),String.Format("ERROR: Could not connect to chat!: {0}", e.ToString()));
            return Error.Failed;
        }

        if (connectErr != Error.Ok) {
            EmitSignal(nameof(SignalSendMessage),"ERROR: Connection error: " + connectErr);
            return connectErr;
        }
		return Error.Ok;
    }
}
