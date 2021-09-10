﻿namespace EasyContainer.Lib.UnitTests
{
    using System.Collections.Generic;

    public class MySettings2 : Setting
    {
        public List<MySettings2Nested> MySettings2Nested { get; set; } = new List<MySettings2Nested>();

        public string Message { get; set; }
    }
    
    public class MySettings2Nested : Setting
    {
        public string Key { get; set; }
        
        public string Value { get; set; }
    }
}