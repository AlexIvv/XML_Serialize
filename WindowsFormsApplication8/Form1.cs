using System;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Threading;


namespace WindowsFormsApplication8
{
    public partial class Form1 : Form
    {
        string path;
        string link;
        List<string> vs = new List<string>();
        Directories Directories = new Directories();
        FileSystemWatcher watcher;
        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        FileSystemWatcher[] arrayWathcer;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var OpenedFile = new OpenFileDialog())
            {
                if (OpenedFile.ShowDialog() == DialogResult.OK)
                {
                    path = OpenedFile.FileName;
                    Loads(Directories, path);
                    StartWatcher();
                }
                else
                    Close();
            }
        }

        /// <summary>
        /// Выполняет проверку валидности XML, существование указанных путей, для корректной работы FSW.
        /// </summary>
        /// <param name="Directories"></param>
        /// <param name="path"></param>
        private void Loads(Directories Directories, string path)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Directories));

            try
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    Directories = (Directories)deserializer.Deserialize(textReader);
                    for (int j = 0; j < Directories.DirectLinks.Length; j++)
                    {
                        if (Directory.Exists(Directories.DirectLinks[j]))
                        {
                            link = Directories.DirectLinks[j];
                            vs.Add(link);
                            watcher = new FileSystemWatcher(link);
                            watchers.Add(watcher);
                            listBox1.Items.Add(link);
                        }
                        else
                        {
                            if (MessageBox.Show("Указанный путь не существует. Пропустить?", "Ошибка!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                continue;
                            else
                            {
                                //link = Directories.DirectLinks[j];
                                //listBox1.Items.Add(link);
                                Close();
                            }

                        }
                    }
                }
            }
            catch
            {
                if (MessageBox.Show("Файл не найден или имеет недопустимую структуру. Создать новый файл?", "Сообщение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Exit(Directories, path);
                else
                    Close();
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            using (var openFile = new FolderBrowserDialog())
            {
                openFile.ShowNewFolderButton = false;
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    link = openFile.SelectedPath;
                    vs.Add(link);
                    watcher = new FileSystemWatcher(link);
                    watchers.Add(watcher);
                    listBox1.Items.Add(link);
                }
            }
            StartWatcher();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null && MessageBox.Show("Вы уверены что хотите удалить выбранный элемент из списка?", "Сообщение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                vs.RemoveAt(listBox1.SelectedIndex);
                watchers.RemoveAt(listBox1.SelectedIndex);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }
        
        private void Exit(Directories Directories, string path)
        {
            Directories.DirectLinks = new string[vs.Count];            
            XmlSerializer formatter = new XmlSerializer(typeof(Directories));

            using (TextWriter textWriter = new StreamWriter(path))
            {
                for (int i = 0; i < vs.Count; i++)
                    Directories.DirectLinks[i] = vs[i];
                formatter.Serialize(textWriter, Directories);// сериализация (сохранение объекта в поток)
            }
        }
        
        private void StartWatcher()
        {
            arrayWathcer = new FileSystemWatcher[vs.Count];
            watchers.CopyTo(arrayWathcer);
            for (int i = 0; i < vs.Count; i++)
            {
                arrayWathcer[i].Renamed += new RenamedEventHandler(fileSystemWatcher1_Renamed_1);
                arrayWathcer[i].Changed += new FileSystemEventHandler(fileSystemWatcher1_Changed);
                arrayWathcer[i].Created += new FileSystemEventHandler(fileSystemWatcher1_Created_1);
                arrayWathcer[i].Deleted += new FileSystemEventHandler(fileSystemWatcher1_Deleted_1);

                arrayWathcer[i].NotifyFilter =  NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
                arrayWathcer[i].EnableRaisingEvents = true;
                arrayWathcer[i].IncludeSubdirectories = false;
            }
        }

        private void SomeFunc(object sender, EventArgs e)
        {
            var myThread = new Thread(myThreade);
            myThread.Start();
        }

        private void myThreade()
        {
            Thread.Sleep(100);
            Action action = () => listBox1.Items.Add(link);
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        private void fileSystemWatcher1_Created_1(object sender, FileSystemEventArgs e)
        {
            //MessageBox.Show(string.Format("Created: {0} {1}", e.FullPath, e.ChangeType));
            link = e.FullPath;
            vs.Add(link);
            watcher = new FileSystemWatcher(link);
            watchers.Add(watcher);
            SomeFunc(sender, e);
            StartWatcher();
        }
        private void fileSystemWatcher1_Deleted_1(object sender, FileSystemEventArgs e)
        {
            MessageBox.Show(string.Format("Deleted: {0} {1}", e.FullPath, e.ChangeType));
        }
        private void fileSystemWatcher1_Renamed_1(object sender, RenamedEventArgs e)
        {
            MessageBox.Show(string.Format("Renamed: {0} {1}", e.FullPath, e.ChangeType));
        }
        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            MessageBox.Show(string.Format("Changed: {0} {1}", e.FullPath, e.ChangeType));
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Exit(Directories, path);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    [XmlRoot(ElementName = "Directories")]
    public class Directories
    {
        [XmlArray(ElementName = "DirectLinks")]
        [XmlArrayItem(ElementName = "link")]
        public string[] DirectLinks { get; set; }

        public Directories() { }

        public Directories(string[] fileDir) { DirectLinks = fileDir; }
    }
}
