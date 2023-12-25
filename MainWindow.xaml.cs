// MainWindow.xaml.cs
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace modinsteller
{
    public partial class MainWindow : Window
    {
        private string selectedPath;

        public MainWindow()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string newFilePath = Path.Combine(appDataPath, "Jjoon Minecraft Insteller", "Old_Version.json");

                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                if (!File.Exists(newFilePath))
                {
                    File.WriteAllText(newFilePath, "{ \"modpack_ver\": \"2023-12-25\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            InitializeComponent();
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                string selected = dialog.SelectedPath;
                lootrabel.Content = selected;
                selectedPath = selected;
            }
        }

        private async void ver_check_click(object sender, RoutedEventArgs e)
        {
            if (selectedPath == null)
            {
                System.Windows.Forms.MessageBox.Show("먼저 마인크래프트 mods 파일을 지정해 주십시오", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string downloadPath = Path.Combine(appDataPath, "Jjoon Minecraft Insteller", "Current_Version.json");

                    await DownloadFileInBackground("https://github.com/Jjoon0513/Minecraftmoddata/raw/main/ver.json", downloadPath);

                    using (StreamReader file = File.OpenText(Path.Combine(appDataPath, "Jjoon Minecraft Insteller", "Current_Version.json")))
                    using (StreamReader file1 = File.OpenText(Path.Combine(appDataPath, "Jjoon Minecraft Insteller", "Old_Version.json")))
                    {
                        using (JsonTextReader reader = new JsonTextReader(file))
                        using (JsonTextReader reader1 = new JsonTextReader(file1))
                        {
                            JObject Current_Version = await JObject.LoadAsync(reader);
                            JObject Old_Version = await JObject.LoadAsync(reader1);

                            if (Current_Version["modpack_ver"].ToString() == Old_Version["modpack_ver"].ToString())
                            {
                                System.Windows.Forms.MessageBox.Show("현재 최신버전 입니다.", "버전확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show($"최신버전: {Current_Version["modpack_ver"].ToString()} \n현재버전: {Old_Version["modpack_ver"].ToString()}", "버전확인", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                MessageBoxResult result = System.Windows.MessageBox.Show("최신버전으로 다운로드 하시겠습니까?", "버전확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (result == MessageBoxResult.Yes)
                                {
                                    string minecraftVer = Current_Version["minecraft_ver"].ToString();
                                    string modpackVer = Current_Version["modpack_ver"].ToString();

                                    string downloadUrl = $"https://archive.org/download/Jjoon-{minecraftVer}-{modpackVer}/mods-{minecraftVer}.zip";

                                    // 다운로드 경로 지정
                                    string downloadFilePath = Path.Combine(appDataPath, "Jjoon Minecraft Insteller", "mods.zip");

                                    // 백그라운드에서 다운로드 수행
                                    var window1 = new Window1(downloadUrl, downloadFilePath, selectedPath);
                                    window1.ShowDialog();
                                }
                            }
                        }
                    }
                    string oldfile = $"{appDataPath}\\Jjoon Minecraft Insteller\\Old_Version.json";
                    string newfile = $"{appDataPath}\\Jjoon Minecraft Insteller\\Current_Version.json";
                    File.Delete(oldfile);
                    File.Move(newfile, oldfile, true);
                }
                catch (WebException ex)
                {
                    // WebException 처리
                    if (ex.Response is HttpWebResponse response)
                    {
                        System.Windows.Forms.MessageBox.Show($"다운로드 중 오류가 발생했습니다. 응답 코드: {response.StatusCode}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show($"다운로드 중 오류가 발생했습니다: {ex.Message}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // 내부 예외 확인
                    if (ex.InnerException != null)
                    {
                        System.Windows.Forms.MessageBox.Show($"내부 오류: {ex.InnerException.Message}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"다운로드 중 오류가 발생했습니다: {ex.Message}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task DownloadFileInBackground(string url, string downloadPath)
        {
            using (WebClient webClient = new WebClient())
            {
                await webClient.DownloadFileTaskAsync(new Uri(url), downloadPath);
            }
        }
    }
}
