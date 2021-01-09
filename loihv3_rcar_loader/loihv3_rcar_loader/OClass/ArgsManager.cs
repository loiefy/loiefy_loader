using System;
using System.Collections.Generic;
using System.Text;

namespace loiefy_loader
{
    public class GetCmdReturnType
    {
        public string value { get; set; }
        public Cmd cmd { get; set; }
    }

    public enum Cmd { openprj = 0, createdefltprj, help, none}
    public class ArgsManager
    {
        public List<string> ArgsList = new List<string>(1)
        {
            "-openprj",
            "-createdefltprj",
            "-help"
        };

        public GetCmdReturnType GetArg(string arg)
        {
            GetCmdReturnType RetVal = new GetCmdReturnType();
            RetVal.cmd = Cmd.none;
            RetVal.value = string.Empty;

            for (int Index = 0; Index < ArgsList.Count; Index ++ )
            {
                if (arg.Contains(ArgsList[Index]))
                {
                    RetVal.cmd = (Cmd)Index;
                    int StartIndex = arg.IndexOf(ArgsList[Index]);
                    StartIndex = StartIndex + ArgsList[Index].Length;
                    RetVal.value = arg.Substring(StartIndex, arg.Length - StartIndex);
                    return RetVal;
                }
            }
            return RetVal;
        }

    }
}
