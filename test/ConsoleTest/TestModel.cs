using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class TestModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime StandardDateTime { get; set; }
        public DateTimeOffset OffsetDateTime { get; set; }
        public Detail Details { get; set; }
    }
    public class Detail
    {
        public int Id { get; set; }
        public Parent Father { get; set; }
        public Parent Mother { get; set; }

    }
    public class Parent
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
