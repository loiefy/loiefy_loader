using System;
using System.Collections.Generic;
using System.Text;

namespace loiefy_loader
{
    public class Project
    {
        public string timeout { get; set; }
        public JsonComSetting com { get; set; }
        public JsonLog log { get; set; }
        public List <JsonActionNode> sequence { get; set; }

        public Project()
        {
            timeout = "3000";
            com = new JsonComSetting();
            log = new JsonLog();
            sequence = new  List<JsonActionNode>();
        }
    }
}
