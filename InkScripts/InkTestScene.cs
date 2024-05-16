using Godot;
using GodotInk;

public partial class InkTestScene : VBoxContainer
{
    [Export]
    private InkStory Story;

    public override void _Ready()
    {
        ContinueStory();
    }

    private void ContinueStory()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        Label content = new() { Text = Story.ContinueMaximally() };
        AddChild(content);

        foreach (InkChoice choice in Story.CurrentChoices)
        {
            Button choiceButton = new() { Text = choice.Text };
            choiceButton.Pressed += delegate
            {
                Story.ChooseChoiceIndex(choice.Index);
                ContinueStory();
            };
            AddChild(choiceButton);
        }
    }
}
