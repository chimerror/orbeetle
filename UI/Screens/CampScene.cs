using Godot;
using System.Collections.Generic;

public partial class CampScene : Node2D
{
    private DialogueDisplay _dialogueDisplay;
    private CampPlayer _campPlayer;
    private List<CampNpc> _campNpcs;

    public override void _Ready()
    {
        _dialogueDisplay = GetNode<DialogueDisplay>("%DialogueDisplay");
        _campPlayer = GetNode<CampPlayer>("%CampPlayer");
        _dialogueDisplay.DialogueStarted += _campPlayer.OnDialogueStarted;
        _dialogueDisplay.DialogueFinished += _campPlayer.OnDialogueFinished;
        _dialogueDisplay.ProcessMode = ProcessModeEnum.Disabled;
        _campNpcs = this.GetChildrenOfType<CampNpc>();
        foreach (var campNpc in _campNpcs)
        {
            campNpc.dialogueDisplay = _dialogueDisplay;
            _dialogueDisplay.DialogueStarted += campNpc.OnDialogueStarted;
            _dialogueDisplay.DialogueFinished += campNpc.OnDialogueFinished;
        }
    }
}
