using System;
using System.Collections.Generic;
using System.Text;

namespace loiefy_loader
{
    public static class CONST
    {
        public const int DEFAULT_TIMEOUT = 3000;
    }
    public enum ActionType { NONE, SET_BAUD, READ_UTIL, SEND_FILE, SEND_CHECK, SEND_DATA, EXIT}
    public enum ActionCheckType { MD2, MD4, MD5,  SHA1, SHA2, SHA128, SHA256, CRC8, CRC16, CRC32}
   
    public class ActionNode
    {
        public static string[] xType = { "NONE", "SET_BAUD", "READ_UTIL", "SEND_FILE", "SEND_CHECK", "SEND_DATA", "EXIT" };
        public static string[] xCheckType = { "MD2", "MD4", "MD5", "SHA1", "SHA2", "SHA128", "SHA256", "CRC8", "CRC16", "CRC32" };
        public ActionType type { get; set; }
        public string value { get; set; }
        public string endValue { get; set; }
        public string foundContent { get; set; }
        public string filePath { get; set; }
        public ActionCheckType checkType { get; set; }
        public int timeout { get; set; }
        public ActionNode ()
        {
            type = ActionType.NONE;
            checkType = ActionCheckType.MD5;
            timeout = CONST.DEFAULT_TIMEOUT;
        }

        public void load (JsonActionNode node)
        {
            value = node.value;
            endValue = node.endValue;
            foundContent = node.foundContent;
            filePath = node.filePath;

            type =  ActionType.EXIT;
            if (string.IsNullOrEmpty(node.type) == false)
            {
                for (int i = 0; i < xType.Length; i++)
                {
                    if (xType[i] == node.type) 
                        type = (ActionType)i;
                }
            }

            checkType = ActionCheckType.MD5;
            if (string.IsNullOrEmpty(node.checkType) == false)
            {
                for (int i = 0; i < xCheckType.Length; i++)
                {
                    if (xCheckType[i] == node.checkType) checkType = (ActionCheckType)i;
                }
            }

            if (string.IsNullOrEmpty(node.timeout) == false)
            {
                int to = 0;
                int.TryParse(node.timeout, out to);
                if (to > 0) timeout = to;
            }
        }

    }
}
