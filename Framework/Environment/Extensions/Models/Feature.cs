﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Environment.Extensions.Models
{
    public class Feature {
        public FeatureDescriptor Descriptor { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
    
}