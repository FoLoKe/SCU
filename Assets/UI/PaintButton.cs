using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class PaintButton : Button
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<PaintButton, UxmlTraits> { }

        [UnityEngine.Scripting.Preserve]
        public class Traits : UxmlTraits
        {
            public UxmlFloatAttributeDescription r = new UxmlFloatAttributeDescription { name = "r", defaultValue = 0f };
            public UxmlFloatAttributeDescription g = new UxmlFloatAttributeDescription { name = "g", defaultValue = 255f };
            public UxmlFloatAttributeDescription b = new UxmlFloatAttributeDescription { name = "b", defaultValue = 0f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                PaintButton ele = ve as PaintButton;

                ele.R = r.GetValueFromBag(bag, cc);
                ele.G = g.GetValueFromBag(bag, cc);
                ele.B = b.GetValueFromBag(bag, cc);
            }
        }
        
        public float R 
        { 
            get { return color.g; }
            set 
            { 
                color.g = value;
                UpdateColor();
            }
        }

        public float G
        {
            get { return color.g; }
            set 
            { 
                color.g = value;
                UpdateColor();
            }
        }

        public float B
        {
            get { return color.b; }
            set 
            { 
                color.b = value;
                UpdateColor();
            }
        }

        public Color Color
        {
            get { return color;}
            set
            {
                color = value;
                UpdateColor();
            }
        }

        private Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        private VisualElement colorDisplay;

        public PaintButton()
        {
            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;
            style.paddingBottom = 4;
            style.paddingLeft = 4;
            style.paddingRight = 4;
            style.paddingTop = 4;
            style.width = 48;
            style.height = 48;

            style.backgroundColor = new Color(0, 0, 0, 0);

            colorDisplay = new VisualElement();
            colorDisplay.style.width = Length.Percent(100);
            colorDisplay.style.height = Length.Percent(100);
            colorDisplay.style.backgroundImage = new Texture2D(1, 1);

            hierarchy.Add(colorDisplay);

            focusable = false;

            UpdateColor();
        }

        private void UpdateColor()
        {
            colorDisplay.style.backgroundImage.value.texture.SetPixel(0, 0, color);
            colorDisplay.style.backgroundImage.value.texture.Apply();
        }
    }
}
