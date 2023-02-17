using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace JsonConverter
{
    public class JSONStructure
    {
        public University University { get; set; }
        
    }

    public class University
    {
        public DateTime CreatedAt { get; set; }
        public string Author { get; set; }
        public HashSet<Student> Students { get; set; }
        public List<ActiveStudies> ActiveStudies { get; set; }
    }

    public class ActiveStudies
    {
        public string Name { get; set; }
        public int NumberOfStudents { get; set; }

        public static int CalculateStudents(HashSet<Student> studentList, string studyName)
        {
            int totalCount = 0;
            foreach(Student student in studentList)
            {
                if (student.Studies.Name == studyName) totalCount++;
            }
            return totalCount;
        }

        public static HashSet<string> UniqueStudies(HashSet<Student> studentSet)
        {
            HashSet<string> uniqueStudies = new HashSet<string>();
            foreach(Student student in studentSet)
            {
                uniqueStudies.Add(student.Studies.Name);
            }

            return uniqueStudies;
        }
    }

    public class ActiveStudiesJSONGenerator
    {
        public static List<ActiveStudies> GetActiveStudiesList(HashSet<Student> studentSet)
        {
            HashSet<string> studiesNamesSet = ActiveStudies.UniqueStudies(studentSet);
            List<ActiveStudies> studiesList = new List<ActiveStudies>();

            foreach (string studName in studiesNamesSet)
            {
                studiesList.Add(new ActiveStudies { Name = studName, NumberOfStudents = ActiveStudies.CalculateStudents(studentSet, studName) });
            }

            return studiesList;
        }
    }

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



    internal class Program
    {
        public static HashSet<Student> ReadStudentsFromCsv(string csvPath)
        {
            var students = new HashSet<Student>(); // custom comparer for hashset

            using (var reader = new StreamReader(csvPath))
            using (var logWriter = new StreamWriter("./log.txt", true))
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

        public static void WriteToJson(JSONStructure output, string outputPath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatString = "dd.MM.yyyy"
            };
            string json = JsonConvert.SerializeObject(output, settings);
            File.WriteAllText(outputPath, json);
        }

        static void Main(string[] args)
        {
            string csvFilePath = "./dane.csv";
            Console.WriteLine("Enter path to CSV file (press enter to use default path './dane.csv'): ");
            Console.WriteLine("Example input: C:\\Users\\borsu\\OneDrive\\Desktop\\file.csv - note that the file name has to be specified!");
            string csvInputPath = Console.ReadLine().Trim();
            if (!string.IsNullOrEmpty(csvInputPath))
            {
                if (File.Exists(csvInputPath))
                {
                    csvFilePath = csvInputPath;
                }
                else
                {
                    using (var logWriter = new StreamWriter("./log.txt"))
                    {
                        while (!File.Exists(csvInputPath))
                        {
                            logWriter.WriteLine($"{DateTime.Now} Incorrect path entered");
                            Console.WriteLine("Invalid path. Please enter a valid path to CSV file: ");
                            csvInputPath = Console.ReadLine().Trim();
                        }
                        csvFilePath = csvInputPath;
                    }
                }
            }

            Console.Write("Enter destination path for JSON file (press enter to use default path './result.json'): ");
            string jsonInputPath = Console.ReadLine().Trim();
            string jsonFilePath = !string.IsNullOrEmpty(jsonInputPath) ? jsonInputPath : "./result.json";

            var students = ReadStudentsFromCsv(csvFilePath);
            var activeStudies = ActiveStudiesJSONGenerator.GetActiveStudiesList(students);
            var university = new University
            {
                Author = "Anna Voitenkova",
                CreatedAt = DateTime.Now,
                Students = students,
                ActiveStudies = activeStudies
            };

            var universityFormat = new JSONStructure
            {
                University = university
            };
            WriteToJson(universityFormat, jsonFilePath);
        }


    }

}
