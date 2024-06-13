﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.OtherLaunch;

public record OfficialObj
{
    public record PatchObj
    { 
        public string id { get; set; }
        public string version { get; set; }
    }
    public record LibrarieObj
    { 
        public string name { get; set; }
    }
    public string id { get; set; }
    public string inheritsFrom { get; set; }
    public List<PatchObj> patches { get; set; }
    public List<LibrarieObj> libraries { get; set; }
}