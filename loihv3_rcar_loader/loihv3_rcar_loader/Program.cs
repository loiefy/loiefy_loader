using System;
using System.IO.Ports;
using System.IO;
using Newtonsoft.Json;

namespace loiefy_loader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No args found, try -help argument for help");
                return;
            }
            foreach (string arg in args)
            {
                ArgsManager argmng = new ArgsManager();
                GetCmdReturnType foundArg = argmng.GetArg(arg);
                switch (foundArg.cmd)
                {
                    case Cmd.createdefltprj:
                        Console.WriteLine("create and save default projec at: " + foundArg.value);
                        break;
                    case Cmd.help:
                        Console.WriteLine("     -help:                 | get help");
                        Console.WriteLine("     -openprj[path]:        | to open a JSON project at path");
                        Console.WriteLine("     -createdefltprj[path]: | to create a default JSON project save to path");
                        break;
                    case Cmd.openprj:
                        Console.WriteLine("loiefy_loader started");
                        Console.WriteLine("Opening project: " + foundArg.value);
                        Console.WriteLine(run_project(foundArg.value));
                        break;
                    case Cmd.none:
                        Console.WriteLine("argument: " + arg + " not supported, try -help argument for help");
                        return;
                }
            }
        }

        private static string run_project(string path)
        {
            if (File.Exists(path))
            {
                string filecontent = string.Empty;
                using (StreamReader reader = new StreamReader(path))
                {
                    filecontent = reader.ReadToEnd();
                    reader.Close();
                }
                if (filecontent != string.Empty)
                {
                    Project prj = JsonConvert.DeserializeObject<Project>(filecontent);
                    if (prj != null)
                    {
                        show_project_content(prj);
                        return "good job";
                    }
                    else
                    {
                        return "Error: file is damaged";
                    }
                }
                else
                {
                    return "Error: file is damaged";
                }
            }
            else
            {
                return "Error: file not found";
            }
        }

        private static void show_project_content(Project project)
        {
            Console.WriteLine("---project.timeout: " + project.timeout);
            Console.WriteLine("---project.com.port: " + project.com.port);
            Console.WriteLine("---project.com.baudrate: " + project.com.baudrate);
            Console.WriteLine("---project.log.enable: " + project.log.enable);
            Console.WriteLine("---project.log.path: " + project.log.path);
            Console.WriteLine();
            Console.WriteLine("---project.sequence.Count: " + project.sequence.Count);
            
            for (int Index = 0;  Index <  project.sequence.Count; Index++)
            {
                JsonActionNode node = project.sequence[Index];
                ActionNode anode = new ActionNode();
                anode.load(node);
                if (anode.type != ActionType.NONE)
                {
                    Console.WriteLine("----[{0}].type: " + node.type, Index);

                    switch (anode.type)
                    {
                        case ActionType.EXIT:
                            break;
                        case ActionType.SET_BAUD:
                            Console.WriteLine("----[{0}].value: " + node.value, Index);
                            break;
                        case ActionType.READ_UTIL:
                            Console.WriteLine("----[{0}].endValue: " + node.endValue, Index);
                            Console.WriteLine("----[{0}].foundContent: " + node.foundContent, Index);
                            break;
                        case ActionType.SEND_CHECK:
                            Console.WriteLine("----[{0}].checkType: " + node.checkType, Index);
                            Console.WriteLine("----[{0}].filePath: " + node.filePath, Index);
                            break;
                        case ActionType.SEND_DATA:
                            Console.WriteLine("----[{0}].value: " + node.value, Index);
                            break;
                        case ActionType.SEND_FILE:
                            Console.WriteLine("----[{0}].filePath: " + node.filePath, Index);
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("sequence[{0}] has problem", Index);
                }
                Console.WriteLine();
            }
        }
    }
}
