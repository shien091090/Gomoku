using UnityEditor;
using UnityEngine.UIElements;

public class ExampleWindow : EditorWindow
{
    public void OnEnable()
    {
        var root = this.rootVisualElement;
        Slider slider = new Slider();
        root.Add(slider);              
    }
}