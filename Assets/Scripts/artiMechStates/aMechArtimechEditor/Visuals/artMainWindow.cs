using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Artimech
{
    /// <summary>
    /// Base window for the Artimech state editor system
    /// </summary>
    public class artMainWindow : artWindowBase
    {
        #region Variables
        baseState m_State;
        Vector2 m_MousePos;

        #endregion
        #region Gets Sets
        public Vector2 MousePos
        {
            get
            {
                return m_MousePos;
            }

            set
            {
                m_MousePos = value;
            }
        }
        #endregion
        #region Member Functions


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title"></param>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        /// <param name="id"></param>
        public artMainWindow(baseState state, string title, Rect rect, Color color, int id) : base(title, rect, color, id)
        {
            m_State = state;
        }

        /// <summary>
        /// Update
        /// </summary>
        new public void Update()
        {
            m_WinRect.width = Screen.width;
            m_WinRect.height = Screen.height;
            GUI.Window(m_Id, WinRect, Draw, m_Title);
        }

        /// <summary>
        /// Check to see if a position is inside the main window.
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        new public bool IsWithin(Vector2 vect)
        {
            if (vect.x >= WinRect.x && vect.x < WinRect.x + WinRect.width)
            {
                if (vect.y >= WinRect.y && vect.y < WinRect.y + WinRect.height)
                {
                    return true;
                }
            }
            return false;
        }

        new public void Draw(int id)
        {
            m_MousePos = Event.current.mousePosition;
            Rect rect = new Rect(0, 0, WinRect.width, WinRect.height);
            EditorGUI.DrawRect(rect, m_WindowColor);

            ArtimechEditor theStateMachineEditor = (ArtimechEditor)m_State.m_UnityObject;
            stateEditorDrawUtils.DrawGridBackground(theStateMachineEditor.ConfigData);

            
            for (int i = 0; i < theStateMachineEditor.VisualStateNodes.Count; i++)
            {
                theStateMachineEditor.VisualStateNodes[i].Update(m_State, theStateMachineEditor.TransMtx, theStateMachineEditor.ConfigData);
            }
        }
    }
    #endregion
}

#endif