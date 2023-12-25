// Window1.xaml.cs
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace modinsteller
{
    public partial class Window1 : Window
    {
        private readonly string _downloadUrl;
        private readonly string _downloadPath;
        private readonly string _selectedPath;

        public Window1(string downloadUrl, string downloadPath, string selectedPath)
        {
            InitializeComponent();
            _downloadUrl = downloadUrl;
            _downloadPath = downloadPath;
            _selectedPath = selectedPath;

            // 백그라운드에서 다운로드 및 압축 해제 수행
            DownloadAndExtractInBackground();
        }

        private async void DownloadAndExtractInBackground()
        {
            try
            {
                string tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                // WebClient 생성
                using (WebClient webClient = new WebClient())
                {
                    // DownloadProgressChanged 이벤트 등록
                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

                    // 파일 다운로드
                    await webClient.DownloadFileTaskAsync(new Uri(_downloadUrl), _downloadPath);
                }

                // 압축 해제
                await ExtractZipAsync(_downloadPath, _selectedPath);

                // 다운로드 및 압축 해제 완료 메시지
                MessageBox.Show("다운로드 및 압축 해제가 완료되었습니다.");
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
            }
            finally
            {
                // 창 닫기
                Close();
            }
        }

        private async Task ExtractZipAsync(string zipFilePath, string extractPath)
        {
            try
            {
                // Ensure the target extraction directory exists
                Directory.CreateDirectory(extractPath);

                // Extract the zip file
                await Task.Run(() => ZipFile.ExtractToDirectory(zipFilePath, extractPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"압축 해제 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        private void MoveFiles(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                string fileName = Path.GetFileName(file);
                string destinationPath = Path.Combine(destinationDirectory, fileName);

                // 파일을 이동하기 전에 기존에 동일한 이름의 파일이 있다면 삭제
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                File.Move(file, destinationPath);
            }
        }

        // 다운로드 진행 상태를 업데이트하는 이벤트 핸들러
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // 프로세스바의 값을 다운로드 진행 상황에 맞게 업데이트
            ProgressBar.Value = e.ProgressPercentage;

            // 상태 레이블 업데이트
            StatusLabel.Content = $"다운로드 중... {e.ProgressPercentage}% 완료";
        }
    }
}
