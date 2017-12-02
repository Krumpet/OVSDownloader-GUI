using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using HtmlAgilityPack;
using mshtml;
using Microsoft.WindowsAPICodePack.Dialogs;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Forms.ListBox;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using Timer = System.Threading.Timer;

//using System.Windows.Forms;


namespace OVSDownloader_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly string user;
        //private string pass;
        //private string server = "campus.technion.ac.il";
        private bool _firstSason = true;
        private string _downloadDir = Directory.GetCurrentDirectory();
        private StringBuilder _sb = new StringBuilder();
        private Process msdl;
        private bool _stopAll = false;
        private List<IHTMLElement> links = new List<IHTMLElement>();
        private String curFile;

        public MainWindow()
        {
            InitializeComponent();
            //EnterCred.IsEnabled = false;
            FolderPath.IsEnabled = false;
            FolderPath.Text = _downloadDir;
        }

        //private void Enter_Cred_Click(object sender, RoutedEventArgs e)
        //{
        //    HTMLDocument doc = (HTMLDocument) WebBrowser1.Document;
        //    doc.getElementById("username")?.setAttribute("value", user + "@" + server);
        //    doc.getElementById("LogiN").setAttribute("value", user);
        //    doc.getElementById("password").setAttribute("value", pass);
        //    doc.getElementById("PasswD").setAttribute("value", pass);
        //    doc.getElementById("ServeR").setAttribute("value", server);
        //    doc.getElementById("idenT_conT").click();
        //    //if (WebBrowser1.CanGoForward) WebBrowser1.GoForward();
        //}

        private void WebBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            string address = ((HTMLDocument) WebBrowser1.Document).URLUnencoded;
            //string address2 = e.Uri.AbsoluteUri;
            if (address.StartsWith(@"https://sason-p.technion.ac.il/") && (_firstSason))
            {
                _firstSason = false;
                //EnterCred.IsEnabled = true;
            }
            //MessageBox.Show("changed to " + address);
            //Regex loginRegex = new Regex(@"^https://video.technion.ac.il/Courses/");
            //Regex courseRegex = new Regex(@"\.html$");
            //if (loginRegex.IsMatch(address))
            //{
            //    if (courseRegex.IsMatch(address))
            //    {
            //        MessageBox.Show("matches!");
            //    }
            //    MessageBox.Show("disabled login");
            //    EnterCred.IsEnabled = false;
            //}
            //MessageBox.Show("changed to " + address2);
        }

        private bool IsCourseAddress(string address)
        {
            Regex loginRegex = new Regex(@"^https://video.technion.ac.il/Courses/");
            Regex courseRegex = new Regex(@"\.html$");
            return (loginRegex.IsMatch(address) && courseRegex.IsMatch(address));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var curDir = Directory.GetCurrentDirectory();
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select Download Location",
                IsFolderPicker = true,
                InitialDirectory = curDir,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = curDir,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() != CommonFileDialogResult.Ok) return;
            _downloadDir = dlg.FileName;
            FolderPath.Text = _downloadDir;
        }

        private void Download(IHTMLElement link)
        {
            link.click();
            AutoClosingMessageBox.Show("Parsing...", "Strange but true", 400);
            
            //var downloadElement = ((HTMLDocument) WebBrowser1.Document).getElementsByTagName("A").Cast<IHTMLElement>()
            //    .Where(x => x.innerHTML != null && x.innerHTML.Contains("rtsp://video9.cc.technion.ac.il")).ToList();
            //MessageBox.Show("got " + downloadElement.Count);
            //var frames = ((HTMLDocument) WebBrowser1.Document).frames.item(0);
            //Thread.Sleep(1000);
            var outer = ((HTMLDocument) WebBrowser1.Document).getElementById("fancybox-outer");
            //MessageBox.Show(outer.innerText);

            // Also get X button
            //var Xbutton = ((HTMLDocument) outer.document).getElementById("fancybox-close");

            var content = ((HTMLDocument) outer.document).getElementById("fancybox-content");
            //MessageBox.Show(content.innerText);

            var frames = ((HTMLDocument) content.document).frames;

            //MessageBox.Show("Frames Length: " + frames.length);

            //MessageBox.Show(frames.outerHTML);
            //MessageBox.Show(innerframe.outerHTML);
            //MessageBox.Show(innerframe2.length.ToString());
            AutoClosingMessageBox.Show("Parsing...", "Strange but true", 400);

            //for (int i = 0; i < 1000000; i++)
            //{
            //    i++;
            //}

            //int i = 0;

            object refObj = 0;
            //while (frames.length == 0)
            //{
            //    frames = ((HTMLDocument)content.document).frames;
            //}
            //MessageBox.Show(frames.length.ToString());
            IHTMLWindow2 currentFrame = (IHTMLWindow2) frames.item(ref refObj);
            //while (currentFrame == null)
            //{
            //    currentFrame = (IHTMLWindow2)frames.item(ref refObj);
            //}
            String toParse = currentFrame.document.body.innerHTML;
            //MessageBox.Show(ToParse);
            TitleInfo.Text = toParse;
            String localfilename = link.getAttribute("href").ToString();
            localfilename = localfilename.Split('/')[7];
            var fullfilename = Path.Combine(_downloadDir, localfilename);
            //MessageBox.Show(fullfilename);
            curFile = fullfilename;
            Regex filenameRegex = new Regex("href=\"rtsp://video9.cc.technion.ac.il:554/Courses\\S+\"");
            var filename = filenameRegex.Match(toParse).Value.TrimStart("href=".ToCharArray()).TrimStart('"').TrimEnd('"');
            // TODO: save parsed filename in variable and use it
            // Close the iframe element to prepare for next download
            //Xbutton?.click();
            //MessageBox.Show(filename);
            //MessageBox.Show(localfilename);
            msdl = new Process
            {
                StartInfo =
                {
                    FileName = "msdl.exe",
                    Arguments = "-s2 " + filename + " -o " + fullfilename,
                    //WindowStyle = ProcessWindowStyle.Maximized,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            //// hookup the eventhandlers to capture the data that is received
            //msdl.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            //msdl.ErrorDataReceived += (sender, args) => sb.AppendLine(args.Data);

            //redirect the output

            // redirect title to one textbox and the rest overrides itself in another
            msdl.OutputDataReceived += SortOutputHandler;
            msdl.ErrorDataReceived += SortOutputHandler;

            // TODO: disable all buttons
            foreach (var element in Controls.Children)
            {
                if (element is Button button)
                {
                    //MessageBox.Show(button.Name);
                    button.IsEnabled = false;
                }
            }
            //{
            //    if (element is Button button) {
            //        button.IsEnabled = false;
            //    }
            //}

            // Wipe text boxes
            DownloadInfo.Text = TitleInfo.Text = "";
            msdl.Start();
            // start our event pumps
            msdl.BeginOutputReadLine();
            msdl.BeginErrorReadLine();

            while (!msdl.HasExited)
            {
                DoEvents(); // This keeps your form responsive by processing events
            }

            // Clear text
            DownloadInfo.Text = TitleInfo.Text = "";

            // TODO: enable all buttons
            foreach (var element in Controls.Children)
            {
                if (element is Button button)
                {
                    button.IsEnabled = true;
                }
            }
            // TODO: click the x button to close the popup
            //// until we are done
            //msdl.WaitForExit();

            //// do whatever you need with the content of sb.ToString();

            //TitleInfo.Text = sb.ToString();

            //for (int i = 0; i < innerframe2.length; i++)
            //{
            //    object ref_index = i;
            //    IHTMLWindow2 currentFrame = (IHTMLWindow2)innerframe2.item(ref ref_index);
            //    if (currentFrame != null)
            //    {
            //        string ToParse = currentFrame.document.body.innerHTML;
            //        MessageBox.Show(ref_index + " " + ToParse);
            //    }
            //    //System.Windows.Forms.HtmlWindow frame = ((HTMLDocument)WebBrowser1.Document).getElementById("fancybox-").Document.Window.Frames["decrpt_ifr"];
            //    //HtmlElement body = frame.Document.GetElementById("tinymce");



            //    //if (WebBrowser1.CanGoBack) WebBrowser1.GoBack();
            //}
        }

        private void SortOutputHandler(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                //sb.Append(e.Data);
                if (e.Data?.StartsWith("DL") == true) // Receive progress report
                {
                    DownloadInfo.Text = e.Data;
                }
                else // Receive file info
                {
                    TitleInfo.AppendText(e.Data);
                    TitleInfo.ScrollToEnd();
                }
            });
            //if (richBox.InvokeRequired) { richBox.BeginInvoke(new DataReceivedEventHandler(SortOutputHandler), new[] { sendingProcess, outLine }); }
            //else
            //{
            //sb.Append(Environment.NewLine + "[" + numOutputLines.ToString() + "] - " + outLine.Data);

            //}
        }

        private List<IHTMLElement> GetLinks()
        {
            string mode = Format.Text;
            //List<string> results = new List<string>();
            IHTMLElementCollection alllinks = ((HTMLDocument) WebBrowser1.Document).getElementsByTagName("A");
            var list = alllinks.Cast<IHTMLElement>().Where(x => x.innerText != null && x.innerText.Contains(mode)).ToList();
            return list;
            
                //if (link.innerText != null && link.innerText.Contains("rtsp"))
                //{
                
                //MessageBox.Show("success with " + href);
                //link.click();
                //WebBrowser1.GoBack();
                //Thread.Sleep(3000);
                //Download(link);
                
                //MessageBox.Show(href);
                //if (link.getAttribute("href"))
                //link.click();
                //}
            
            //using (var client = new WebClient())
            //{
            //    HTMLDocument doc = (HTMLDocument)WebBrowser1.Document;
            //    var htmlText = doc.documentElement.outerHTML;
            //    MessageBox.Show(htmlText);
            //    HtmlDocument doc2 = new HtmlDocument();
            //    doc2.LoadHtml(htmlText);
            //    //MessageBox.Show();

            //    foreach (HtmlNode link in doc2.DocumentNode.SelectNodes("//a[@href]"))
            //    {
            //        HtmlAttribute href = link.Attributes["href"];
            //        if (href != null)
            //        {
            //            results.Add(href.Value);
            //        }
            //    }
            //}
            
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if ((IsCourseAddress(((HTMLDocument) WebBrowser1.Document).URLUnencoded)))
            {
                //WebBrowser1.LoadCompleted += LoadCompleted;
                //WebBrowser1.Refresh();

                //while (!WebBrowser1.IsLoaded) { }

                int start = int.Parse(StartVid.Text),
                    end = int.Parse(EndVid.Text),
                    amount = end - start + 1;

                links = GetLinks();

                // check file numbers make sense
                StringBuilder errorMessage = new StringBuilder("The following error(s) were found:").AppendLine();
                int errno = 0;

                if (start < 1 || start > end || start > links.Count)
                {
                    errno++;
                    errorMessage.AppendFormat("{0}) Invalid start value \"{1}\".", errno, start).AppendLine();
                }

                if (end > links.Count)
                {
                    errno++;
                    errorMessage.AppendFormat("{0}) Invalid end value \"{1}\".", errno, end).AppendLine();
                }

                if (errno > 0)
                {
                    errorMessage.AppendFormat("There are {0} files.", links.Count).AppendLine();
                    MessageBox.Show(errorMessage.ToString());
                    return;
                }

                var filteredList = links.GetRange(start-1, amount);

                foreach (IHTMLElement element in filteredList)
                {
                    if (_stopAll)
                    {
                        _stopAll = false;
                        return;
                    }
                    // call Download for each link
                    Download(element);
                    
                    //MessageBox.Show(result);
                }
                
                //Download(filteredList[0]);
            }
            else
            {
                MessageBox.Show("Not a valid course address.");
            }
        }

        //private HttpClient client = new HttpClient();

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    //Parsing(address.Text);
        //    loading(address.Text);
        //}

        //private async void Parsing(string website)
        //{
        //    try
        //    {
        //        HttpClient http = new HttpClient();
        //        var response = await http.GetByteArrayAsync(website);
        //        String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
        //        source = WebUtility.HtmlDecode(source);
        //        HtmlDocument resultat = new HtmlDocument();
        //        resultat.LoadHtml(source);
        //        MessageBox.Show(resultat.ParsedText);
        //        HtmlNodeCollection user = resultat.DocumentNode.SelectNodes("//*[@id=\"username\"]");
        //        HtmlNodeCollection pass = resultat.DocumentNode.SelectNodes("//*[@id=\"password\"]");
        //        MessageBox.Show(user.Count.ToString());
        //        MessageBox.Show(pass.Count.ToString());
        //        foreach (HtmlNode node in user)
        //        {
        //            node.Attributes["value"].Value = "@";
        //        }
        //        foreach(HtmlNode node in pass)
        //        {
        //            node.Attributes["value"].Value = "";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("Network error");
        //    }
        //}

        //private async void loading(string uri)
        //{
        //    using (var response = await client.GetAsync(uri))
        //    {
        //        using (var content = response.Content)
        //        {
        //            // read answer in non-blocking way
        //            var result = await content.ReadAsStringAsync();
        //            var document = new HtmlDocument();
        //            document.LoadHtml(result);
        //            var user = document.DocumentNode.SelectNodes("//*[@id=\"username\"]");
        //            var pass = document.DocumentNode.SelectNodes("//*[@id=\"password\"]");
        //            MessageBox.Show(user.Count.ToString());
        //            MessageBox.Show(pass.Count.ToString());
        //            //Some work with page....
        //            foreach (HtmlNode node in user)
        //            {
        //                node.Attributes["value"].Value = "@";
        //            }
        //            foreach (HtmlNode node in pass)
        //            {
        //                node.Attributes["value"].Value = "";
        //            }

        //            HtmlElement button = document
        //        }
        //    }
        //}

        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //}

        public static void DoEvents()
        {
            Application.Current?.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(delegate { }));
        }

        // TODO: delete current file MSDL is working on
        private void StopCurrent_OnClick(object sender, RoutedEventArgs e)
        {
            if (!msdl?.HasExited == true) msdl.Kill();
        }

        private void StopAll_OnClick(object sender, RoutedEventArgs e)
        {
            _stopAll = true;
            if (!msdl?.HasExited == true) msdl.Kill();
        }
    }

    public class AutoClosingMessageBox
    {
        Timer _timeoutTimer;
        string _caption;

        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                MessageBox.Show(text, caption);
        }

        public static void Show(string text, string caption, int timeout)
        {
            var autoClosingMessageBox = new AutoClosingMessageBox(text, caption, timeout);
        }

        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }

        const int WM_CLOSE = 0x0010;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}