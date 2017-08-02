﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace artiMech
{
    /// <summary>
    /// A static class to help with some of the specific code needed for the
    /// state editor.
    /// </summary>
    public static class stateEditorUtils
    {

        // A list of windows to be displayed
        static IList<stateWindowsNode> m_StateList = new List<stateWindowsNode>();

        // This is a list filled by scanning the source code of the 
        // generated statemachine and populating them with their class names.
        static IList<string> m_StateNameList = new List<string>();

        static GameObject m_EditorCurrentGameObject = null;

        static Vector2 m_MousePos;

        static GameObject m_GameObject = null;
        static GameObject m_WasGameObject = null;
        static string m_StateMachineName = "";

        #region Accessors 
        public static IList<stateWindowsNode> StateList
        {
            get
            {
                return m_StateList;
            }

            set
            {
                m_StateList = value;
            }
        }

        public static GameObject EditorCurrentGameObject
        {
            get
            {
                return m_EditorCurrentGameObject;
            }

            set
            {
                m_EditorCurrentGameObject = value;
            }
        }

        public static GameObject GameObject
        {
            get
            {
                return m_GameObject;
            }

            set
            {
                m_GameObject = value;
            }
        }

        public static string StateMachineName
        {
            get
            {
                return m_StateMachineName;
            }

            set
            {
                m_StateMachineName = value;
            }
        }

        public static GameObject WasGameObject
        {
            get
            {
                return m_WasGameObject;
            }

            set
            {
                m_WasGameObject = value;
            }
        }

        #endregion

        /// <summary>
        /// This function is really more specific to the Artimech project and its 
        /// code generation system.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="objectName"></param>
        /// <param name="pathName"></param>
        /// <param name="pathAndFileName"></param>
        /// <param name="findName"></param>
        /// <param name="replaceName"></param>
        /// <returns></returns>
        public static string ReadReplaceAndWrite(
                            string fileName,
                            string objectName,
                            string pathName,
                            string pathAndFileName,
                            string findName,
                            string replaceName)
        {

            string text = utlDataAndFile.LoadTextFromFile(fileName);

            string changedName = replaceName + objectName;
            string modText = text.Replace(findName, changedName);

            StreamWriter writeStream = new StreamWriter(pathAndFileName);
            writeStream.Write(modText);
            writeStream.Close();

            return changedName;
        }

        public static void CreateStateWindows(string fileName)
        {
            string strBuff = utlDataAndFile.LoadTextFromFile(fileName);
            PopulateStateStrings(strBuff);

            for(int i=0;i<m_StateNameList.Count;i++)
            {
                stateWindowsNode node = CreateStateWindowsNode(m_StateNameList[i]);
                m_StateList.Add(node);
            }
        }

        public static stateWindowsNode CreateStateWindowsNode(string typeName)
        {
            stateWindowsNode winNode = new stateWindowsNode(StateList.Count);
            winNode.WindowTitle = typeName;

            float x = 0;
            float y = 0;
            float width = 0;
            float height = 0;
            string winName = typeName;

//            TextAsset text = Resources.Load(typeName+".cs") as TextAsset;
            string strBuff = "";
            string fileName = "";
            fileName = utlDataAndFile.FindPathAndFileByClassName(typeName,false);
            strBuff = utlDataAndFile.LoadTextFromFile(fileName);
            string[] words = strBuff.Split(new char[] { '<', '>' });

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "name" && words[i + 1]!="nada")
                    winName = words[i + 1];
                if (words[i] == "posX")
                    x = Convert.ToSingle(words[i + 1]);
                if (words[i] == "posY")
                    y = Convert.ToSingle(words[i + 1]);
                if (words[i] == "sizeX")
                    width = Convert.ToSingle(words[i + 1]);
                if (words[i] == "sizeY")
                    height = Convert.ToSingle(words[i + 1]);
            }

            winNode.Set(winName, x, y, width, height);
            return winNode;
        }

        public static void PopulateStateStrings(string strBuff)
        {
            m_StateNameList.Clear();

            string[] words = strBuff.Split(new char[] { ' ', '(' });

            for(int i=0;i<words.Length;i++)
            {
                if (words[i] == "new")
                {
                    Type type = Type.GetType("artiMech." + words[i + 1]);

                    if (type != null)
                    {
                        string buffer = "";
                        buffer = type.BaseType.Name;
                        if (buffer == "baseState")
                        {
                            m_StateNameList.Add(words[i + 1]);
                            // Debug.Log("<color=cyan>" + "<b>" + "words[i + 1] = " + "</b></color>" + "<color=grey>" + words[i + 1] + "</color>" + " .");
                        }

                    }
                }
            }
        }

        public static bool CreateAndAddStateCodeToProject(GameObject gameobject,string stateName)
        {

            string pathName = "Assets/Scripts/artiMechStates/";
            string FileName = "Assets/Scripts/Common/statemachine/stateTemplate.cs";

            string pathAndFileNameStartState = pathName
                                + "aMech"
                                + gameobject.name
                                + "/"
                                + stateName
                                + ".cs";

            if (File.Exists(pathAndFileNameStartState))
            {
                Debug.Log("<color=red>stateEditor.CreateStateMachine = </color> <color=blue> " + pathAndFileNameStartState + "</color> <color=red>Already exists and can't be overridden...</color>");
                return false;
            }

            //creates a start state from a template and populate aMech directory
            string stateStartName = stateEditorUtils.ReadReplaceAndWrite(FileName, stateName, pathName, pathAndFileNameStartState, "stateTemplate", "");

            return true;
        }

        public static bool AddStateCodeToStateMachineCode(string fileAndPath,string stateName)
        {
            string strBuff = "";
            strBuff = utlDataAndFile.LoadTextFromFile(fileAndPath);

            if (strBuff == null || strBuff.Length == 0)
                return false;

            string modStr = "";
            //AddState(new stateStartTemplate(this.gameObject), "stateStartTemplate", "new state change system");
            string insertString = "\n            AddState(new "
                                + stateName
                                + "(this.gameObject),"
                                + "\""
                                + stateName
                                + "\""
                                + ");";

            modStr = utlDataAndFile.InsertInFrontOf(strBuff, 
                                                    "<ArtiMechStates>",
                                                    insertString);

            utlDataAndFile.SaveTextToFile(fileAndPath, modStr);

            return true;
        }

        public static bool SetPositionAndSizeOfAStateFile(string fileName,int x, int y, int width, int height)
        {
            string strBuff = "";
            strBuff = utlDataAndFile.LoadTextFromFile(fileName);

            if (strBuff == null || strBuff.Length == 0)
                return false;

            string modStr = "";
            modStr = utlDataAndFile.ReplaceBetween(strBuff, "<posX>", "</posX>",x.ToString());
            modStr = utlDataAndFile.ReplaceBetween(modStr, "<posY>", "</posY>", y.ToString());
            modStr = utlDataAndFile.ReplaceBetween(modStr, "<sizeX>", "</sizeX>", width.ToString());
            modStr = utlDataAndFile.ReplaceBetween(modStr, "<sizeY>", "</sizeY>", height.ToString());

            utlDataAndFile.SaveTextToFile(fileName, modStr);

            return true;
        }

        public static string GetCode(int number)
        {
            int start = (int)'A' - 1;
            if (number <= 26) return ((char)(number + start)).ToString();

            StringBuilder str = new StringBuilder();
            int nxt = number;

            List<char> chars = new List<char>();

            while (nxt != 0)
            {
                int rem = nxt % 26;
                if (rem == 0) rem = 26;

                chars.Add((char)(rem + start));
                nxt = nxt / 26;

                if (rem == 26) nxt = nxt - 1;
            }


            for (int i = chars.Count - 1; i >= 0; i--)
            {
                str.Append((char)(chars[i]));
            }

            return str.ToString();
        }

        //paths and filenames
        public const string k_StateMachineTemplateFileAndPath = "Assets/Scripts/Common/statemachine/stateMachineTemplate.cs";
        public const string k_StateTemplateFileAndPath = "Assets/Scripts/Common/statemachine/stateTemplate.cs";
        public const string k_PathName = "Assets/Scripts/artiMechStates/";

        /// <summary>
        /// Artimech's statemachine and startState generation system.
        /// </summary>
        public static void CreateStateMachineScriptAndStartState()
        {

            string pathAndFileName = k_PathName
                                                        + "aMech"
                                                        + stateEditorUtils.GameObject.name
                                                        + "/"
                                                        + "aMech"
                                                        + stateEditorUtils.GameObject.name
                                                        + ".cs";

            string pathAndFileNameStartState = k_PathName
                                            + "aMech"
                                            + stateEditorUtils.GameObject.name
                                            + "/"
                                            + "aMech"
                                            + stateEditorUtils.GameObject.name
                                            + "StartState"
                                            + ".cs";

            if (File.Exists(pathAndFileName))
            {
                Debug.Log("<color=red>stateEditor.CreateStateMachine = </color> <color=blue> " + pathAndFileName + "</color> <color=red>Already exists and can't be overridden...</color>");
                return;
            }

            //clear the visual list if there are any in the editor
           // ClearStatesAndRefresh();

            //create the aMech directory 
            string replaceName = "aMech";
            string directoryName = k_PathName + replaceName + stateEditorUtils.GameObject.name;
            Directory.CreateDirectory(directoryName);

            //creates a start state from a template and populate aMech directory
            string stateStartName = "";
            stateStartName = stateEditorUtils.ReadReplaceAndWrite(
                                                        k_StateTemplateFileAndPath,
                                                        stateEditorUtils.GameObject.name + "StartState",
                                                        k_PathName,
                                                        pathAndFileNameStartState,
                                                        "stateTemplate",
                                                        "aMech");

            //creates the statemachine from a template
            string stateMachName = "";
            stateMachName = stateEditorUtils.ReadReplaceAndWrite(
                                                        k_StateMachineTemplateFileAndPath,
                                                        stateEditorUtils.GameObject.name,
                                                        k_PathName,
                                                        pathAndFileName,
                                                        "stateMachineTemplate",
                                                        replaceName);

            //replace the startStartStateTemplate
            utlDataAndFile.ReplaceTextInFile(pathAndFileName, "stateTemplate", stateStartName);

            Debug.Log(
                        "<b><color=navy>Artimech Report Log Section A\n</color></b>"
                        + "<i><color=grey>Click to view details</color></i>"
                        + "\n"
                        + "<color=blue>Finished creating a state machine named </color><b>"
                        + stateMachName
                        + "</b>:\n"
                        + "<color=blue>Created and added a start state named </color>"
                        + stateStartName
                        + "<color=blue> to </color>"
                        + stateMachName
                        + "\n\n");

            SaveStateInfo(stateMachName, stateEditorUtils.GameObject.name);

            AssetDatabase.Refresh();

            stateEditorUtils.StateMachineName = stateMachName;
//            m_AddStateMachine = true;

            utlDataAndFile.FindPathAndFileByClassName(stateEditorUtils.StateMachineName, false);
        }

        public static void SaveStateInfo(string stateMachineName,string gameObjectName)
        {
            string pathAndFile = Application.dataPath + "StateMachine.txt";
            string stateInfo = stateMachineName + " " + gameObjectName;
            utlDataAndFile.SaveTextToFile(pathAndFile, stateInfo);
            
        } 


        public static void ContextCallback(object obj)
        {
            //make the passed object to a string
            string clb = obj.ToString();
            //string stateName = "";



            if (clb.Equals("addState") && GameObject != null)
            {
                if (StateList.Count == 0)
                {
                    Debug.LogError("StateList is Empty so you can't create a state.");
                    return;
                }
                string stateName = "aMech" + GameObject.name + "State" + GetCode(StateList.Count);
                if (stateEditorUtils.CreateAndAddStateCodeToProject(GameObject, stateName))
                {
                    stateWindowsNode windowNode = new stateWindowsNode(stateEditorUtils.StateList.Count);
                    windowNode.Set(stateName, m_MousePos.x, m_MousePos.y, 150, 80);
                    stateEditorUtils.StateList.Add(windowNode);

                    string fileAndPath = "";
                    fileAndPath = utlDataAndFile.FindPathAndFileByClassName(stateName);
                    stateEditorUtils.SetPositionAndSizeOfAStateFile(fileAndPath, (int)m_MousePos.x, (int)m_MousePos.y, 150, 80);

                    fileAndPath = utlDataAndFile.FindPathAndFileByClassName(StateMachineName);

                    stateEditorUtils.AddStateCodeToStateMachineCode(fileAndPath, stateName);

                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
