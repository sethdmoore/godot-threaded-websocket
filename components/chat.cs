using Godot;
using System;

public class chat : ColorRect
{
    RichTextLabel text = new RichTextLabel();

    public override void _Ready()
    {
        text = (RichTextLabel)GetNode("MarginContainer/display");
        ClearDialog();
    }

    public void ClearDialog() {
        text.Text = "";
    }

    public void Println(string msg) {
        text.Text += msg + "\n";
    }
}
