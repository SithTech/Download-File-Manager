using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;



namespace Download_File_Manager
{
    struct Data
    {
        public string[] dwnld_files { get; set; }
        public List<string> extensions { get; set; }
        public List<string> move_files { get; set; }
        public string save_path { get; set; }
        public string backup_path { get; set; }
        public string dwnld_path { get; set; }
        public string user_profile { get; set; }
        public bool guiMode { get; set; }
    }

    class Program
    {
        static Data d;
        static void Main(string[] args)
        {
            Console.WriteLine("Download File Manager is currently running...");

            string save_name = "Default Name";   
            d.guiMode = false;  //Deafults to no gui
            d.extensions = new List<string>();
            d.move_files = new List<string>();


            //User info
            d.user_profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            Directory.CreateDirectory(d.user_profile + "\\Documents\\Download File Manager");
            string[] options = File.ReadAllLines(d.user_profile + "\\Documents\\Download File Manager\\options.txt");

            foreach(string str in options)
            {
                string[] opt = str.Split(':');
                switch (opt[0])
                {
                    case "save_name":
                        {
                            save_name = opt[1];
                            break;
                        }
                    case "ext":
                        {
                            d.extensions.Add(opt[1]);
                            break;
                        }
                    case "gui":
                        {
                            if (opt[1].ToLower() == "true") d.guiMode = true;
                            else d.guiMode = false;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

        
            //Paths
            d.save_path = d.user_profile + "\\Desktop\\" + save_name;     //Save location of the files
            d.backup_path = d.user_profile + "\\Documents\\Download File Manager\\" + save_name + "_BackUp";
            d.dwnld_path = d.user_profile + "\\Downloads";

            //Events
            FileSystemWatcher dwnld_trig = new FileSystemWatcher(d.dwnld_path);
            dwnld_trig.EnableRaisingEvents = true;
            dwnld_trig.Created += Dwnld_trig_Created;
            dwnld_trig.Changed += Dwnld_trig_Changed;
            
            //Setup
            d.dwnld_files = Directory.GetFiles(d.dwnld_path);
            Directory.CreateDirectory(d.save_path);   //Create save folder if it doesn't already exist
            Directory.CreateDirectory(d.backup_path);     //Create backup folder if it doesn't already exist

            //Used when running in the background
            Console.Read();
            
        }//END_MAIN

        

        static void manageFiles()
        {
            
            d.dwnld_files = Directory.GetFiles(d.dwnld_path);
            Directory.CreateDirectory(d.save_path);
            Directory.CreateDirectory(d.backup_path);

            //Sort based on extensions
            foreach (string file in d.dwnld_files)
            {
                foreach (string ext in d.extensions)
                {
                    if (file.EndsWith(ext))
                    {
                        try
                        {
                            if (!File.Exists(d.backup_path + "\\" + Path.GetFileName(file)))
                            {
                                File.Copy(file, d.backup_path + "\\" + Path.GetFileName(file));
                            }

                            if (!File.Exists(d.save_path + "\\" + Path.GetFileName(file)))
                            {
                                File.Move(file, d.save_path + "\\" + Path.GetFileName(file));
                            }
                            else
                            {
                                File.Delete(file);
                            }

                        }
                        catch (IOException err)
                        {
                            //Console.WriteLine(err.GetBaseException());                            
                        }

                    }
                }
            }
        }

        private static void Dwnld_trig_Created(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine("New File Created...");
            manageFiles();
        }

        private static void Dwnld_trig_Changed(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine("File Changed...");          
            int attempts = 5;
            int timeout = 2000;     
            
            for (int i = 0; i < attempts; i++)
            {
                //Console.WriteLine(i+1);
                manageFiles();
                Thread.Sleep(timeout);
            }

        }

    }
}
