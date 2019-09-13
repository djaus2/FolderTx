using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileCopy
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        //private async Task<StorageFolder> ChooseFolder()
        //{
        //    FolderPicker folderPicker = new FolderPicker();
        //    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
        //    folderPicker.FileTypeFilter.Add("*");
        //    folderPicker.FileTypeFilter.Add(".docx");
        //    folderPicker.FileTypeFilter.Add(".xlsx");
        //    folderPicker.FileTypeFilter.Add(".pptx");
        //    StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        //    if (folder != null)
        //    {
        //        // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
        //        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
        //        //OutputTextBlock.Text = "Picked folder: " + folder.Name;
        //    }
        //    else
        //    {
        //        //OutputTextBlock.Text = "Operation cancelled.";
        //    }
        //    return folder;
        //}

        //public static async Task CopyAsync( StorageFolder source,StorageFolder destination)
        //{
        //    // If the destination exists, delete it.
        //    var targetFolder = await destination.TryGetItemAsync(source.DisplayName);

        //    if (targetFolder is StorageFolder)
        //        await targetFolder.DeleteAsync();

        //    targetFolder = await destination.CreateFolderAsync(source.DisplayName);

        //    // Get all files (shallow) from source
        //    var queryOptions = new QueryOptions
        //    {
        //        IndexerOption = IndexerOption.DoNotUseIndexer,  // Avoid problems cause by out of sync indexer
        //        FolderDepth = FolderDepth.Shallow,
        //    };
        //    var queryFiles = source.CreateFileQueryWithOptions(queryOptions);
        //    var files = await queryFiles.GetFilesAsync();

        //    // Copy files into target folder
        //    foreach (var storageFile in files)
        //    {
        //        await storageFile.CopyAsync((StorageFolder)targetFolder, storageFile.Name, NameCollisionOption.ReplaceExisting);
        //    }

        //    // Get all folders (shallow) from source
        //    var queryFolders = source.CreateFolderQueryWithOptions(queryOptions);
        //    var folders = await queryFolders.GetFoldersAsync();

        //    // For each folder call CopyAsync with new destination as destination
        //    foreach (var storageFolder in folders)
        //    {
        //        await CopyAsync(storageFolder, (StorageFolder)targetFolder);
        //    }
        //}

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        void UpdateProgress(int filecount, int foldercount)
        {
            Task.Run(async () => {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    tbNumFiles.Text = filecount.ToString();
                    tbNumFolders.Text = foldercount.ToString();
                    if (filecount > pb.Maximum)
                    {                        
                        while (filecount > pb.Maximum)
                            filecount -= (int)pb.Maximum;
                    }
                    pb.Value = filecount;
                    if (foldercount > pb2.Maximum)
                    {                       
                        while (foldercount > pb2.Maximum)
                            foldercount -= (int)pb2.Maximum;
                    }
                    pb2.Value = foldercount;
                });
            });
        }

        void UpdateStatus(int filecount, int foldercount, string status)
        {
            Task.Run(async () => {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    tbNumFiles.Text = filecount.ToString();
                    tbNumFolders.Text = foldercount.ToString();
                    tbStatus.Text = status;
                });
            });
        }

        //StorageFolder src;
        //StorageFolder dest;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //src = await ChooseFolder();
            tbSrcFolder.Text =  await FolderCopy.GetSrc();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //dest = await ChooseFolder();
            tbTargetFolder.Text =    await FolderCopy.GetDest();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //await CopyAsync(src, dest);
            await FolderCopy.CopyFolder(UpdateProgress,UpdateStatus);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            FolderCopy.Cancel();
        }
    }
}
