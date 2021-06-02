using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class Alert
    {
        public int start { get; set; }
        public int end { get; set; }
        public string event_ { get; set; }
        public string description { get; set; }
        public string sender_name { get; set; }
    }
}