using System.CommandLine;
using System.Diagnostics;

var bundleCommand = new Command("Bundle", "Bundle code files to a single file");
var createRspCommand = new Command("CreateRsp", "Create response file");

var bundleOption = new Option<FileInfo>("--output", "File path and name") { IsRequired = false };
var bundleLanguage = new Option<String>("--language", "Check language") { IsRequired=true};
var bundleNote = new Option<Boolean>("--note", "Comment with the source code") { IsRequired = false };
var bundleSort = new Option<String>("--sort", "sort by type or name") { IsRequired = false };
var bundleRemoveEmptyLine = new Option<Boolean>("--remove", "remove empty lines") { IsRequired = false };
var bundleAuthor = new Option<String>("--author", "Author of file") { IsRequired = false };

bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(bundleLanguage);
bundleCommand.AddOption(bundleNote);
bundleCommand.AddOption(bundleSort);
bundleCommand.AddOption(bundleRemoveEmptyLine);
bundleCommand.AddOption(bundleAuthor);

bundleOption.AddAlias("--o");
bundleLanguage.AddAlias("--l");
bundleNote.AddAlias("--n");
bundleSort.AddAlias("--s");
bundleRemoveEmptyLine.AddAlias("--r");
bundleAuthor.AddAlias("--a");

static string ReadNonEmptyLines(string file)
{
    string fileContent = "";

    try
    {
        fileContent = string.Join(Environment.NewLine,
            File.ReadAllLines(file)
                .Where(line => !string.IsNullOrWhiteSpace(line)));
    }
    catch (Exception)
    {
        throw new Exception();
    }

    return fileContent;
}
static List<String> SortFiles(String sort, List<String> files)
{
    if (sort == "type")
    {
        List<String> sortedByExtension = files.OrderBy(name => Path.GetExtension(name).Substring(1)).ToList();
        return sortedByExtension;
    }
    List<String> sortedByName = files.OrderBy(name => name).ToList();
    return sortedByName;
}


    static void ProcessFilesInDirectory(string directoryPath, string output,string[] languages,bool note,string sort,bool remove, string author)
{
    List<string> files = new List<string>();
    List<string> chosenFiles = new List<string>();

    try
    {
        if (Directory.Exists(directoryPath))
        {
             files = Directory.GetFiles(directoryPath).ToList();

            using (StreamWriter writer = new StreamWriter(output))
            {
                if (languages[0] != "all")
                {
                    foreach (string file in files)
                    {
                        if (Array.Exists(languages, element => element == Path.GetExtension(file).Substring(1)))
                        {
                            chosenFiles.Add(file);
                        }
                    }
                    files = chosenFiles;

                }
                files=SortFiles(sort, files);
                if (author != null)
                {
                    writer.WriteLine($"Author: {author}\n");
                }
                foreach (string file in files)
                {
                    string fileContent = "";
                    if (remove)
                    {
                        fileContent = ReadNonEmptyLines(file);
                    }
                    else
                    {
                        fileContent = File.ReadAllText(file);
                    }
                    if (note)
                    {
                        writer.WriteLine($"//File: {Path.GetFileName(file)}\n");
                    }
                        writer.WriteLine($"{fileContent}\n");    
                }

            }

        }
        else
        {
            Console.WriteLine("The specified folder does not exist.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}


void Languageparse( string  languages,bool note, string directoryPath,string output,string sort,bool remove,string author)
{
    string currentPath = Environment.CurrentDirectory;
    string[] lan = languages.ToString().Split(',');
    ProcessFilesInDirectory(currentPath, output, lan,note,sort,remove,author);
}
static void RunCommand(string command)
{
    Console.WriteLine(command);
    try
    {
        using (Process process = new Process())
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.Start();

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(command);
                }
            }

            process.WaitForExit();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running command: {ex.Message}");
    }
}
static void CreateResponseFile()
{
    string[] questions = { "Question 1: What is the bundle file name ?",
                           "Question 2: What is the file type you want to bundle?",
                           "Question 3: Do you  want a comment to be written before each file?",
                           "Question 4:Accoroding what you want the file to be sorted by, name or type?",
                           "Question 5: Do you want to remove empty line?",
                           "Question 6: What your name?"};
    string[] options = { "--output", "--language", "--note", "--sort", "--remove", "--author" };
    string responseFilePath = "response_file.txt";

    Console.WriteLine("Creating response file...");
    using (StreamWriter writer = new StreamWriter(responseFilePath))
    {
        string userInput = "";
        for (int i = 0; i < questions.Length; i++)
        {
            if (i == 0 || i==1)
            {
                do
                {
                    Console.Write($"{questions[i]} ");
                    userInput = Console.ReadLine();
                } while (string.IsNullOrEmpty(userInput));
            }
            else
            {
                Console.Write($"{questions[i]} ");
                userInput = Console.ReadLine();
            }

           
           if(!string.IsNullOrEmpty(userInput))
            {
                writer.WriteLine($"{options[i]} {userInput}");
            }

        }
    }

    Console.WriteLine($"Response file created: {responseFilePath}");
    string[] responseLines = File.ReadAllLines(responseFilePath);
    string lines = "";
    foreach (string line in responseLines)
    {
        Console.WriteLine($"Processing line: {line}");
        lines=lines+" "+line;
    }
    Console.WriteLine(lines);
    RunCommand($"CliProject Bundle {lines}");
}
bundleCommand.SetHandler((output,language,note,sort,remove, author) =>
{
try
    {
        var directoryPath = Path.Combine(output.DirectoryName);
        Languageparse(language,note, directoryPath,output.FullName,sort,remove,author);
    }
    catch(DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error:File path is invalid");
    }
}, bundleOption, bundleLanguage,bundleNote,bundleSort,bundleRemoveEmptyLine,bundleAuthor);

createRspCommand.SetHandler(() =>
{
    CreateResponseFile();
});
var rootCommand = new RootCommand("Root Commanr for File Bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);