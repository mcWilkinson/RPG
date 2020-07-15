﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RPG.Dialogue
{
    [CreateAssetMenu(menuName = ("RPG/Dialogue"))]
    public class Dialogue : ScriptableObject
    {
        [SerializeField]
        List<DialogueNode> nodes = new List<DialogueNode>();
        Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

        private void OnValidate() {
#if UNITY_EDITOR
            if (nodes.Count <= 0)
            {
                CreateNode(null);
            }
#endif
            nodeLookup.Clear();
            foreach (DialogueNode node in GetAllNodes())
            {
                nodeLookup[node.name] = node;
            }
        }

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public IEnumerable<DialogueNode> GetChildren(DialogueNode node)
        {
            foreach (string childID in node.GetChildren())
            {
                if (nodeLookup.ContainsKey(childID))
                {
                    yield return nodeLookup[childID];
                }
            }
        }

#if UNITY_EDITOR
        public DialogueNode CreateNode(DialogueNode parent)
        {   
            DialogueNode newNode = CreateInstance<DialogueNode>();
            Undo.RegisterCreatedObjectUndo(newNode, "");
            newNode.name = System.Guid.NewGuid().ToString();
            if (parent != null)
            {
                Vector2 childOffset = new Vector2(200, 0);
                newNode.SetPosition(parent.GetRect().position + childOffset);
                parent.AddChild(newNode.name);
            }
            nodes.Add(newNode);
            AssetDatabase.AddObjectToAsset(newNode, this);
            newNode.SetNextSpeaker(!parent.IsPlayerNextSpeaker());
            OnValidate();
            return newNode;
        }

        public void DeleteNode(DialogueNode deletingNode)
        {
            nodes.Remove(deletingNode);
            OnValidate();
            CleanDanglingChildren(deletingNode.name);
            Undo.DestroyObjectImmediate(deletingNode);
        }

        private void CleanDanglingChildren(string IDToRemove)
        {
            foreach (DialogueNode node in GetAllNodes())
            {
                node.RemoveChild(IDToRemove);
            }
        }
#endif
    }
}