using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class TabbedPane : VisualElement
    {

        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<TabbedPane, UxmlTraits> { }

        [UnityEngine.Scripting.Preserve]
        public class Traits : UxmlTraits
        {

        }

        public override VisualElement contentContainer
        {
            get
            {
                return tabsContainer;
            }
        }

        private VisualElement tabs;
        private VisualElement tabsContainer;

        private VisualElement currentTab;

        public TabbedPane()
        {
            tabs = new VisualElement();
            tabs.style.height = 40;
            tabs.style.paddingBottom = 0;
            tabs.style.paddingLeft = 0;
            tabs.style.paddingRight = 0;
            tabs.style.paddingTop = 0;
            tabs.style.width = Length.Percent(100);
            tabs.style.flexDirection = FlexDirection.Row;
            tabs.style.justifyContent = Justify.FlexStart;

            tabs.style.flexShrink = 0;
            hierarchy.Add(tabs);

            tabsContainer = new VisualElement();
            tabsContainer.style.flexShrink = 1;
            tabsContainer.style.height = Length.Percent(100);
            tabsContainer.style.width = Length.Percent(100);
            tabsContainer.style.paddingBottom = 0;
            tabsContainer.style.paddingLeft = 0;
            tabsContainer.style.paddingRight = 0;
            tabsContainer.style.paddingTop = 0;
            hierarchy.Add(tabsContainer);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            tabsContainer.Clear();
            tabs.Clear();
        }

        private void OnTabChange(VisualElement toSelect)
        {
            if (currentTab != null)
            {
                currentTab.visible = false;
            }

            toSelect.visible = true;
            currentTab = toSelect;
        }

        public void AddTab(VisualElement visualElement, string tabName)
        {
            Button button = new Button();
            button.text = tabName;
            button.style.fontSize = 20;
            button.style.color = Color.white;
            button.style.height = 32;

            button.style.marginLeft = 0;
            button.style.marginRight = 1;
            button.style.marginTop = 0;
            button.style.marginLeft = 0;

            button.clicked += () => OnTabChange(visualElement);

            tabs.Add(button);

            visualElement.style.height = Length.Percent(100);
            visualElement.style.width = Length.Percent(100);
            visualElement.style.position = Position.Absolute;

            if (tabsContainer.childCount > 0)
            {
                visualElement.visible = false;
            }
            else
            {
                currentTab = visualElement;
            }

            Debug.Log("new Tab: " + visualElement.name);
            tabsContainer.Add(visualElement);
        }
    }
}
