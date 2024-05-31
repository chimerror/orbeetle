using Godot;
using System;

public partial class DialogueCharacter : TextureRect
{
    private Character _character;
    private string _currentMood = "neutral";

    [Export]
    public string CurrentMood
    {
        get => _currentMood;
        set
        {
            var needUpdate = value != _currentMood;
            _currentMood = value;
            if (needUpdate)
            {
                UpdateTexture();
            }
        }
    }

    [Export]
    public Character Character
    {
        get => _character;
        set
        {
            var needUpdate = value != _character;
            _character = value;
            if (needUpdate)
            {
                UpdateTexture();
            }
        }
    }

    public override void _Ready()
    {
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        if (_character != null)
        {
            Texture = _character.MoodDictionary[_currentMood];
        }
    }
}
