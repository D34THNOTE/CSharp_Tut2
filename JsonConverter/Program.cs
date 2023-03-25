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
                    string[] columns = line.Split(',');

                    if (columns.Length != 9)
                    {
                        logWriter.WriteLine($"(Line {lineCounter}) - The row does not have enough columns: {line}");
                        Console.WriteLine("An incomplete line added to log.txt");
                        continue;
                    }
                    
                    if (columns.Any(string.IsNullOrWhiteSpace))
                    {
                        logWriter.WriteLine($"(Line {lineCounter}) - The row cannot have empty columns: {line}");
                        Console.WriteLine("A row with empty column(s) added to log.txt");
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
                    // checking for duplicate
                    if (!students.Add(student))
                    {
                        logWriter.WriteLine($"(Line {lineCounter}) - Duplicate: {line}");
                        Console.WriteLine("A duplicate student added to log.txt");
                    }
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
                    throw new FileNotFoundException("The specified CSV file does not exist", csvInputPath);
                }
            }
            else if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException("The default CSV file does not exist", csvFilePath);
            }

            // TODO output format concatenation string!!
            Console.WriteLine("Enter destination path for the output file (press enter to use default path './university.outputFormat'): ");
            string jsonInputPath = Console.ReadLine().Trim();
            string jsonFilePath = !string.IsNullOrEmpty(jsonInputPath) ? jsonInputPath : "./university.json";
            if (!Directory.Exists(Path.GetDirectoryName(jsonFilePath)))
            {
                throw new DirectoryNotFoundException("The specified output directory does not exist");
            }
            
            Console.WriteLine("Enter destination path for log.txt file (press enter to use default path './log.txt'): ");
            string logInputPath = Console.ReadLine().Trim();
            string logFilePath = !string.IsNullOrEmpty(logInputPath) ? logInputPath : "./log.txt";
            if (!File.Exists(logFilePath))
            {
                throw new FileNotFoundException($"The specified log file does not exist", logFilePath);
            }
            
            Console.WriteLine("Enter format of the output file (press enter to use default format \".json\"): ");
            Console.WriteLine("Currently supported: json");
            Console.WriteLine("Example input: json");
            string formatUserInput = Console.ReadLine().Trim();
            string format = !string.IsNullOrEmpty(formatUserInput) ? formatUserInput : "json";
            if (format != "json")
            {
                throw new InvalidOperationException("The specified format is not supported by the application");
            }

            var students = ReadStudentsFromCsv(csvFilePath);
            var activeStudies = ActiveStudiesJSONGenerator.GetActiveStudiesList(students);
            var university = new University
            {
                Author = "Bartosz Janowski",
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
