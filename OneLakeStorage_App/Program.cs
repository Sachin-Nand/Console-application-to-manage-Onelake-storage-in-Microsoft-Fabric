using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Security;
using Spectre.Console;

namespace LakeHouseFileDirectoryOperations
{
    public class Program : Action.Actions
    {
        private static string DownloadLocation = "";
        public static string workSpace = "";
        public static string lakeHouse = "";
        public static string dfsendpoint = "";
        public static string operation = "";
        public static string Dir = "";
        public static string Filepath = "";
        public static string selectedOption = "";
        public static string currlakehouse = "";
        public static async Task Main(string[] args)
        {

            ReadConfig();

            await ConfirmWorkspace();

            while (Dir.Contains("Files") || Dir.Contains("Back To Lakehouse"))
            {

                AnsiConsole.Clear(); Header(); CurrentWorkspaceLakehouse();
                await MainMenu(selectedOption);

            }

        }

        public static void ReadConfig()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            Security.Authentication.clientId = config["ClientId"];
            DownloadLocation = config["DownloadLocation"];
        }
        public static void Header()
        {
            Console.Title = "Microsoft Fabric Command Line";
            AnsiConsole.MarkupLine($".NET Version: [blue]{Environment.Version.ToString()}[/]");
            AnsiConsole.Write(
               new FigletText("Microsoft Fabric Command Line Tool")
            .Centered()
            .Color(Color.Red));

        }

        public async static Task ConfirmWorkspace()
        {
            Header();
            AnsiConsole.MarkupLine("");
            workSpace = AnsiConsole.Prompt(
            new TextPrompt<string>("Please Enter the Workspace : "));
            AnsiConsole.MarkupLine("");
            var confirmation_1 = AnsiConsole.Prompt(
            new TextPrompt<bool>($"Is the workspace [Yellow]{workSpace}[/] that you want to use ?")
           .AddChoice(true)
           .AddChoice(false)
           .DefaultValue(true)
           .WithConverter(choice => choice ? "yes" : "no"));
            string str = confirmation_1 ? "Confirmed" : "Declined";
            if (str == "Declined")
            { AnsiConsole.Clear(); await ConfirmWorkspace(); }
            else await MainMenu("");
        }

        public static void CurrentWorkspaceLakehouse()
        {

            AnsiConsole.MarkupLine($"Your current workspace is [Yellow]{workSpace}[/] and lakehouse is [Yellow]{lakeHouse}[/] ");
            AnsiConsole.MarkupLine("");
        }
        public async static Task DisplayLakehouses()
        {

            int i = 1;
            JObject jobject = await TraverseAllLakeHousesInWorkspace(workSpace);
            if (jobject == null)
            {
                AnsiConsole.MarkupLine($"Your current workspace [red]{workSpace}[/] is invalid ");
                Thread.Sleep(2500);
                AnsiConsole.Clear();
                await ConfirmWorkspace();

            };
            var lkhouse = new List<string>();
            AnsiConsole.Status()
                .Start($"Getting a list of lakehouses from the workspace [Yellow]{workSpace}[/]...", ctx =>
                {

                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("Yellow"));
                    JArray pathsArray = (JArray)jobject["paths"];

                    foreach (JObject path in pathsArray)
                    {
                        if (path["name"].ToString().Contains(".Lakehouse"))
                        {
                            lkhouse.Add(path["name"].ToString().Replace(".Lakehouse", ""));
                        }
                    }
                    Thread.Sleep(1500);
                    ctx.Status("Done ..");
                    Thread.Sleep(1000);

                });

            var prompt = new SelectionPrompt<string>();

            foreach (var obj in lkhouse)
            {
                prompt.AddChoice($"{i}.\t{obj.ToString()}"); i++;

            }
            prompt.AddChoice($"[red]{""} \t<< Main Menu >>[/]");
            AnsiConsole.Clear();
            Header();
            AnsiConsole.MarkupLine($"Your current workspace is [Yellow]{workSpace}[/]");
            AnsiConsole.MarkupLine("");
            prompt.Title("Which lakehouse would you like to interact with ?")
                    .PageSize(30)
                    .MoreChoicesText("[grey](Move up and down to reveal more)[/]");

            lakeHouse = AnsiConsole.Prompt(prompt);
            lakeHouse = lakeHouse.Replace(lakeHouse.Substring(0, lakeHouse.IndexOf("\t") + 1), "");

