using System;
using System.IO.Ports;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace loiefy_loader
{
    class Program
    {
        private static SerialPort myport = new SerialPort();
        private static string[] ports ;
        private static ComSetting tempport;
        private static Timer mytimer;
        private static int sendfilepercent = 0;

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
                        Console.WriteLine("created and saved default projec at: " + foundArg.value);
                        break;
                    case Cmd.help:
                        Console.WriteLine("     -help:                 | get help");
                        Console.WriteLine("     -openprj[path]:        | to open a JSON project at path");
                        Console.WriteLine("     -createdefltprj[path]: | to create a default JSON project save to path");
                        break;
                    case Cmd.openprj:
                        ports = SerialPort.GetPortNames();
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

        private static bool openport(ComSetting comport)
        {
            if (string.IsNullOrEmpty(comport.port) )
            {
                Console.WriteLine("Error: please provide serial port name !");
                return false;
            }

            ports = SerialPort.GetPortNames();
            bool foundport = false;
            foreach (string str in ports)
            {
                if (str.ToUpper() == comport.port.ToUpper())
                {
                    foundport = true;
                    break;
                }
            }
            if (foundport == false)
            {
                Console.WriteLine("Error: " + comport.port.ToUpper() + " not found !");
                return false;
            }

            if (myport == null) myport = new SerialPort();
            if (myport.IsOpen)
            {
                myport.Close();
                Console.WriteLine("closed port: " + comport.port.ToUpper());
            }
            myport.PortName = comport.port.ToUpper();
            myport.BaudRate = (int)comport.baud;

            if (comport.parity == eParity.even) myport.Parity = Parity.Even;
            if (comport.parity == eParity.none) myport.Parity = Parity.None;
            if (comport.parity == eParity.odd) myport.Parity = Parity.Odd;

            myport.DataBits = comport.dataBits;

            switch (comport.stopBits)
            {
                case eStopBits.none:
                    myport.StopBits = StopBits.None;
                    break;
                case eStopBits.one:
                    myport.StopBits = StopBits.One;
                    break;
                case eStopBits.onepointfive:
                    myport.StopBits = StopBits.OnePointFive;
                    break;
                case eStopBits.two:
                    myport.StopBits = StopBits.Two;
                    break;
            }
            try
            {
                Console.WriteLine("opening port: " + myport.PortName);
                myport.Open();
                Console.WriteLine("opened");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error, detail: " + ex.Message);
                return false;
            }

            tempport = comport;
            return true;
        }

        private static bool setbaud(UInt32 newbaud)
        {
            if (newbaud == 0 )
            {
                Console.WriteLine("baudrate cannot be zero");
                return false;
            }
            if (newbaud > 3000000)
            {
                Console.WriteLine("baudrate cannot be greater than 3000000");
                return false;
            }

            if (tempport == null) tempport = new ComSetting();
            tempport.baud = newbaud;

            Console.WriteLine("Set baud: " + newbaud.ToString());
            openport(tempport);

            return true;
        }

        private static bool read_util(string end, string content, int timeoutms)
        {
            if (myport.IsOpen == false)
            {
                Console.WriteLine("Error: COM port is not open");
                return false;
            }

            Console.WriteLine("reading ...");
            string s = string.Empty;
            int loopcount = 0;
            bool ItTimeToEnd = false;
            do
            {
                Thread.Sleep(50);
                string ss = myport.ReadExisting();
                Console.Write(ss);
                s = s + ss;
                loopcount += 1;

                if (s.Length > 0)
                {
                    bool condition1 = s.Contains(content);
                    if (string.IsNullOrEmpty(content))
                        condition1 = true;

                    string endvalue = string.Empty;
                    if (end.Contains("RF")) endvalue = "\n";
                    if (end.Contains("CL_RF")) endvalue = System.Environment.NewLine;
                    if (end.Contains("CL")) endvalue = "\r";
                    if (end.Contains("ZERO")) endvalue = "\0";

                    bool condition2 = s.Contains(endvalue);
                    if (condition1 && condition2) ItTimeToEnd = true;
                }

                if ((loopcount * 50) > timeoutms)
                {
                    Console.WriteLine("Error: Timeout");
                    return false;
                }

            } while ( ItTimeToEnd == false);

            return true;
        }

        private static bool sendFile(string filepath)
        {
            if (myport.IsOpen == false)
            {
                Console.WriteLine("Error: Port is not open");
                return false;
            }

            Console.WriteLine("Sending file: " + filepath);
            if (File.Exists(filepath) == false)
            {
                Console.WriteLine("Error: file " + filepath + " not found");
                return false;
            }
            else
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(filepath);
                    mytimer = new Timer(TimerCallback, null, 0, 1000);
                    int time = bytes.Length / 1000;
                    if (time == 0)
                    {
                        myport.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        int i = 0;
                        for ( i = 0; i< time; i++)
                        {
                            myport.Write(bytes, i * 1000, 1000);
                            Thread.Sleep(100);
                        }
                        myport.Write(bytes, i * 1000, bytes.Length - (i * 1000));
                        Thread.Sleep(100);
                    }

                    mytimer.Dispose();
                    Console.WriteLine();
                    Console.WriteLine("Send file done! ");
                }
                catch (Exception ex)
                {
                    mytimer.Dispose();
                    Console.WriteLine("Error, detail: " + ex);
                    return false;
                }
            }
            return true;
        }

        private static void TimerCallback(Object o)
        {
            Console.Write(". ");
        }

        private static bool sendData(byte[] data, int length)
        {
            if (myport.IsOpen)
            {
                Console.WriteLine("Send data...");
                myport.Write(data, 0, length);
                return true;
            }
            else
            {
                Console.WriteLine("Error: port is not open");
                return false;
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
                        //show_project_content(prj);
                        run_process(prj);
                        return "";
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
            Console.WriteLine("=========== Debug project =============");
            Console.WriteLine("---project.com.port: " + project.com.port);
            Console.WriteLine("---project.com.baudrate: " + project.com.baudrate);
            Console.WriteLine("---project.com.dataBits: " + project.com.dataBits);
            Console.WriteLine("---project.com.parity: " + project.com.parity);
            Console.WriteLine("---project.com.stopBits: " + project.com.stopBits);
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
                            Console.WriteLine("----[{0}].timeout: " + node.timeout, Index);
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

        private static bool run_process(Project prj)
        {
            tempport = new ComSetting();
            tempport.load(prj.com);
            if (string.IsNullOrEmpty(tempport.port) )
            {
                Console.WriteLine("Error: Comport error");
                return false;
            }
            if (openport(tempport) == false) return false;

            foreach (JsonActionNode nd in prj.sequence)
            {
                ActionNode node = new ActionNode();
                node.load(nd);
                switch(node.type)
                {
                    case ActionType.NONE:
                        Console.WriteLine("Error: Unknow action \"" + nd.type + "\"");
                        return false;
                    case ActionType.READ_UTIL:
                        if (read_util(node.endValue, node.foundContent, node.timeout) == false) return false;
                        break;
                    case ActionType.SEND_CHECK:

                        break;
                    case ActionType.SEND_DATA:
                        byte[] bytes = Encoding.ASCII.GetBytes(node.value);
                        if (sendData( bytes, bytes.Length) == false) return false;
                        break;
                    case ActionType.SEND_FILE:
                        if (sendFile(node.filePath) == false) return false;
                        break;
                    case ActionType.SET_BAUD:
                        int br = 0;
                        int.TryParse(node.value, out br);
                        if (br <= 0)
                        {
                            Console.WriteLine("Error: baudrate canot be zero");
                            return false;
                        }
                        if (br > 3000000)
                        {
                            Console.WriteLine("Error: baudrate cannot greater than 3000000");
                            return false;
                        }
                        if (setbaud((uint)br) == false) return false;
                        break;
                    case ActionType.EXIT:
                        Console.WriteLine("Exit");
                        return true;
                       
                    default:
                        break;
                }
            }
            return true;
        }
    }
}
