using System;

namespace Qwf.Test.Consoles
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string name = "LILEY";
            Console.WriteLine(Yiwan.Utilities.Cache.LocalCache.Set("name", name));
            Console.WriteLine(Yiwan.Utilities.Cache.LocalCache.Get("name"));

            var student = new Student { Name = "大力", Age = 18 };
            Console.WriteLine(Yiwan.Utilities.Cache.LocalCache.Set("student", student));
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(Yiwan.Utilities.Cache.LocalCache.Get<Student>("student")));
            Console.ReadLine();
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(Yiwan.Utilities.Cache.LocalCache.Get<Student>("student")));
        }
        public class Student
        {
            public string Name { set; get; }
            public int Age { set; get; }
        }
    }
}
