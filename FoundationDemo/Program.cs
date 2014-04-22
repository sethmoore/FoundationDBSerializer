using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationDbSerializer;

namespace FoundationDemo
{
    public class Item
    {
        public uint ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public override string ToString()
        {
            return String.Format("{0} {1} {2}: {3}", ID, Price, Name, Description);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Item> Catalog = new List<Item>();
            Catalog.Add(new Item() { ID = 1, Name = "Pencil", Description = "Writes Stuff", Price = .50 });
            Catalog.Add(new Item() { ID = 2, Name = "Pen", Description = "Writes Stuff Permentantly", Price = .70 });
            Catalog.Add(new Item() { ID = 3, Name = "Erasers", Description = "Un-Writes Stuff", Price = .20 });

        }
    }
}
