using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JsonConverter
{
    public class Studies : IEquatable<Studies>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mode")]
        public string Mode { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Studies);
        }

        public bool Equals(Studies other)
        {
            return other != null &&
                   Name == other.Name &&
                   Mode == other.Mode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Mode);
        }
    }

    public class Student : IEquatable<Student>
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as Student);
        }

        public bool Equals(Student? other)
        {
            return other is not null &&
                   IndexNumber == other.IndexNumber &&
                   Name == other.Name &&
                   LastName == other.LastName &&
                   DateOfBirth == other.DateOfBirth &&
                   Email == other.Email;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IndexNumber, Name, LastName, DateOfBirth, Email);
        }

        public static bool operator ==(Student? left, Student? right)
        {
            return EqualityComparer<Student>.Default.Equals(left, right);
        }

        public static bool operator !=(Student? left, Student? right)
        {
            return !(left == right);
        }
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
            using (var logWriter = new StreamWriter("./log.txt"))
            {
                string line;
                int lineCounter = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCounter++;
                    string[] columns = line.Split(',', StringSplitOptions.RemoveEmptyEntries); // added StringSplitOptions to make columns.Length relevant

                    if (columns.Length != 9)
                    {
                        logWriter.WriteLine($"(Line {lineCounter}) - Rejected input: {line}");
                        Console.WriteLine("An incomplete line added to log.txt");
                        continue;
                    }

                    var name = columns[2];
                    var mode = columns[3];
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
                            Name = name,
                            Mode = mode
                        }

                    };
                    // TODO: check for duplicates(done using the implemented Equals and GetHashCode methods in Student)
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
