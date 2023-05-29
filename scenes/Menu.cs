using Godot;
using System.Collections.Generic;

public class Menu : Control
{
    [Signal] private delegate void ContinueSimulation();
    [Signal] private delegate void UpdateTexturePair(int textureNum);
    [Signal] private delegate void SetClickToLookStatus(bool clickToLook);

    private Slider throatSlider, radiusSlider;
    private Label textureLabel;
    private Button clickToLookButton;

    private List<TexturePair> texturePairs;
    private int textureNum;

    public override void _Ready()
    {
        throatSlider = GetNode<Slider>("VBoxContainer/ThroatSlider");
        radiusSlider = GetNode<Slider>("VBoxContainer/RadiusSlider");
        textureLabel = GetNode<Label>("VBoxContainer/HBoxContainer3/TextureLabel");
        clickToLookButton = GetNode<Button>("VBoxContainer/HBoxContainer4/ClickButton");
    }

    public void PassTexturePairs(List<TexturePair> texturePairs)
    {
        this.texturePairs = texturePairs;
        textureNum = 0;
    }

    private void UpdateTextureLabel()
    {
        textureLabel.Text = "Texture: " + texturePairs[textureNum].Name;
        EmitSignal(nameof(UpdateTexturePair), textureNum);
    }

    public void OnNextTexture()
    {
        textureNum = (textureNum + 1) % texturePairs.Count;
        UpdateTextureLabel();
    }

    public void OnPrevTexture()
    {
        textureNum = (textureNum + texturePairs.Count - 1) % texturePairs.Count;
        UpdateTextureLabel();
    }

    public void OnToggleClickToLook()
    {
        clickToLookButton.Text = clickToLookButton.Pressed ? "True" : "False";
        EmitSignal(nameof(SetClickToLookStatus), clickToLookButton.Pressed);
    }

    public void OnContinueButtonPressed()
    {
        EmitSignal(nameof(ContinueSimulation));
    }

    public void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

    public float GetThroatLength()
    {
        return throatSlider.GetValue();
    }

    public float GetCurvatureRadius()
    {
        return radiusSlider.GetValue();
    }
}
