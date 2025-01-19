using LakeHouseFileDirectoryOperations;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Security;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Text;

namespace Action
{
    public class Actions
    {
        public static async Task _DeleteDirectory(string directoryfullpath)
        {

            Program.dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{directoryfullpath}?restype=directory&recursive=true";
            try
            {
                await Http.HttpMethods.DeleteAsync(Program.dfsendpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Directory deletion failed : " + ex.Message);
            }
        }

        public static async Task UploadFilesToLakeHouse(string uploadpath, string lakehousedirectory)
        {

            DirectoryInfo d = new DirectoryInfo(uploadpath);
            byte[] bytes;

            foreach (FileInfo file in d.GetFiles())
            {
                using (Stream stream = File.OpenRead(file.FullName))
                {
                    string RequestUri = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{lakehousedirectory}/{file.Name}?resource=file";
                    string jsonString = System.String.Empty;
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await Http.HttpMethods.PutAsync(RequestUri, content);
                    await FileStreamSendAsync(stream, lakehousedirectory, file.Name);
                }

            }

        }

        public static async Task DownloadFilesFromLakeHouse(string lakehouse_directoryfullpath, string local_directoryfullpath)
        {
            string dfsendpoint = "";

            dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{lakehouse_directoryfullpath}";

            var downloadMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(dfsendpoint)
            };

            try
            {
                var response_d = await Http.HttpMethods.SendAsync(downloadMessage);
                string filename_n = await GetDirectoryName(lakehouse_directoryfullpath);
                File.WriteAllBytes($"{local_directoryfullpath}\\{filename_n}", response_d);
            }
            catch (Exception ex)
            {
                Console.WriteLine("File download failed : " + ex.Message);

            }

        }

        public static async Task RenameFile(string oldfilename, string newfilename, string directoryfullpath)
        {
            try
            {
                Program.dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{directoryfullpath}/{newfilename}?restype=directory&comp=rename";
                var Metadata = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(Program.dfsendpoint)
                };
                Metadata.Headers.Add("x-ms-rename-source", $"/My_Workspace/LakeHouse_1.Lakehouse/{directoryfullpath}/{oldfilename}");//Source directory
                var byte_response = await Http.HttpMethods.SendAsync(Metadata);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async static Task<string> FileStreamSendAsync(Stream stream, string directory, string filename)
        {
            AuthenticationResult result = await Authentication.ReturnAuthenticationResult();

            Security.Authentication.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            var content = new StreamContent(stream);
            string url = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{directory}/{filename}?action=append&position=0";
            var streamMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(url),
                Content = content
            };
            HttpResponseMessage response = await Http.HttpMethods.client.SendAsync(streamMessage);

            try
            {
                response.EnsureSuccessStatusCode();
                await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                return null;
            }
            if (response.IsSuccessStatusCode)
            {
                url = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{directory}/{filename}?action=flush&position={stream.Length}";
                Http.HttpMethods.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                var flushMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri(url)
                };
                response = await Http.HttpMethods.client.SendAsync(flushMessage);
                response.EnsureSuccessStatusCode();
            }
            return null;
        }

        public static async Task Create_Directory(string directoryfullpath)
        {
            string jsonString = System.String.Empty;
            Program.dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{directoryfullpath}?resource=directory";
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                await Http.HttpMethods.PutAsync(Program.dfsendpoint, content);
                AnsiConsole.MarkupLine("");
                AnsiConsole.MarkupLine($"[blue]Success[/] : Directory [Yellow]{await GetDirectoryName(directoryfullpath)}[/] successfully created in lakehouse : [Yellow]{Program.lakeHouse}[/]");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Directory creation failed : " + ex.Message);

            }
        }


        public static async Task Rename_Directory(string old_directoryfullpath, string new_directoryfullpath)
        {
            Program.dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{new_directoryfullpath}?restype=directory&comp=rename";
            var Metadata = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Program.dfsendpoint)
            };
            Metadata.Headers.Add("x-ms-rename-source", $"/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{old_directoryfullpath}");

            try
            {
                await Http.HttpMethods.SendAsync(Metadata);
                AnsiConsole.MarkupLine("");
                AnsiConsole.MarkupLine($"[blue]Success[/] : Directory [Yellow]{await GetDirectoryName(old_directoryfullpath)}[/] successfully renamed to [Yellow]{await GetDirectoryName(new_directoryfullpath)}[/] in lakehouse : [Yellow]{Program.lakeHouse}[/]");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Directory rename failed : " + ex.Message);

            }
        }

        public static async Task<string> GetDirectoryName(string value)
        {
            int lastSlashIndex_n = value.ToString().LastIndexOf('/');
            string dir_n = value.ToString().Substring(lastSlashIndex_n + 1);
            return dir_n;
        }


        public static async Task<JObject> TraverseAllLakeHousesInWorkspace(string workspace)
        {
            string dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{workspace}" + $"?resource=filesystem&recursive=false";
            string response = await Http.HttpMethods.GetAsync(dfsendpoint);
            if (response == null)
            {
                return null;
            }
            else
            {
                JObject jsonObject_lakehouse = JObject.Parse(response);
                return jsonObject_lakehouse;
            }
        }

        public static async Task<SelectionPrompt<string>> TraverseAllDirectoriesInLakeHouse(string lakehouse)
        {
            int i = 1;
            string dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/Files?resource=filesystem&recursive=true";
            string response = await Http.HttpMethods.GetAsync(dfsendpoint);
            JObject jsonObject_dir = JObject.Parse(response);
            JArray dirArray = (JArray)jsonObject_dir["paths"];
            var prompt = new SelectionPrompt<string>();
            prompt.AddChoice($"{i}.\tFiles");
            i = i + 1;
            foreach (JObject dir in dirArray)
            {
                if (dir["isDirectory"] != null)
                {
                    int lastSlashIndex_n = dir["name"].ToString().IndexOf('/');
                    string dir_n = dir["name"].ToString().Substring(lastSlashIndex_n + 1);
                    prompt.AddChoice($"{i}.\t{dir_n.ToString()}"); i++;
                }
            }

            return prompt;
        }

        public static async Task<MultiSelectionPrompt<string>> TraverseAllFilesInDirectories(string directorypath)
        {
            if (directorypath == "" && Program.Dir != "")
            {
                int tabIndex = Program.Dir.IndexOf('\t');
                directorypath = Program.Dir.Substring(tabIndex + 1);
            }
            string dfsendpoint = $"https://onelake.dfs.fabric.microsoft.com/{Program.workSpace}/{Program.lakeHouse}.Lakehouse/{directorypath}?resource=filesystem&recursive=false";
            string response = await Http.HttpMethods.GetAsync(dfsendpoint);
            JObject jsonObject_dir = JObject.Parse(response);
            JArray dirArray = (JArray)jsonObject_dir["paths"];
            var prompt = new MultiSelectionPrompt<string>();
            List<string> lst = new List<string>();

            foreach (JObject dir in dirArray)
            {
                if (dir["isDirectory"] == null)
                {
                    // prompt.AddChoices(await GetDirectoryName(dir["name"].ToString());
                    //   prompt.AddChoiceGroup("Files",await GetDirectoryName(dir["name"].ToString()));
                    lst.Add(await GetDirectoryName(dir["name"].ToString()));
                }
            }
            if (lst.Count > 0) { prompt.AddChoiceGroup("File List", lst); }
            prompt.AddChoice("[red]<< Back To Directory List >>[/]");
            return prompt;
        }

    }
}
