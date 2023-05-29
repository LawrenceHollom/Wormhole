using Godot;
using System.Collections.Generic;

public class Main : Control
{
    private Screen screen;
    private Menu menu;
    private Control texturePairHolder;

    private List<TexturePair> texturePairs;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screen = GetNode<Screen>("Screen");
        menu = GetNode<Menu>("Menu");
        texturePairHolder = GetNode<Control>("TexturePairs");
        
        Input.SetMouseMode(Input.MouseMode.Captured);
        menu.Visible = false;
        screen.SetIsRunning(true);

        texturePairs = new List<TexturePair>();
        foreach (Object o in texturePairHolder.GetChildren())
        {
            if (o is TexturePair)
                texturePairs.Add(o as TexturePair);
        }
        menu.PassTexturePairs(texturePairs);
        screen.SetTexturePair(texturePairs[0]);
    }

    public void OnUpdateTexturePair(int textureNum)
    {
        screen.SetTexturePair(texturePairs[textureNum]);
    }

    public void SetSimulationRunning(bool running)
    {
        menu.Visible = !running;
        screen.SetIsRunning(running);
        if (running)
            Input.SetMouseMode(Input.MouseMode.Captured);
        else
            Input.SetMouseMode(Input.MouseMode.Visible);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("escape"))
        {
            SetSimulationRunning(menu.Visible);
        }

        screen.SetThroatLength(menu.GetThroatLength());
        screen.SetCurvatureRadius(menu.GetCurvatureRadius());
    }
}
