﻿using System.ComponentModel.DataAnnotations;

/// <summary>
/// Created By Sz => sz
/// </summary>

namespace Models
{
    public class tbl_Setting
    {
        [Key]
        public int id { get; set; }

        public string userName { get; set; }

        public string title { get; set; }

        public string passWord { get; set; }

        public bool isStartUp { get; set; }

    }
}
