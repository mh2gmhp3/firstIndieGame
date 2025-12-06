using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public class CommonGUIStyle
    {
        public static string Default_Box = "Box";

        [MenuItem("Tools/ClearAllGUIStyle", priority = 100)]
        public static void ClearAllGUIStyle()
        {
            var t = typeof(CommonGUIStyle);
            var staticFields = t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            for (int i = 0; i < staticFields.Length; i++)
            {
                var staticField = staticFields[i];
                staticField.SetValue(t, null);
            }
        }

        private static GUIStyle _redBox;
        public static GUIStyle RedBox
        {
            get
            {
                if (_redBox == null)
                {
                    _redBox = new GUIStyle();
                    var redTex = new Texture2D(1, 1);
                    redTex.SetPixel(0, 0, Color.red);
                    redTex.Apply();
                    _redBox.normal.background = redTex;
                }

                return _redBox;
            }
        }

        private static GUIStyle _unselectBlueBox;
        public static GUIStyle UnselectBlueBox
        {
            get
            {
                if (_unselectBlueBox == null)
                {
                    _unselectBlueBox = new GUIStyle();
                    var tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, new Color(0, 0.25f, 0.5f, 1));
                    tex.Apply();
                    _unselectBlueBox.normal.background = tex;
                }

                return _unselectBlueBox;
            }
        }

        private static GUIStyle _selectBlueBox;
        public static GUIStyle SelectBlueBox
        {
            get
            {
                if (_selectBlueBox == null)
                {
                    _selectBlueBox = new GUIStyle();
                    var tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, new Color(0, 0.5f, 1f, 1));
                    tex.Apply();
                    _selectBlueBox.normal.background = tex;
                }

                return _selectBlueBox;
            }
        }

        public static GUIStyle SelectableBlueBox(bool selected)
        {
            return selected ? SelectBlueBox : UnselectBlueBox;
        }
    }
}
