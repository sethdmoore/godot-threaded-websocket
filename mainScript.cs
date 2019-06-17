using Godot;
using System;
using System.Text;
using System.Threading;

public class mainScript : Node2D
{
    private chat ch = new chat();
    private Godot.TextEdit wsURI = new Godot.TextEdit();
    private Button btn = new Button();
    private static WebSocketThread websocket;
    private static System.Threading.Thread wsThreadControl;
    private Godot.Error wsThreadError;
    string checkInstanceOf;


    public override void _Ready()
    {
        ch = (chat)GetNode("MarginContainer/VBoxContainer/chat");
        btn = (Button)GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button");

        wsURI = (Godot.TextEdit)GetNode("MarginContainer/VBoxContainer/HBoxContainer/TextEdit");
        ch.Println(wsURI.GetType().ToString());
        ch.Println("Ready");
        checkInstanceOf = "Blahblah";

        // Instantiate websocket with a reference to calling class
        websocket = NewWebSocketThread();
        // this must come after
        wsThreadControl = NewWsThreadControl();

        updateButton();
    }

    private WebSocketThread NewWebSocketThread() {
        WebSocketThread ws;
        ws = new WebSocketThread(GetConnectionURI());
        // wire connections
        ws.Connect("SignalSendMessage", this, nameof(Println));
        ws.Connect("SignalReturnStringPacket", this, nameof(TextPkt));
        // new thread control
        return ws;
    }

    private System.Threading.Thread NewWsThreadControl() {
        return new System.Threading.Thread(new ThreadStart(websocket.Init));
    }

    private void startWs() {
        if (wsThreadControl.IsAlive) {
            // stop our thread
            websocket.StopThread();
            // join the thread
            wsThreadControl.Join();
        }
        // new websocket object (reset)
        websocket = NewWebSocketThread();
        // new thread control
        wsThreadControl = NewWsThreadControl();
        // start the method
        wsThreadControl.Start();
    }

    public string GetConnectionURI() {
        return wsURI.Text;
    }

    private void updateButton() {
        if (websocket.IsPolling) {
            btn.Text = "Disconnect";
        } else {
            btn.Text = "Connect";
        }
    }

    public void Println(string msg) {
        ch.Println(msg);
    }
    
    public void TextPkt(string msg) {
        ch.Println("Text Packet: " + msg);
    }

    private void _on_Button_button_up()
    {
        if (!websocket.IsPolling) {
            ch.ClearDialog();
            startWs();
        } else {
            websocket.StopThread();
            // block until we return
            wsThreadControl.Join();
            wsThreadControl = NewWsThreadControl();
        }
        updateButton();
    }

    public override void _Process(float delta) {
        updateButton();
    }
}
