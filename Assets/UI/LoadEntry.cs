using Assets.BlueprintUtils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class LoadEntry : VisualElement
    {
        public Blueprint blueprint;

        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<LoadEntry, UxmlTraits> { }

        [UnityEngine.Scripting.Preserve]
        public class Traits : UxmlTraits
        {

        }

        public Texture2D texture;

        public LoadEntry()
        {
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
                texture = null;
            }
        }

        public LoadEntry(Blueprint blueprint) : this()
        {
            this.blueprint = blueprint;
            style.width = Length.Percent(100);
            style.flexDirection = FlexDirection.Row;
            style.justifyContent = Justify.FlexStart;
            style.borderBottomWidth = 5;
            style.borderRightWidth = 5;
            style.borderLeftWidth = 5;
            style.borderTopWidth = 5;

            var icon = new VisualElement();
            icon.style.width = 96;
            icon.style.height = 96;

            texture = blueprint.GetTexture();
            icon.style.backgroundImage = texture;

            Add(icon);

            var descriptionContainer = new VisualElement();

            var name = new Label(blueprint.Name);
            name.style.color = Color.white;
            name.style.fontSize = 20;

            var stats = new Label(blueprint.sizeX + " " + blueprint.sizeY); // TODO: GAME REALTED STATS
            stats.style.color = Color.white;
            stats.style.fontSize = 20;

            descriptionContainer.Add(name);
            descriptionContainer.Add(stats);

            Add(descriptionContainer);
        }
    }
}