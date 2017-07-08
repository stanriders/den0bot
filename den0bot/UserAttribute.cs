using System;

namespace den0bot
{
    class UserAttribute : Attribute
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public UserAttribute(string name = "", int id = 0)
        {
            ID = id;
            Name = name;
        }
    }
}
