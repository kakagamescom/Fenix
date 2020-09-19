﻿using Fenix.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenix
{
    //Service cannot communicate with client
    //server only
    [ActorType(AType.SERVER)]
    public partial class Service : Actor
    {
//#if !RUNTIME
        public dynamic self => this;
//#else
//        public Service self => this;
//#endif

    }
}
