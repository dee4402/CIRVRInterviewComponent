using System.Collections.Generic;
using SimpleJSON;
using System.Text;
using UnityEngine;

namespace Cirvr.ConversationManager {

    public class JsonHelper 
    {
        public static List<string> parseIntoList(JSONArray jsonArray)
        {
            List<string> list = new List<string>();
            foreach (JSONNode node in jsonArray.Children)
            {
                list.Add(node.Value);
            }
            return list;
        }

        public static List<T> parseIntoList<T>(JSONArray jsonArray) where T : IConstructFromJson<T>, new()
        {
            List<T> list = new List<T>();
            foreach (JSONNode node in jsonArray.Children)
            {
                list.Add(new T().ConstructFromNode(node));
            }
            return list;
        }

        // JsonUtility doesn't support arrays as top level args so we need this
        public static string toJsonList<T>(T[] objList)
        {
            StringBuilder sb = new StringBuilder("[", 500);
            for (int i = 0; i < objList.Length; i++)
            {
                sb.Append(JsonUtility.ToJson(objList[i]));
                if (i != (objList.Length - 1))
                {
                    sb.Append(',');
                }
            }
            sb.Append(']');

            return sb.ToString();
        }
    }
}