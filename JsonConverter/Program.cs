using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JsonConverter
{
    public class Studies
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mode")]
        public string Mode { get; set; }

        public class Student
        {
            [JsonProperty("indexNumber")]
            public string IndexNumber { get; set; }
            [JsonProperty("fname")]
            public string Name { get; set; }
            [JsonProperty("lname")]
            public string LastName { get; set; }
            [JsonProperty("birthdate")]
            public DateTime DateOfBirth { get; set; }
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("mothersName")]
            public string MothersName { get; set; }
            [JsonProperty("fathersName")]
            public string FathersName { get; set; }
            [JsonProperty("studies")]
            public Studies Studies { get; set; }
        }

        public class University
        {
            public DateTime CreatedAt { get; set; }
            public string Author { get; set; }
            public HashSet<Student> Students { get; set; }
            //public List<ActiveStudy> ActiveStudies { get; set; }
        }

        internal class Program
        {


            public static HashSet<Student> ReadStudentsFromCsv()
            {
                var students = new HashSet<Student>(); // custom comparer for hashset
                using (var reader = new StreamReader("./dane.csv"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] columns = line.Split(',');

                        if (columns.Length != 9) continue; // TODO: error handling 

                        var student = new Student
                        {
                            IndexNumber = "s" + columns[4],
                            Name = columns[0],
                            LastName = columns[1],
                            DateOfBirth = DateTime.Parse(columns[5]),
                            Email = columns[6],
                            MothersName = columns[7],
                            FathersName = columns[8],

                            Studies = new Studies
                            {
                                Name = columns[2],
                                Mode = columns[3]
                            }

                        };
                        // TODO: check for duplicates 
                        students.Add(student);
                    }
                }
                return students;
            }

            public static void WriteToJson(University university)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };
                string json = JsonConvert.SerializeObject(university, settings);
                File.WriteAllText("result.json", json);
            }

            static void Main(string[] args)
            {
                var students = ReadStudentsFromCsv();
                var university = new University
                {
                    Author = "Anna Voitenkova",
                    CreatedAt = DateTime.Now,
                    Students = students
                };
                WriteToJson(university);
            }



        }
    }
}
