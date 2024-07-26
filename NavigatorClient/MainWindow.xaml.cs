////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App //
// ver 2.0                                                                //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017        //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * Maintenance History:
 * --------------------
 * ver 2.1 : 26 Oct 2017
 * - relatively minor modifications to the Comm channel used to send messages
 *   between NavigatorClient and NavigatorServer
 * ver 2.0 : 24 Oct 2017
 * - added remote processing - Up functionality not yet implemented
 *   - defined NavigatorServer
 *   - added the CsCommMessagePassing prototype
 * ver 1.0 : 22 Oct 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MessagePassingComm;

namespace Navigator
{
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;

        public MainWindow()
        {
            InitializeComponent();
            initializeEnvironment();
            Console.Title = "Navigator Client";
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            getTopFiles();
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }
        //----< make Environment equivalent to ClientEnvironment >-------

        void initializeEnvironment()
        {
            Environment.root = ServerEnvironment.root;
            Environment.address = ServerEnvironment.address;
            Environment.port = ServerEnvironment.port;
            Environment.endPoint = ServerEnvironment.endPoint;
        }
        //----< define how to process each message command >-------------

        void initializeMessageDispatcher()
        {
            messageDispatcher["getTokens"] = (CommMessage msg) =>
            {
                foreach (string arg in msg.arguments)
                {
                    string contents = File.ReadAllText(arg);
                    CodePopUp popup = new CodePopUp();
                    popup.codeView.Text = contents;
                    popup.Show();
                }
            };

            messageDispatcher["getSemiExpression"] = (CommMessage msg) =>
            {
                foreach (string arg in msg.arguments)
                {
                    string contents = File.ReadAllText(arg);
                    CodePopUp popup = new CodePopUp();
                    popup.codeView.Text = contents;
                    popup.Show();
                }
            };

            // load type table in the file

            messageDispatcher["getTypeTable"] = (CommMessage msg) =>
            {
                foreach (string arg in msg.arguments)
                {
                    string contents = File.ReadAllText(arg);
                    CodePopUp popup = new CodePopUp();
                    popup.codeView.Text = contents;
                    popup.Show();
                    textBlock.Text = "Received results from " + msg.command;
                }
            };

            messageDispatcher["getDependencyTable"] = (CommMessage msg) =>
            {
                foreach (string arg in msg.arguments)
                {
                    string contents = File.ReadAllText(arg);
                    CodePopUp popup = new CodePopUp();
                    popup.codeView.Text = contents;
                    popup.Show();
                }
            };

            messageDispatcher["getGraph"] = (CommMessage msg) =>
            {
                foreach (string arg in msg.arguments)
                {
                    string contents = File.ReadAllText(arg);
                    CodePopUp popup = new CodePopUp();
                    popup.codeView.Text = contents;
                    popup.Show();
                }
            };

            messageDispatcher["getStrongComponents"] = (CommMessage msg) =>
            {
                foreach (string arg in msg.arguments)
                {
                    string contents = File.ReadAllText(arg);
                    CodePopUp popup = new CodePopUp();
                    popup.codeView.Text = contents;
                    popup.Show();
                }
            };
        }
        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*tokens_Click();
            semi_Click();
            typeTable_Click();
            dependencyTable_Click();
            graph_Click();
            strongComponents_Click();*/
        }

        //----< shut down comm when the main window closes >-------------

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            // The step below should not be nessary, but I've apparently caused a closing event to 
            // hang by manually renaming packages instead of getting Visual Studio to rename them.

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        //----< show files and dirs in root path >-----------------------

        public void getTopFiles()
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            localFiles.Items.Clear();
            foreach (string file in files)
            {
                CheckBox chk = new CheckBox();
                chk.Content = file.ToString();
                chk.Checked += new RoutedEventHandler(selectedFilesCheck);
                chk.Unchecked += new RoutedEventHandler(selectedFilesUnCheck);
                localFiles.Items.Add(chk);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();
            localDirs.Items.Clear();
            foreach (string dir in dirs)
            {
                localDirs.Items.Add(dir);
            }
        }
        //----< move to directory root and display files and subdirs >---

        private void localTop_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }

        public void tokens_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getTokens";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }

        private void semi_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getSemiExpression";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }
        private void typeTable_Click(object sender, RoutedEventArgs e)
        {   
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getTypeTable";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }

        private void dependencyTable_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getDependencyTable";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }

        private void graph_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getGraph";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }

        private void strongComponents_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getStrongComponents";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }

        public void tokens_Click()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getTokens";
            string dirPath = System.IO.Path.GetFullPath(ServerEnvironment.root + "TestFiles");
            string[] fileList = new string[] { };
            fileList = Directory.GetFiles(dirPath);
            foreach (string file in fileList)
            {
                msg1.arguments.Add(file);
            }
            comm.postMessage(msg1);
        }

        private void semi_Click()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getSemiExpression";
            string dirPath = System.IO.Path.GetFullPath(ServerEnvironment.root + "TestFiles");
            string[] fileList = new string[] { };
            fileList = Directory.GetFiles(dirPath);
            foreach (string file in fileList)
            {
                msg1.arguments.Add(file);
            }
            comm.postMessage(msg1);
        }
        private void typeTable_Click()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getTypeTable";
            string dirPath = System.IO.Path.GetFullPath(ServerEnvironment.root + "TestFiles");
            string[] fileList = new string[] { };
            fileList = Directory.GetFiles(dirPath);
            foreach (string file in fileList)
            {
                msg1.arguments.Add(file);
            }
            comm.postMessage(msg1);
        }

        private void dependencyTable_Click()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getDependencyTable";
            string dirPath = System.IO.Path.GetFullPath(ServerEnvironment.root + "TestFiles");
            string[] fileList = new string[] { };
            fileList = Directory.GetFiles(dirPath);
            foreach (string file in fileList)
            {
                msg1.arguments.Add(file);
            }
            comm.postMessage(msg1);
        }

        private void graph_Click()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getGraph";
            foreach (string file in selectedFiles.Items)
            {
                string filePath = ServerEnvironment.root + file;
                string fullPath = System.IO.Path.GetFullPath(filePath);
                msg1.arguments.Add(fullPath);
            }
            comm.postMessage(msg1);
        }

        private void strongComponents_Click()
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Sneha Giranje";
            msg1.command = "getStrongComponents";
            string dirPath = System.IO.Path.GetFullPath(ServerEnvironment.root + "TestFiles");
            string[] fileList = new string[] { };
            fileList = Directory.GetFiles(dirPath);
            foreach (string file in fileList)
            {
                msg1.arguments.Add(file);
            }
            comm.postMessage(msg1);
        }

        //----< show selected file in code popup window >----------------

        private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = localFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        //----< move to parent directory and show files and subdirs >----

        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        //----< move into subdir and show files and subdirs >------------

        private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string dirName = localDirs.SelectedValue as string;
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles();
        }
        //----< move to root of remote directories >---------------------
        /*
         * - sends a message to server to get files from root
         * - recv thread will create an Action<CommMessage> for the UI thread
         *   to invoke to load the remoteFiles listbox
         */
        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Jim Fawcett";
            msg1.command = "getTopFiles";
            msg1.arguments.Add("");
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopDirs";
            comm.postMessage(msg2);
        }
        //----< download file and display source in popup window >-------

        private void selectedFilesCheck(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox) sender;
            selectedFiles.Items.Add(cb.Content);
        }

        private void selectedFilesUnCheck(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            selectedFiles.Items.Remove(cb.Content);
        }
    }
}
