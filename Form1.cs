﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DigglesModManager
{


    public partial class DigglesModManager : Form
    {
        public static string exePath = @"."; //dyn: @"." | local: D:\Programme\Wiggles
        public static string modPath = exePath; //dyn: exePath | local: @"D:\Projekte\DigglesModManager"
        public static string modDirectoryName = "Mods";
        public static string activeModsFileName = "mods.dm";
        public static string restoreFileName = "restore.dm";
        public static string modSettingsFileName = "settings.dm";
        public static string modDescriptionFileName = "description.dm";

        public static string changeFileStarting = "change_";
        public static string copyFileEnding = "_copy";

        bool warning = false;

        List<Mod> inactiveMods = new List<Mod>();
        List<Mod> activeMods = new List<Mod>();

        public DigglesModManager()
        {
            InitializeComponent();

            if (!File.Exists(exePath + "\\" + "Wiggles.exe"))
            {
                MessageBox.Show("Legen Sie die Datei ins Wiggles Verzeichnis!");
            }
            else
            {
                setMessage("", Color.Black);
                readMods();
            }
        }

        private void setMessage(string text, Color color)
        {
            label_message.Text = text;
            label_message.ForeColor = color;
        }

        private void readMods()
        {
            inactiveMods.Clear();
            activeMods.Clear();

            //check if mod directory exists
            if (!Directory.Exists(modPath + "\\" + modDirectoryName))
            {
                Directory.CreateDirectory(modPath + "\\" + modDirectoryName);
            }

            //read last active mods
            List<string> lastActiveMods = new List<string>();
            if (File.Exists(exePath + "\\" + activeModsFileName))
            {
                StreamReader reader = new StreamReader(exePath + "\\" + activeModsFileName);
                string mod;
                while ((mod = reader.ReadLine()) != null)
                {
                    lastActiveMods.Add(mod);
                }
                reader.Close();
            }

            //add to active mods 
            foreach (string modAndSettings in lastActiveMods)
            {
                //read settings values
                int separatorIndex = modAndSettings.IndexOf('|');
                string mod = modAndSettings;
                string settings = null;
                if (separatorIndex > 0)
                {
                    settings = modAndSettings.Substring(separatorIndex + 1);
                    mod = modAndSettings.Substring(0, separatorIndex);
                }

                //add active mod
                if (Directory.Exists(modPath + "\\" + modDirectoryName + "\\" + mod))
                {
                    activeMods.Add(new Mod(mod, settings));
                }
            }

            //read mods
            DirectoryInfo[] modDirectories = (new DirectoryInfo(modPath + "\\" + modDirectoryName)).GetDirectories();
            foreach (DirectoryInfo modInfo in modDirectories)
            {
                if (!activeMods.Contains(new Mod(modInfo.Name, null)))
                {
                    inactiveMods.Add(new Mod(modInfo.Name, null));
                }
            }
            inactiveMods.Sort();

            changeDataSource();
        }

        private void changeDataSource()
        {
            // Change the DataSource.
            listBox1.DataSource = null;
            listBox1.DataSource = inactiveMods;
            listBox2.DataSource = null;
            listBox2.DataSource = activeMods;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            setMessage("", Color.Black);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            setMessage("", Color.Black);

            //find settings file
            int selectedIndex = listBox2.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < activeMods.Count) {
                Mod mod = activeMods.ElementAt(selectedIndex);

                DirectoryInfo modDir = new DirectoryInfo(modPath + "\\" + modDirectoryName + "\\" + mod.ModDirectoryName);
                FileInfo[] modFiles = modDir.GetFiles();

                bool hasSettings = false;
                foreach (FileInfo gameFile in modFiles)
                {
                    if (gameFile.Name.Equals(modSettingsFileName))
                    {
                        hasSettings = true;
                    }
                }
                //set settings button enabled
                button_mod_settings.Enabled = hasSettings;
            }
        }

        private void button_right_Click(object sender, EventArgs e)
        {
            setMessage("", Color.Black);
            int selectedIndex = listBox1.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < inactiveMods.Count)
            {
                activeMods.Insert(0, inactiveMods.ElementAt(selectedIndex)); //add element right at first position
                inactiveMods.RemoveAt(selectedIndex); //remove element left

                changeDataSource();
            }
        }

        private void button_left_Click(object sender, EventArgs e)
        {
            setMessage("", Color.Black);
            int selectedIndex = listBox2.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < activeMods.Count)
            {
                inactiveMods.Add(activeMods.ElementAt(selectedIndex)); //add element left
                activeMods.RemoveAt(selectedIndex); //remove element right
                inactiveMods.Sort();

                changeDataSource();
            }
        }

        private void button_up_Click(object sender, EventArgs e)
        {
            setMessage("", Color.Black);
            int selectedIndex = listBox2.SelectedIndex;
            if (selectedIndex > 0 && selectedIndex < activeMods.Count)
            {
                Mod mod = activeMods.ElementAt(selectedIndex); //get mod
                activeMods.RemoveAt(selectedIndex); //remove mod
                selectedIndex--;
                activeMods.Insert(selectedIndex, mod); //add at new position

                changeDataSource();
                listBox2.SetSelected(selectedIndex, true);
            }
        }

        private void button_down_Click(object sender, EventArgs e)
        {
            setMessage("", Color.Black);
            int selectedIndex = listBox2.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < activeMods.Count - 1)
            {
                Mod mod = activeMods.ElementAt(selectedIndex); //get mod
                activeMods.RemoveAt(selectedIndex); //remove mod
                selectedIndex++;
                activeMods.Insert(selectedIndex, mod); //add at new position

                changeDataSource();
                listBox2.SetSelected(selectedIndex, true);
            }
        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            setMessage("", Color.Black);
            readMods();
        }

        private void button_mod_Click(object sender, EventArgs e)
        {
            warning = false;
            setMessage("...", Color.Black);

            restore();
            foreach (Mod mod in activeMods)
            {
                DirectoryInfo modDir = new DirectoryInfo(modPath + "\\" + modDirectoryName + "\\" + mod.ModDirectoryName);
                letsMod(mod, modDir, new DirectoryInfo(exePath));
            }
            saveActiveMods();
            if (warning)
            {
                setMessage("Warning", Color.Orange);
            }
            else
            {
                setMessage("Success", Color.Green);
            }
        }

        private void letsMod(Mod mod, DirectoryInfo modDirectory, DirectoryInfo gameDirectory)
        {
            FileInfo[] modFiles = modDirectory.GetFiles();
            FileInfo[] gameFiles = gameDirectory.GetFiles();

            //if Texture directory deactivate texmaps.bin
            if (gameDirectory.Name == "Texture")
            {
                foreach (FileInfo gameFile in gameFiles)
                {
                    if (gameFile.Name == "texmaps.bin")
                    {
                        rememberForRestore(gameFile, "res");
                        gameFile.MoveTo(gameFile.FullName + copyFileEnding);
                        break;
                    }
                }
            }

            //add or override game files
            foreach (FileInfo modFile in modFiles)
            {
                //detect mode
                string mode = "replace";
                string filename = modFile.Name;

                //skip modmanager files
                if (filename.Equals(modSettingsFileName) || filename.Equals(modDescriptionFileName))
                {
                    continue;
                }

                if (filename.StartsWith(changeFileStarting))
                {
                    mode = "change";
                    filename = filename.Substring(changeFileStarting.Count());
                }

                FileInfo rightGameFile = null;
                bool copyExists = false;
                foreach (FileInfo gameFile in gameFiles)
                {
                    //check for game file
                    if (gameFile.Name == filename)
                    {
                        rightGameFile = gameFile;
                    }
                    //check for copy
                    if (gameFile.Name == filename + copyFileEnding)
                    {
                        copyExists = true;
                    }
                }


                string type = "del"; //type for delelte at restore
                if (rightGameFile != null && !copyExists)
                {
                    //rename game file
                    rightGameFile.CopyTo(rightGameFile.FullName + copyFileEnding, true);
                    type = "res"; //type for restore
                }

                FileInfo newModFile = null;
                //switch mode
                if (mode == "replace") //replacement mode
                {
                    //copy file to game folder
                    newModFile = modFile.CopyTo(gameDirectory.FullName + "\\" + filename, true);
                }
                else if (mode == "change" && rightGameFile != null) //file change mode
                {
                    //change game file
                    type = "res";
                    newModFile = rightGameFile;

                    string origFileContent = File.ReadAllText(newModFile.FullName, Encoding.Default);

                    StreamReader reader = new StreamReader(modFile.FullName, Encoding.Default);
                    string line;
                    bool started = false;
                    Stack<bool> ifStack = new Stack<bool>();
                    int commandCount = -1;
                    string[] command = { "", "" };
                    string[] commandText = { "", "" };

                    int i = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        i++;
                        //check for if ending
                        if (line.StartsWith("$ifend") && ifStack.Count > 0)
                        {
                            ifStack.Pop();
                        }
                        //if clause
                        if (line.StartsWith("$if:"))
                        {
                            if (ifStack.Count > 0 && ifStack.Contains(false))
                            {
                                //skip check, because it is already false
                                ifStack.Push(false);
                            }
                            else
                            {
                                //check if statement
                                string ifStatement = line.Substring(4).TrimEnd();
                                bool not = false;
                                if (ifStatement.StartsWith("!"))
                                {
                                    not = true;
                                    ifStatement = ifStatement.Substring(1);
                                }

                                //check if-type
                                if (ifStatement.StartsWith("mod:"))
                                {
                                    //$if:mod:
                                    ifStatement = ifStatement.Substring(4);
                                    //search mod
                                    bool modFound = false;
                                    foreach (Mod m in activeMods)
                                    {
                                        if (m.ModDirectoryName.Equals(ifStatement))
                                        {
                                            modFound = true;
                                            break;
                                        }
                                    }
                                    ifStack.Push(modFound != not);
                                }
                                else
                                {
                                    //$if:varname
                                    //look for vaviable
                                    bool modVarFound = false;
                                    foreach(ModVar modVar in mod.Vars)
                                    {
                                        if (modVar.VarName.Equals(ifStatement))
                                        {
                                            modVarFound = true;
                                            //supports only bool variables
                                            if (modVar.Type.Equals("bool"))
                                            {
                                                ifStack.Push(((ModVar<bool>)modVar).Value != not);
                                            }
                                            else
                                            {
                                                MessageBox.Show("$if unterstuetzt nur boolsche Variablen: " + i + "\nDatei: " + modFile.FullName);
                                            }
                                            break;
                                        }
                                    }
                                    if (!modVarFound)
                                    {
                                        MessageBox.Show("$if boolsche Variable '" + ifStatement + "' nicht gefunden: " + i + "\nDatei: " + modFile.FullName);
                                    }
                                }
                            }
                        }
                        //skip when if clause was false
                        if (ifStack.Count > 0 && ifStack.Contains(false))
                        {
                            continue;
                        }

                        //start tag
                        if (line.StartsWith("$start"))
                        {
                            if (!started)
                            {
                                started = true;
                                commandCount = -1;
                            }
                            else
                            {
                                MessageBox.Show("$start an der falschen Stelle\nZeile: " + i + "\nDatei: " + modFile.FullName);
                                warning = true;
                                break;
                            }
                            //end tag
                        }
                        else if (line.StartsWith("$before"))
                        {
                            commandCount++;
                            if (started && commandCount < 2)
                            {
                                command[commandCount] = "before";
                                commandText[commandCount] = "";
                            }
                            else
                            {
                                MessageBox.Show("$before an der falschen Stelle\nZeile: " + i + "\nDatei: " + modFile.FullName);
                                warning = true;
                                break;
                            }
                        }
                        else if (line.StartsWith("$after"))
                        {
                            commandCount++;
                            if (started && commandCount < 2)
                            {
                                command[commandCount] = "after";
                                commandText[commandCount] = "";
                            }
                            else
                            {
                                MessageBox.Show("$after an der falschen Stelle\nZeile: " + i + "\nDatei: " + modFile.FullName);
                                warning = true;
                                break;
                            }
                        }
                        else if (line.StartsWith("$put"))
                        {
                            commandCount++;
                            if (started && commandCount < 2)
                            {
                                command[commandCount] = "put";
                                commandText[commandCount] = "";
                            }
                            else
                            {
                                MessageBox.Show("$put an der falschen Stelle\nZeile: " + i + "\nDatei: " + modFile.FullName);
                                warning = true;
                                break;
                            }
                        }
                        else if (line.StartsWith("$replace"))
                        {
                            commandCount++;
                            if (started && commandCount < 2)
                            {
                                command[commandCount] = "replace";
                                commandText[commandCount] = "";
                            }
                            else
                            {
                                MessageBox.Show("$replace an der falschen Stelle\nZeile: " + i + "\nDatei: " + modFile.FullName);
                                break;
                            }
                        }
                        else if (line.StartsWith("$with"))
                        {
                            commandCount++;
                            if (started && commandCount < 2)
                            {
                                command[commandCount] = "with";
                                commandText[commandCount] = "";
                            }
                            else
                            {
                                MessageBox.Show("$with an der falschen Stelle\nZeile: " + i + "\nDatei: " + modFile.FullName);
                                warning = true;
                                break;
                            }
                        }
                        else if (line.StartsWith("$end"))
                        {
                            if (started)
                            {
                                started = false;
                                //replace
                                string oldValue = "";
                                string newValue = "";

                                if (command.Contains("before") && command.Contains("put"))
                                {
                                    if (command[0] == "before")
                                    {
                                        oldValue = commandText[0];
                                        newValue = commandText[1] + commandText[0];
                                    }
                                    else
                                    {
                                        oldValue = commandText[1];
                                        newValue = commandText[0] + commandText[1];
                                    }
                                }
                                else if (command.Contains("after") && command.Contains("put"))
                                {
                                    if (command[0] == "after")
                                    {
                                        oldValue = commandText[0];
                                        newValue = commandText[0] + commandText[1];
                                    }
                                    else
                                    {
                                        oldValue = commandText[1];
                                        newValue = commandText[1] + commandText[0];
                                    }
                                }
                                else if (command.Contains("replace") && command.Contains("with"))
                                {
                                    if (command[0] == "replace")
                                    {
                                        oldValue = commandText[0];
                                        newValue = commandText[1];
                                    }
                                    else
                                    {
                                        oldValue = commandText[1];
                                        newValue = commandText[0];
                                    }
                                }
                                else
                                {

                                    MessageBox.Show("Zwei nicht zueinander passende Kommandos gefunden\nvor Zeile: " + i + "\nDatei: " + modFile.FullName);
                                    warning = true;
                                }

                                //replace old value with new value
                                if (oldValue != "")
                                {
                                    //before: replace variables
                                    if (mod.Vars.Count > 0)
                                    {
                                        foreach (ModVar var in mod.Vars)
                                        {
                                            newValue = newValue.Replace("$print:" + var.VarName, var.getValueAsString());
                                        }
                                    }

                                    //replace old value with new value
                                    origFileContent = origFileContent.Replace(oldValue, newValue);

                                }
                            }
                            //other commands
                        }
                        else if (commandCount == 0 || commandCount == 1)
                        {
                            //add text 
                            if (commandText[commandCount] != "")
                            {
                                //add break line 
                                commandText[commandCount] += "\r\n";
                            }
                            commandText[commandCount] += line;
                        }
                    }
                    reader.Close();

                    //write content to file
                    File.WriteAllText(newModFile.FullName, origFileContent, Encoding.Default);
                }

                //remember
                if (newModFile != null && !copyExists)
                {
                    rememberForRestore(newModFile, type);
                }
            }

            DirectoryInfo[] modDirectories = modDirectory.GetDirectories();
            DirectoryInfo[] gameDirectories = gameDirectory.GetDirectories();
            foreach (DirectoryInfo modDir in modDirectories)
            {
                //search for game directory
                DirectoryInfo rightGameDir = null;
                foreach (DirectoryInfo gameDir in gameDirectories)
                {
                    if (gameDir.Name == modDir.Name)
                    {
                        rightGameDir = gameDir;
                        break;
                    }
                }
                //if game directory does not exist, create it
                if (rightGameDir == null)
                {
                    rightGameDir = gameDirectory.CreateSubdirectory(modDir.Name);
                    //TODO merke neues verzeichnis fuer wiederherstellung
                }
                //same procedure for subdirectrory
                letsMod(mod, modDir, rightGameDir);
            }
        }

        private void rememberForRestore(FileInfo file, string type)
        {
            //remember
            StreamWriter writer = new StreamWriter(exePath + "\\" + restoreFileName, true, Encoding.Default);
            writer.WriteLine(type + "||" + file.FullName);
            writer.Flush();
            writer.Close();
        }

        private void restore()
        {
            if (File.Exists(exePath + "\\" + restoreFileName))
            {
                StreamReader reader = new StreamReader(exePath + "\\" + restoreFileName, Encoding.Default);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split("||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string type = parts[0];
                    string filename = parts[1];

                    //delete file if type is "del" or type is "res" and a copy exists
                    if (File.Exists(filename) && (type == "del" || (type == "res" && File.Exists(filename + copyFileEnding))))
                    {
                        //delete mod file
                        try
                        {
                            File.Delete(filename);
                        }
                        catch { }
                    }

                    //delete copy if two mods added the same NEW file
                    if (type == "del" && File.Exists(filename + copyFileEnding))
                    {
                        //delete mod file copy
                        try
                        {
                            File.Delete(filename + copyFileEnding);
                        }
                        catch { }
                    }

                    //restore file if type is res
                    if (type == "res" && File.Exists(filename + copyFileEnding))
                    {
                        //restore game file
                        try
                        {
                            File.Move(filename + copyFileEnding, filename);
                        }
                        catch { }
                    }
                }
                reader.Close();
                //delete restore file
                File.Delete(exePath + "\\" + restoreFileName);
            }
        }

        private void saveActiveMods()
        {
            //delete old file
            if (File.Exists(exePath + "\\" + activeModsFileName))
            {
                File.Delete(exePath + "\\" + activeModsFileName);
            }

            if (activeMods.Count > 0)
            {
                //save
                StreamWriter writer = new StreamWriter(exePath + "\\" + activeModsFileName, true, Encoding.Default);
                foreach (Mod mod in activeMods)
                {
                    string line = mod.ModDirectoryName;
                    if (mod.Vars.Count > 0)
                    {
                        line += "|";
                        foreach (ModVar var in mod.Vars)
                        {
                            line += var.VarName + ":" + var.getValueAsString() + ";";
                        }
                    }
                    writer.WriteLine(line);
                }
                writer.Flush();
                writer.Close();
            }
        }

        private void button_mod_settings_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBox2.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < activeMods.Count)
            {
                Mod mod = activeMods.ElementAt(selectedIndex); //get mod
                FormModSettings form = new FormModSettings(mod);
                form.Show();
            }
        }
    }
}