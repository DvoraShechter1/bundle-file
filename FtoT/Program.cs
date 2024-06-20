using System.Collections;
using System.CommandLine;
using System.Diagnostics.SymbolStore;

//סוגי משתנים שהפעולה יכולה לקבל
var bundleOption = new Option<DirectoryInfo>("--input", "Folder path and name");
var bundleOption1 = new Option<string>("--language", "list of language");
var bundleOption2 = new Option<FileInfo>("--output", "File path and name");
var bundleOption3 = new Option<bool>("--note", "source");
var bundleOption4 = new Option<bool>("--sort", "type order");
var bundleOption5 = new Option<string>("--author", "author");
var bundleOption6 = new Option<bool>("--delEmpty","delate empty lines from the sourse file");

//יצירת פעולה
var bundleCommand = new Command("bundle", "Bundle code filed to a single file");

//הוספת המשתנים לפעולה
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(bundleOption1);
bundleCommand.AddOption(bundleOption2);
bundleCommand.AddOption(bundleOption3);
bundleCommand.AddOption(bundleOption4);
bundleCommand.AddOption(bundleOption5);
bundleCommand.AddOption(bundleOption6);

//הגדרת מערך סיומות קבצים
string[] extension = { "css", "html", "js", "py", "java", "cs", "sql", "cpp" };

//הגדרת מחרוזת לתוכה נכניס את תוכן הקובץ ובסוף הפעולה נכתוב אותה בקובץ היעודי
string s = "";

//פונקציה שמוחקת שורות ריקות מקובץ המקור
void delEmptyLine(string file)
{
    var lines = File.ReadAllLines(file).Where(arg => !string.IsNullOrWhiteSpace(arg));
    File.WriteAllLines(file, lines);
}

//פונקציה שמטפלת בתקייה
void folder(string pathIn,string pathOut,bool sort,bool delEmpty)
{
    //מערך של כלל הקבצים בתקייה
    string[] files=Directory.GetFiles(pathIn);

    //מערך המכיל את כל הקבצים עם הסיומות המתאימות
    List<string> goodFiles = new List<string>();
    
    foreach(string file in files)
    {
        string ex = file.Substring(file.IndexOf('.') + 1);
        for (int i = 0; i < extension.Length; i++)
            if (ex.CompareTo(extension[i]) == 0)//אם סיומת הקובץ קיימת במערך הסיומות
            {
                goodFiles.Add(file);
                break;
            }
    }
    if (sort)
        goodFiles.Sort(((q, p) => q.Substring(q.LastIndexOf('.') + 1).CompareTo(p.Substring(p.LastIndexOf('.') + 1))));
    else
        goodFiles.Sort();
    if (goodFiles.Count() != 0)//אם בתקייה המקומית יש קבצים שצריך להעתיק
    {
        string name = pathIn.Substring(pathIn.LastIndexOf('\\') + 1);
        //כותרת של שם התיקיה
        s += "//folder: "+name+"\n";

        foreach (string file in goodFiles)
        {
            if(delEmpty)
                delEmptyLine(file);
            s += "//file: " + file.Substring(file.LastIndexOf('\\') + 1) + "\n";
            s += File.ReadAllText(file);
        }
    }
    //עובר על תקיות מקומיות שיש במיקום הנוכחי
    string[] folders = Directory.GetDirectories(pathIn);
    for (int i = 0; i < folders.Length; i++)
        folder(folders[i], pathOut,sort, delEmpty);
}

//הפעולה
bundleCommand.SetHandler((input,language,output, note,sort, author,delEmpty) =>
{

    if (author != null)
    {
        s += "©: " + author + ".\n";
    }
    if (note==true)
    {
        s += "source: " + output.FullName + "\n";
    }
    if (language!=null && language != "all")//שינוי מערך סיומות קבצים
    {
        extension = language.Split(' ');
    }
    try
    {
        folder(input.FullName, output.FullName,sort, delEmpty);
        File.WriteAllText(output.FullName, s);
        Console.WriteLine("file was created");
    }
    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine("error: file path is invalid");
    }
    catch 
    { 
        Console.WriteLine("failed"); 
    }

}, bundleOption,bundleOption1, bundleOption2, bundleOption3,bundleOption4,bundleOption5,bundleOption6);


var rootCommand = new RootCommand("Root command for File Bundler CLI");
rootCommand.AddCommand(bundleCommand);


//===================================== יצירת קובץ תגובה ===================================

var rspCommand = new Command("rsp","use a rsp file");
rspCommand.SetHandler(() =>
{
    Console.WriteLine("hello");
    string file = "", s;
    try
    {
        Console.WriteLine("input the file's directory");
        s=Console.ReadLine();
        file +="--input "+s;
        Console.WriteLine("input the directory for the new file and its name");
        s= Console.ReadLine();
        file+=" --output "+s;
        Console.WriteLine("input a list of the file types you want to add (spaces between each) or input all for adding all the files");
        s=Console.ReadLine();
        file+=" --language "+s;
        Console.WriteLine("if you want to show the files original direction input 1 else input 0");
        s= Console.ReadLine();
        if (s=="0")
            file+=" --note false";
        else if (s=="1")
            file+=" --note true";
        Console.WriteLine("if you want the files to be copied ordered acoording to thier types input 1 otherwise input 0");
        s=Console.ReadLine();
        if (s=="0")
            file+=" --sort false";
        else if (s=="1")
            file+=" --sort true";
        Console.WriteLine("if you want the aouthors name added to the file input his name, otherwise input 1");
        s=Console.ReadLine();
        if (s!="0" && s!=null)
            file+=" --author "+s;
        Console.WriteLine("if you want to delete the empty lines from input 1, otherwise input 0");
        s= Console.ReadLine();
        if (s=="0")
            file+=" --delEmpty false";
        else if (s=="1")
            file+=" --delEmpty true";

        //יצירת קובץ התגובה במיקום הנוכחי
        File.WriteAllText("file.rsp", file);

        Console.WriteLine("for creating your wanted file in the next cmd line input 'FtoT bundle @file.rsp' ");
    }
    catch { Console.WriteLine("failed"); }

});

rootCommand.AddCommand(rspCommand);

rootCommand.InvokeAsync(args);
