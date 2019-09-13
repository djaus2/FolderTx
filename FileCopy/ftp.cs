using System;
using System.Net;
using System.Threading;

using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;


//https://docs.microsoft.com/en-us/dotnet/api/system.net.ftpwebrequest?redirectedfrom=MSDN&view=netframework-4.8
namespace FileCopy
{
    public class FtpState
    {
        private ManualResetEvent wait;
        private FtpWebRequest request;
        private string fileName;
        private Exception operationException = null;
        string status;

        public FtpState()
        {
            wait = new ManualResetEvent(false);
        }

        public ManualResetEvent OperationComplete
        {
            get { return wait; }
        }

        public FtpWebRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        public Exception OperationException
        {
            get { return operationException; }
            set { operationException = value; }
        }
        public string StatusDescription
        {
            get { return status; }
            set { status = value; }
        }
    }
    public class AsynchronousFtpUpLoader
    {
        // Command line arguments are two strings:
        // 1. The url that is the name of the file being uploaded to the server.
        // 2. The name of the file on the local machine.
        //
        public static void FTPMain(string targetFtpServer, string fileName)
        {
            // Create a Uri instance with the specified URI string.
            // If the URI is not correctly formed, the Uri constructor
            // will throw an exception.
            ManualResetEvent waitObject;

            Uri target = new Uri(targetFtpServer);
            //string fileName = args[1];
            FtpState state = new FtpState();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example uses anonymous logon.
            // The request is anonymous by default; the credential does not have to be specified. 
            // The example specifies the credential only to
            // control how actions are logged on the server.

            request.Credentials = new NetworkCredential("Administrator", "a");

            // Store the request in the object that we pass into the
            // asynchronous operations.
            state.Request = request;
            state.FileName = fileName;

            // Get the event to wait on.
            waitObject = state.OperationComplete;

            // Asynchronously get the stream for the file contents.
            request.BeginGetRequestStream(
                new AsyncCallback(EndGetStreamCallback),
                state
            );

            // Block the current thread until all operations are complete.
            waitObject.WaitOne();

            // The operations either completed or threw an exception.
            if (state.OperationException != null)
            {
                throw state.OperationException;
            }
            else
            {
                Console.WriteLine("The operation completed - {0}", state.StatusDescription);
            }
        }
        private async static void EndGetStreamCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;

            Stream requestStream = null;
            // End the asynchronous call to get the request stream.
            try
            {
                requestStream = state.Request.EndGetRequestStream(ar);
                // Copy the file contents to the request stream.
                const int bufferLength = 2048;
                //byte[] buffer = new byte[bufferLength];
                //int count = 0;
                //int readBytes = 0;
                //FileStream stream = File.OpenRead(state.FileName);


                //state.Filename

                //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                //StorageFile sampleFile = await storageFolder.GetFileAsync("");

                //byte[] result;
                //using (Stream stream2 = await sampleFile.OpenStreamForReadAsync())
                //{
                //    using (var memoryStream = new MemoryStream())
                //    {

                //        stream2.CopyTo(memoryStream);
                //        result = memoryStream.ToArray();
                //    }

                //                //}
                //                using System.Runtime.InteropServices.WindowsRuntime;
                //                using Windows.Storage;
                //                ...
                IStorageFile file=null;
                IBuffer ibuffer = await FileIO.ReadBufferAsync(file);
                byte[] buffer2 = ibuffer.ToArray();
                int totalNumBytesToWrite = buffer2.Length;
                int ptr = 0;
                do
                {
                    //readBytes = stream.Read(buffer, 0, bufferLength);
                    //requestStream.Write(buffer, 0, readBytes);
                    //count += readBytes;

                    int numBytesToWrite = bufferLength;
                    if ((ptr + numBytesToWrite) > totalNumBytesToWrite)
                        numBytesToWrite = buffer2.Length - ptr;
                    requestStream.Write(buffer2, ptr, numBytesToWrite);
                    ptr += numBytesToWrite;
                }
                while (ptr< totalNumBytesToWrite);
                Console.WriteLine("Writing {0} bytes to the stream.", bufferLength);
                // IMPORTANT: Close the request stream before sending the request.
                requestStream.Close();
                // Asynchronously get the response to the upload request.
                state.Request.BeginGetResponse(
                    new AsyncCallback(EndGetResponseCallback),
                    state
                );
            }
            // Return exceptions to the main application thread.
            catch (Exception e)
            {
                Console.WriteLine("Could not get the request stream.");
                state.OperationException = e;
                state.OperationComplete.Set();
                return;
            }

        }

        // The EndGetResponseCallback method  
        // completes a call to BeginGetResponse.
        private static void EndGetResponseCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                response.Close();
                state.StatusDescription = response.StatusDescription;
                // Signal the main application thread that 
                // the operation is complete.
                state.OperationComplete.Set();
            }
            // Return exceptions to the main application thread.
            catch (Exception e)
            {
                Console.WriteLine("Error getting response.");
                state.OperationException = e;
                state.OperationComplete.Set();
            }
        }

        public static bool DeleteFileOnServer(Uri serverUri)
        {
            // The serverUri parameter should use the ftp:// scheme.
            // It contains the name of the server file that is to be deleted.
            // Example: ftp://contoso.com/someFile.txt.
            // 

            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Delete File Status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }

        public static bool CreateDirOnServer(Uri serverUri)
        {
            // The serverUri parameter should use the ftp:// scheme.
            // It contains the name of the server file that is to be deleted.
            // Example: ftp://contoso.com/dir.
            // 
            
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;//.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Create Directory status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }

        public static bool RemoveDirOnServer(Uri serverUri)
        {
            // The serverUri parameter should use the ftp:// scheme.
            // It contains the name of the server file that is to be deleted.
            // Example: ftp://contoso.com/dir.
            // 

            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;//.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Delete Directory Status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }
    }
}
