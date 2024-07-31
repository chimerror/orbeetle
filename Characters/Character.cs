using Godot;
using GColl = Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Character : Resource
{
    private string _shortName;

    public Dictionary<string, Texture2D> MoodDictionary
    {
        get => Moods.Zip(MoodTextures).ToDictionary(g => g.First, g => g.Second);
    }

    [Export]
    public string ShortName
    {
        get => _shortName;
        set => _shortName = value.ToLowerInvariant();
    }

    [Export]
    public string FullDialogueName { get; set; }

    [Export]
    public GColl.Array<string> Moods { get; set; }

    [Export]
    public GColl.Array<Texture2D> MoodTextures { get; set; }

    [Export]
    public Texture2D CampTexture { get; set; }
}