            if (lakeHouse.Contains("Main Menu"))
            { await MainMenu(""); }

        }
        public static async Task<SelectionPrompt<string>> MiscOperationsInPrompt(string operation)
        {
            var dirprompt = new SelectionPrompt<string>();
            if (Dir == "") { await DisplayLakehouses(); }
            AnsiConsole.Clear();
            Header();
            dirprompt = await TraverseAllDirectoriesInLakeHouse(lakeHouse);
            dirprompt.AddChoice("[red]<< Back To Lakehouse List >>[/]");
            dirprompt.Title("Which directory would you like to interact with ?")
                   .PageSize(30)
                   .MoreChoicesText("[grey](Move up and down to reveal more lakehouses)[/]");
            AnsiConsole.MarkupLine($"Your current workspace is [Yellow]{workSpace}[/] and lakehouse is [Yellow]{lakeHouse}[/] ");
            AnsiConsole.MarkupLine("");
            return dirprompt;
        }

        public static async Task<string> ListOperationsInPrompt(string operation)
        {
            var dirprompt_s = new SelectionPrompt<string>();
            var dirprompt_m = new MultiSelectionPrompt<string>();
            string title = "";
            if (!Dir.Contains("Files"))
            {

                if (operation == "1.\tLakeHouse directory structure")
                {

                    title = "Select the directory to view its structure";
                }

                if (operation == "2.\tCreate a lakehouse directory")
                {

                    title = "Which location would you like to create a directory ?";
                }

                if (operation == "3.\tRename a lakehouse directory")
                {

                    title = "Which directory would you like to rename ?";
                }

                if (operation == "4.\tDelete a lakehouse directory")
                {

                    title = "Which directory would you like to delete ?";
                }

                if (operation == "5.\tDownload files from a lakehouse directory")
                {

                    title = "Files from which directory would you like to download ?";

                }
                if (operation == "6.\tUpload files to a lakehouse directory")
                {

                    title = "Directory to which files to be uploaded";

                }
                if (Dir.Contains("Back To Lakehouse List"))
                { await DisplayLakehouses(); }
                dirprompt_s = await MiscOperationsInPrompt(operation);
                dirprompt_s.Title(title)
                       .PageSize(30)
                       .MoreChoicesText("[grey](Move up and down to reveal more directories)[/]");
                Dir = AnsiConsole.Prompt(dirprompt_s);

            }
            return Dir;
        }

        public static async Task<string> ReturnFileName()
        {
            int tabIndex = Dir.IndexOf('\t');
            return Dir.Substring(tabIndex + 1);
        }

        public static async Task MainMenu(string operation)
        {

            const string LakeHouseStructure = "1.\tLakeHouse directory structure";
            const string CreateDirectory = "2.\tCreate a lakehouse directory";
            const string RenameDirectory = "3.\tRename a lakehouse directory";
            const string DeleteDirectory = "4.\tDelete a lakehouse directory";
            const string DownloadFiles = "5.\tDownload files from a lakehouse directory";
            const string UploadFiles = "6.\tUpload files to a lakehouse directory";
            const string ChangeWorkspace = "7.\tChange Workspace";
            const string exit = "8.\tExit this Application";

            AnsiConsole.Clear();
            Header();
            AnsiConsole.MarkupLine($"Your current workspace is [Yellow]{workSpace}[/]");
            AnsiConsole.MarkupLine("");
            selectedOption = "";
            if (operation == "")
            {
                selectedOption = AnsiConsole.Prompt(
               new SelectionPrompt<string>()
                 .Title("Select an option to continue")
                 .PageSize(30)
                 .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                 .AddChoices(new[] {
                           LakeHouseStructure,CreateDirectory,RenameDirectory,DeleteDirectory,DownloadFiles,UploadFiles,ChangeWorkspace,exit
                 }));
            }
            else
            {
                selectedOption = operation;
            }

            if (Authentication.istokencached == false)
            {
                AnsiConsole.Clear();
                Header();
                AnsiConsole.MarkupLine("[red]Note[/] : You will be prompted in a new browser window to enter the credentials of your fabric tenant!!!");
                Thread.Sleep(1500);

            }

            var fileprompt_m = new MultiSelectionPrompt<string>();
            var dirprompt_s = new SelectionPrompt<string>();
            switch (selectedOption)
            {

                case LakeHouseStructure:
                    operation = "1.\tLakehouse directory structure";
                    dirprompt_s = await MiscOperationsInPrompt(operation);
                    Dir = AnsiConsole.Prompt(dirprompt_s);
                    if (Dir == "Main Menu" || Dir.Contains("Back To Lakehouse")) { Dir = ""; await MainMenu(operation); }
                    else
                    {
                        Filepath = await ReturnFileName();
                        fileprompt_m = await TraverseAllFilesInDirectories(Filepath.ToString());
                        var filename_d = AnsiConsole.Prompt(fileprompt_m);
                    }
                    dirprompt_s = await MiscOperationsInPrompt(operation);
                    Dir = AnsiConsole.Prompt(dirprompt_s);
                    await ListOperationsInPrompt(operation);
                    break;

                case CreateDirectory:
                    operation = "2.\tCreate a lakehouse directory";
                    Dir = await ListOperationsInPrompt(operation);
                    if (Dir == "Main Menu" || Dir.Contains("Back To Lakehouse")) { Dir = ""; await MainMenu(operation); }
                    else

                    {
                        Filepath = await ReturnFileName();
                        var new_dir_n = AnsiConsole.Prompt(
                           new TextPrompt<string>(($"Enter the name for the new directory in [Yellow]{Filepath}[/] : ")));
                        await Create_Directory($"{Filepath}/{new_dir_n}");

                    };
                    dirprompt_s = await MiscOperationsInPrompt(operation);
                    Dir = AnsiConsole.Prompt(dirprompt_s);
                    await ListOperationsInPrompt(operation);

                    break;

                case RenameDirectory:
                    operation = "3.\tRename a lakehouse directory";
                    Dir = await ListOperationsInPrompt(operation);
                    if (Dir == "Main Menu") { await MainMenu("RenameDirectory"); }
                    if (Dir.Contains("Back To Lakehouse")) { Dir = ""; await TraverseAllLakeHousesInWorkspace(workSpace); }

                    else
                    {
                        int lastSlashIndex = Dir.ToString().LastIndexOf('\t');
                        string dir_n = Dir.ToString().Substring(lastSlashIndex + 1);

                        if (Dir == "1.	Files")
                        {
                            AnsiConsole.MarkupLine($"Cannot rename directory [Yellow]{dir_n}[/]");
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            var new_dir_n = AnsiConsole.Prompt(
                             new TextPrompt<string>(($"Enter the new name for the directory [Yellow]{dir_n}[/] : ")));

                            int index = Dir.IndexOf(dir_n);
                            lastSlashIndex = Dir.ToString().LastIndexOf('/');
                            if (index != -1)
                            {

                                Filepath = await ReturnFileName();
                                int tabIndex = (Dir.Substring(0, lastSlashIndex + 1) + new_dir_n).IndexOf('\t');
                                string newFile = (Dir.Substring(0, lastSlashIndex + 1) + new_dir_n).Substring(tabIndex + 1);
                                await Rename_Directory(Filepath, newFile);
                            }
                        }
                    };
                    dirprompt_s = await MiscOperationsInPrompt(operation);
                    Dir = AnsiConsole.Prompt(dirprompt_s);
                    await ListOperationsInPrompt(operation);
                    break;

                case DeleteDirectory:

                    operation = "4.\tDelete a lakehouse directory";

                    Dir = await ListOperationsInPrompt(operation);

                    if (Dir == "Main Menu")
                    { await MainMenu("DeleteDirectory"); }

                    if (Dir.Contains("Back To Lakehouse") || Dir == "")
                    {
                        Dir = ""; await MainMenu("");
                        break;
                    }

                    else
                    {
                        Filepath = await ReturnFileName();

                        var confirmation = AnsiConsole.Prompt(
                         new TextPrompt<bool>($"Are you sure you want to delete this directory ?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(true)
                        .WithConverter(choice => choice ? "yes" : "no"));

                        if (confirmation == true)
                        {
                            await _DeleteDirectory(Filepath);
                            AnsiConsole.MarkupLine("");
                            AnsiConsole.MarkupLine($"Successfully deleted directory [Yellow]{await GetDirectoryName(Dir)}[/]");
                            Thread.Sleep(2000);
                            dirprompt_s = await MiscOperationsInPrompt(operation);
                            Dir = AnsiConsole.Prompt(dirprompt_s);
                            await ListOperationsInPrompt(operation);
                            break;
                        }

                        else
                        {
                            Dir = ""; await MainMenu(operation);
                            break;

                        }


                    }

                case DownloadFiles:

                    operation = "5.\tDownload files from a lakehouse directory";
                    if (Dir == "")
                    {
                        Dir = await ListOperationsInPrompt(operation);
                    }
                    if (Dir.Contains("Back To Lakehouse"))
                    {
                        Dir = await ListOperationsInPrompt(operation);
                    }

                    var filename = new List<string>();
                    Filepath = await ReturnFileName();
                    fileprompt_m = await TraverseAllFilesInDirectories(Filepath.ToString());

                    filename = AnsiConsole.Prompt(fileprompt_m);
                    int i = 0; int j = 0;
                    foreach (string flname in filename)
                    {
                        if (flname.ToString().Contains("Back To Directory"))
                        {
                            if (i > 0)
                            {
                                AnsiConsole.MarkupLine("");
                                AnsiConsole.MarkupLine($"[Yellow]All files downloaded succesfully[/]");
                                Thread.Sleep(2000);
                                AnsiConsole.Clear();
                                Header();
                                CurrentWorkspaceLakehouse();
                                j = 1;
                            }
                        }
                        else
                        {
                            i++;
                            int tabIndex = Dir.IndexOf('\t');
                            Filepath = Dir.Substring(tabIndex + 1);
                            await DownloadFilesFromLakeHouse(Filepath + "//" + flname, DownloadLocation);

                        }

                    }
                    if (i > 0 && j == 0)
                    {
                        AnsiConsole.MarkupLine("");
                        AnsiConsole.MarkupLine($"All files downloaded succesfully from [Yellow]{Filepath}[/] to [Yellow]{DownloadLocation}[/]");
                        Thread.Sleep(2000);
                        AnsiConsole.Clear();
                        Header();
                        CurrentWorkspaceLakehouse();
                    }
                    dirprompt_s = await MiscOperationsInPrompt(operation);
                    Dir = AnsiConsole.Prompt(dirprompt_s);
                    await ListOperationsInPrompt(operation);
                    break;

                case ChangeWorkspace:

                    AnsiConsole.Clear();
                    await ConfirmWorkspace();
                    break;

                case UploadFiles:

                    operation = "6.\tUpload files to a lakehouse directory";

                    Dir = await ListOperationsInPrompt(operation);

                    if (Dir == "Main Menu" || Dir.Contains("Back To Lakehouse"))
                    {
                        Dir = ""; await MainMenu(operation);
                    }
                    else
                    {
                        Filepath = await ReturnFileName();
                        var local_dir_n = AnsiConsole.Prompt(
                           new TextPrompt<string>(($"Enter the local path : ")));
                        await UploadFilesToLakeHouse(local_dir_n, Filepath);
                    };
                    AnsiConsole.MarkupLine("");
                    AnsiConsole.MarkupLine($"All files uploaded succesfully to [Yellow]{Filepath}[/]");
                    Thread.Sleep(2000);
                    AnsiConsole.Clear();
                    Header();
                    CurrentWorkspaceLakehouse();
                    AnsiConsole.MarkupLine($"Uploaded files in [Yellow]{Filepath}[/]");
                    AnsiConsole.MarkupLine("");
                    Filepath = await ReturnFileName();
                    fileprompt_m = await TraverseAllFilesInDirectories(Filepath.ToString());
                    filename = AnsiConsole.Prompt(fileprompt_m);
                    dirprompt_s = await MiscOperationsInPrompt(operation);
                    Dir = AnsiConsole.Prompt(dirprompt_s);
                    await ListOperationsInPrompt(operation);
                    break;

                case exit:
                    await ExitApplication();
                    return;
            }

        }

        public static async Task ExitApplication()
        {
            AnsiConsole.MarkupLine("");
            var confirmation_3 = AnsiConsole.Prompt(
              new TextPrompt<bool>($"Are you sure you want to exit the application ?")
             .AddChoice(true)
             .AddChoice(false)
             .DefaultValue(true)
             .WithConverter(choice => choice ? "yes" : "no"));
            if (confirmation_3 == true)
            { Environment.Exit(0); }

        }
    }
}