﻿using System;
using System.Collections.Generic;
using System.Web.Optimization;

namespace Entangled.Life.Web
{
    internal class NonOrderingOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}