using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace Game.UI
{
	public class ColorPicker : VisualElement
	{
		[UnityEngine.Scripting.Preserve]
		public new class UxmlFactory : UxmlFactory<ColorPicker, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield return new UxmlChildElementDescription(typeof(VisualElement)); }
			}
		}


		private float H, S, V = 1.0f;

		private Slider rSlider;
		private Slider gSlider;
		private Slider bSlider;
		private Slider2D gradientSlider;
		private Slider hueSlider;
		private VisualElement gradientSliderDragger;
		private VisualElement hueSliderDragger;

		private VisualElement controlsContainter;

		private Texture2D gradientTexture;
		private Texture2D hueSliderTexture;
		
		private const string pickerStylesResource = "ColorPickerStyleSheet";
		private const string ussPickerClassName = "color-picker";
		private const string ussContentBack = ussPickerClassName + "__content-area";
		private const string ussRSlider = ussPickerClassName + "__red-slider";
		private const string ussGSlider = ussPickerClassName + "__green-slider";
		private const string ussBSlider = ussPickerClassName + "__blue-slider";
		private const string ussGradientArea = ussPickerClassName + "__gradient-area";
		private const string ussGradientSlider = ussPickerClassName + "__gradient-slider";
		private const string ussHueSlider = ussPickerClassName + "__hue-slider";

		public UnityEvent colorChangeEvent;

		public Color Color 
		{ 
			get 
			{
				var c = Color.HSVToRGB(H, S, V);
				c.a = 255;
				return c;
			} 
			set 
			{
				Color.RGBToHSV(value, out H, out S, out V);
				OnColorChanged(true, true);
			}
		}

		public override VisualElement contentContainer
		{
			get
			{
				return controlsContainter;
			}
		}

		public ColorPicker()
		{
			if(colorChangeEvent == null)
            {
				colorChangeEvent = new UnityEvent();
            }

			styleSheets.Add(Resources.Load<StyleSheet>(pickerStylesResource));

			// panel
			AddToClassList(ussPickerClassName);

			// content area
			var content = new VisualElement();
			content.AddToClassList(ussContentBack);
			hierarchy.Add(content);

			// gradient area
			var gradientArea = new VisualElement();
			gradientArea.AddToClassList(ussGradientArea);
			gradientArea.style.width = Length.Percent(100);
			gradientArea.style.alignContent = Align.Center;
			gradientArea.style.justifyContent = Justify.Center;

			content.Add(gradientArea);

			// gradient block
			gradientSlider = new Slider2D();
			gradientArea.name = "unity-content";

			gradientSliderDragger = gradientSlider.Q("dragger");
			gradientSlider.AddToClassList(ussGradientSlider);
			gradientArea.Add(gradientSlider);

			// hue slider
			hueSlider = new Slider(null, 0f, 360f, SliderDirection.Vertical, 0f);
			hueSliderDragger = hueSlider.Q("unity-dragger");
			hueSlider.AddToClassList(ussHueSlider);
			gradientArea.Add(hueSlider);

			controlsContainter = new VisualElement();
			controlsContainter.style.justifyContent = Justify.SpaceBetween;
			controlsContainter.style.flexDirection = FlexDirection.Column;
			controlsContainter.style.alignItems = Align.Stretch;

			gradientArea.Add(controlsContainter);

			// rgb sliders
			rSlider = new Slider();
			gSlider = new Slider();
			bSlider = new Slider();

			rSlider.showInputField = true;
			gSlider.showInputField = true;
			bSlider.showInputField = true;

			rSlider.AddToClassList(ussRSlider);
			gSlider.AddToClassList(ussGSlider);
			bSlider.AddToClassList(ussBSlider);

			content.Add(rSlider);
			content.Add(gSlider);
			content.Add(bSlider);

			rSlider.RegisterValueChangedCallback(ev => SetColorFromRSliders(ev.newValue));
			gSlider.RegisterValueChangedCallback(ev => SetColorFromGSliders(ev.newValue));
			bSlider.RegisterValueChangedCallback(ev => SetColorFromBSliders(ev.newValue));
			hueSlider.RegisterValueChangedCallback(SetColorFromHueSlider);
			gradientSlider.RegisterValueChangedCallback(SetColorFromGradientSlider);

			var color = Color.green;
			Color.RGBToHSV(color, out H, out S, out V);
			CreateTextures();
			OnColorChanged(true, true);
		}

		private void SetColorFromGradientSlider(ChangeEvent<Vector2> ev)
		{
			S = ev.newValue.x;
			V = ev.newValue.y;
			OnColorChanged(false, false);
		}

		private void SetColorFromHueSlider(ChangeEvent<float> ev)
		{		
			H = ev.newValue / 360f; // hue slider value 0..360
			OnColorChanged(false, true);
		}

		private void SetColorFromRSliders(float value)
		{
			Color c = Color.HSVToRGB(H, S, V);
			c.r = value / 255f;
			Color.RGBToHSV(c, out H, out S, out V);
			OnColorChanged(true, true);
		}

		private void SetColorFromGSliders(float value)
		{
			Color c = Color.HSVToRGB(H, S, V);
			c.g = value / 255f;
			Color.RGBToHSV(c, out H, out S, out V);
			OnColorChanged(true, true);
		}

		private void SetColorFromBSliders(float value)
		{
			Color c = Color.HSVToRGB(H, S, V);
			c.b = value / 255f;
			Color.RGBToHSV(c, out H, out S, out V);
			OnColorChanged(true, true);
		}

		// ------------------------------------------------------------------------------------------------------------

		private void OnColorChanged(bool updateHue, bool updateGradient)
		{
			var c = Color.HSVToRGB(H, S, V);
			hueSliderDragger.style.backgroundColor = Color.HSVToRGB(H, 1f, 1f);
			gradientSliderDragger.style.backgroundColor = c;

			rSlider.SetValueWithoutNotify((int)(c.r * 255));
			gSlider.SetValueWithoutNotify((int)(c.g * 255));
			bSlider.SetValueWithoutNotify((int)(c.b * 255));

			if (updateHue)
			{
				hueSlider.SetValueWithoutNotify(H * 360f);
			}

			if (updateGradient)
			{
				UpdateGradientTexture();
				gradientSlider.SetValueWithoutNotify(new Vector2(S, V));
			}

			colorChangeEvent.Invoke();
		}

		private void CreateTextures()
		{
			gradientTexture = new Texture2D(128, 128, TextureFormat.RGB24, false) { filterMode = FilterMode.Point };
			gradientTexture.hideFlags = HideFlags.HideAndDontSave;
			gradientSlider.style.backgroundImage = gradientTexture;

			hueSliderTexture = new Texture2D(1, 128, TextureFormat.RGB24, false) { filterMode = FilterMode.Point };
			hueSliderTexture.hideFlags = HideFlags.HideAndDontSave;
			hueSlider.style.backgroundImage = hueSliderTexture;
			UpdateHueSliderTexture();
		}

		private void UpdateHueSliderTexture()
		{
			if (hueSliderTexture == null) return;
			for (var i = 0; i < hueSliderTexture.height; i++)
			{
				hueSliderTexture.SetPixel(0, i, Color.HSVToRGB((float)i / (hueSliderTexture.height - 1), 1f, 1f));
			}

			hueSliderTexture.Apply();
			hueSlider.MarkDirtyRepaint();
		}
		
		private void UpdateGradientTexture()
		{
			if (gradientTexture == null) return;
			var pixels = new Color[gradientTexture.width * gradientTexture.height];

			for (var x = 0; x < gradientTexture.width; x++)
			{
				for (var y = 0; y < gradientTexture.height; y++)
				{
					pixels[x * gradientTexture.width + y] = Color.HSVToRGB(H, (float)y / gradientTexture.height, (float)x / gradientTexture.width);
				}
			}

			gradientTexture.SetPixels(pixels);
			gradientTexture.Apply();
			gradientSlider.MarkDirtyRepaint();
		}
	}
}