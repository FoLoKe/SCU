using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class RatioPanel : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<RatioPanel, Traits> { }

        [UnityEngine.Scripting.Preserve]
        public class Traits : UxmlTraits
        {
            public UxmlBoolAttributeDescription verticalFit = new UxmlBoolAttributeDescription { name = "verticalFit", defaultValue = true};
            public UxmlBoolAttributeDescription shrinkOpposite = new UxmlBoolAttributeDescription { name = "shrinkOpposite", defaultValue = false };

            public UxmlFloatAttributeDescription ratioWidth = new UxmlFloatAttributeDescription { name = "ratioWidth", defaultValue = 16 };
            public UxmlFloatAttributeDescription ratioHeight = new UxmlFloatAttributeDescription { name = "ratioHeight", defaultValue = 9 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                RatioPanel ele = ve as RatioPanel;

                ele.VerticalFit = verticalFit.GetValueFromBag(bag, cc);
                ele.ShrinkOpposite = shrinkOpposite.GetValueFromBag(bag, cc);
                ele.RatioWidth = ratioWidth.GetValueFromBag(bag, cc);
                ele.RatioHeight = ratioHeight.GetValueFromBag(bag, cc);
            }
        }

        public RatioPanel()
        {
            style.flexDirection = FlexDirection.Row;

            //style.flexShrink = 0;

            style.height = Length.Percent(100);
            style.width = Length.Percent(100);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        }

        public float RatioWidth { get; set; }
        public float RatioHeight { get; set; }

        public bool VerticalFit { 
            get { return verticalFit; } 
            set
            {
                verticalFit = value;
                FitChildren();
            }
        }

        public bool ShrinkOpposite
        {
            get { return shrinkOpposite; }
            set
            {
                shrinkOpposite = value;
                FitChildren();
            }
        }

        private bool verticalFit;
        private bool shrinkOpposite;

        public void FitChildren()
        {
            
            if (float.IsNaN(resolvedStyle.width) || float.IsNaN(resolvedStyle.height))
            {
                return;
            }

            float size;
            if (verticalFit)
            {
                size = resolvedStyle.height < resolvedStyle.width / childCount ? resolvedStyle.height : resolvedStyle.width / childCount;
            }
            else
            {
                size = resolvedStyle.width / childCount < resolvedStyle.height ? resolvedStyle.width / childCount : resolvedStyle.height;
            }

            foreach (VisualElement child in Children())
            {
                child.style.width = Length.Percent(100);
                child.style.height = size;
            }

            if (shrinkOpposite)
            {
                if (verticalFit)
                {
                    style.height = Length.Percent(100);
                    style.width = size * childCount;
                }
                else
                {
                    Debug.Log(size + " " + resolvedStyle.width / childCount + " " + resolvedStyle.height);
                    style.width = Length.Percent(100);
                    style.height = StyleKeyword.Auto; // size * 7;// = //new StyleScale(new Scale(new Vector3(2, 2, 2)));
                }
            }
            else
            {
                style.height = Length.Percent(100);
                style.width = Length.Percent(100);
            }
        }

        private void OnAttachToPanelEvent(AttachToPanelEvent e)
        {
            FitChildren();
        }

        private void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            FitChildren();
            //{
            //    float size = resolvedStyle.width / childCount < resolvedStyle.height ? resolvedStyle.width / childCount : resolvedStyle.height;


            //foreach (VisualElement child in Children())
            //{
            //child.style.width = size;
            //child.style.height = size;
            //}

            //    style.height = size;
            //}

            //if(Axis && )

            

            //foreach (VisualElement child in Children())
            //{
            //    if (Axis)
            //        child.style.width = size;
            //    else
            //        child.style.height = size;
            //}

            //if (float.IsNaN(resolvedStyle.width) || float.IsNaN(resolvedStyle.height))
            //{
            //    return;
            //}

            //if (diff > 0.01f)
            //{
            //   var w = (resolvedStyle.width - (resolvedStyle.height * designRatio)) * 0.5f;
            //    style.marginLeft = w;
            //    style.marginRight = w;
            //}
            //else
            //{
            //    style.marginLeft = 0f;
            //    style.marginRight = 0f;
            //}
        }

        public void UpdateElements()
        {
            if (RatioWidth <= 0.0f || RatioHeight <= 0.0f)
            {
                style.marginLeft = 0f;
                style.marginRight = 0f;
                Debug.LogError($"[AspectRatioPadding] Invalid width:{RatioWidth} or height:{RatioHeight}");
                return;
            }

            if (float.IsNaN(resolvedStyle.width) || float.IsNaN(resolvedStyle.height))
            {
                return;
            }

            float size = resolvedStyle.width / childCount < resolvedStyle.height ? resolvedStyle.width / childCount : resolvedStyle.height;

            foreach (VisualElement child in Children())
            {
                child.style.width = size;
                child.style.height = size;
            }
        }
    }
}
