using System;

namespace den0bot
{
    class UserAttribute : Attribute
    {
        public uint ID { get; set; }
        public string Name { get; set; }

        public UserAttribute(string name = "", uint id = 0)
        {
            ID = id;
            Name = name;
        }
    }
}
