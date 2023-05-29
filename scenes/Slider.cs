using Godot;
using System;

[Tool]
public class Slider : HBoxContainer
{
    [Export] private string name;
    [Export] private float minValue, maxValue;
    [Export] private float defaultValue;

    private Label nameLabel, valueLabel;
    private HSlider slider;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        nameLabel = GetNode<Label>("Name");
        valueLabel = GetNode<Label>("Value");
        slider = GetNode<HSlider>("HSlider");

        nameLabel.Text = name;
        slider.MinValue = minValue;
        slider.MaxValue = maxValue;
        slider.Value = defaultValue;
    }

    public float GetValue()
    {
        return (float)slider.Value;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (nameLabel == null)
            _Ready();
        if (Engine.EditorHint)
        {
            nameLabel.Text = name;
            slider.MinValue = minValue;
            slider.MaxValue = maxValue;
            slider.Value = defaultValue;
        }
        valueLabel.Text = string.Format("{0:0.00}", slider.Value);
    }
}
