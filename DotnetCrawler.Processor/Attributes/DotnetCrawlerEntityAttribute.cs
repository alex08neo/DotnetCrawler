﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetCrawler.Processor.Attributes
{
    public class DotnetCrawlerEntityAttribute : Attribute
    {
        public string XPath { get; set; }
    }    
}
