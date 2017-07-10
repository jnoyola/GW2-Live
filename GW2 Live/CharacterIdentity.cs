﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    struct CharacterIdentity
    {
        #pragma warning disable 0649
        // These fields are assigned by Json deserialization.

        public string name;
        public int profession;
        public int race;
        public int map_id;
        public int world_id;
        public int team_color_id;
        public bool commander;
        public double fov;

        #pragma warning restore 0649
    }
}