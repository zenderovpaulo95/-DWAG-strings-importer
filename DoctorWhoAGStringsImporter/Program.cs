using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace DoctorWhoAGStringsImporter
{
    class Program
    {

        [DllImport("Kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWind, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int DEFAULT_ENCODING_CODE = 1251; //Cyrillic encoding
        static char[] syms = { '#', '$', '%', '*', '+', '<', '>', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~' };
        static string[] strs; //Для русских и английских букв
        static bool needChange = false;

        private class xmldata
        {
            public string actor; //Имя персонажа
            public string id; //ID строки
            public string text; //Текст персонажа

            public xmldata() { }
            public xmldata(string _actor, string _id, string _text)
            {
                this.actor = _actor;
                this.id = _id;
                this.text = _text;
            }
        }

        private class stringsdata
        {
            public string fileName;
            public string stringActor;

            public stringsdata() { }
            public stringsdata(string _fileName, string _stringActor)
            {
                fileName = _fileName;
                stringActor = _stringActor;
            }
        }

        static void MakeTxt(string unused_lua, string unused_evp)
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\unused_lua.txt")) File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\unused_lua.txt");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\unused_evp.txt")) File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\unused_evp.txt");

            if (unused_lua != null)
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\unused_lua.txt", FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(unused_lua);
                sw.Close();
                fs.Close();
            }

            if (unused_evp != null)
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\unused_evp.txt", FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(unused_evp);
                sw.Close();
                fs.Close();
            }
        }

        static void MakeTxt(List<string> unused_lua, List<string> unused_evp)
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\unused_lua.txt")) File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\unused_lua.txt");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\unused_evp.txt")) File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\unused_evp.txt");

            if (unused_lua != null)
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\unused_lua.txt", FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                for (int i = 0; i < unused_lua.Count; i++)
                {
                    sw.Write(unused_lua[i]);
                    sw.Write("\r\n");
                }
                sw.Close();
                fs.Close();
            }

            if (unused_evp != null)
            {
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\unused_evp.txt", FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                for (int i = 0; i < unused_evp.Count; i++)
                {
                    sw.Write(unused_evp[i]);
                    sw.Write("\r\n");
                }
                sw.Close();
                fs.Close();
            }
        }

        static string changechars(string str, string or, string tr)
        {
            if (or.Length == tr.Length)
            {
                for (int j = 0; j < or.Length; j++)
                {
                    str = str.Replace(tr[j], or[j]);
                }
            }

            return str;
        }

        static void checkstrings(List<xmldata> xmlex, FileInfo[] fi1, FileInfo[] fi2, bool check_only, int code_encoding, bool add_sym, string sym, List<stringsdata> luatxt, List<stringsdata> evptxt, bool isDialogTag, string searchTag)
        {
            //string unused_lua = null, unused_evp = null;
            List<string> unused_lua = new List<string>();
            List<string> unused_evp = new List<string>();
            bool hasnt_id, new_file, modified;

            for (int i = 0; i < fi1.Length; i++) //Lua
            {
                string[] lua = null;

                if (check_only)
                {
                    byte[] templua = File.ReadAllBytes(fi1[i].FullName);
                    lua = Bin2String(templua, code_encoding);
                    if (lua == null) lua = File.ReadAllLines(fi1[i].FullName);
                    templua = null;
                }
                else lua = File.ReadAllLines(fi1[i].FullName);

                hasnt_id = true;
                new_file = true;
                modified = false;

                for (int l = 0; l < lua.Length; l++) //lua
                {
                    string checkstr = lua[l].ToLower();
                    int comment_index = checkstr.IndexOf("--");

                    if ((checkstr.IndexOf("::sid_") > 0) && (comment_index < 0))
                    {
                        for (int m = 0; m < xmlex.Count; m++)
                        {
                            if (needChange) xmlex[m].text = changechars(xmlex[m].text, strs[1], strs[0]);

                            if (checkstr.IndexOf(xmlex[m].actor.ToLower() + "::sid_" + xmlex[m].id + ":") >= 0)
                            {
                                if (!check_only)
                                {

                                    string temp = xmlex[m].text;

                                    if (add_sym)
                                    {
                                        if (xmlex[m].text.Length > 0)
                                        {
                                            char ch = xmlex[m].text[0];
                                            if (ch > 0x7E || ch == 0x20)
                                            {
                                                temp = sym + xmlex[m].text;
                                            }
                                        }
                                    }


                                    int index = lua[l].IndexOf("::SID_" + xmlex[m].id + ":") + 11;
                                    string temporary = lua[l].Substring(0, index) + temp;
                                    lua[l] = temporary;

                                    modified = true;
                                }

                                hasnt_id = false;
                            }
                        }

                        if ((checkstr.IndexOf("::sid_") > 0) && (hasnt_id) && (check_only))
                        {
                            if (new_file)
                            {
                                //unused_lua += fi1[i].Name + "\r\n";
                                unused_lua.Add(fi1[i].Name);
                                new_file = false;
                            }

                            //unused_lua += lua[l] + "\r\n";
                            unused_lua.Add(lua[l]);
                        }
                        else if ((checkstr.IndexOf("::sid_") > 0) && (hasnt_id) && (!check_only))
                        {
                            if (luatxt.Count > 0)
                            {
                                for (int k = 0; k < luatxt.Count; k++)
                                {
                                    if (needChange) luatxt[k].stringActor = changechars(luatxt[k].stringActor, strs[1], strs[0]);

                                    int index = luatxt[k].stringActor.ToLower().IndexOf("::sid_");
                                    string checkid = null;
                                    if (index >= 0) checkid = luatxt[k].stringActor.Substring(index + 6, 4);

                                    if ((luatxt[k].fileName.ToLower() == fi1[i].Name.ToLower())
                                        && (index > 0) && (checkstr.IndexOf(checkid) > 0) && (checkid != null))
                                    {
                                        lua[l] = luatxt[k].stringActor;
                                        if (add_sym)
                                        {
                                            if (index + 12 < luatxt[k].stringActor.Length)
                                            {
                                                char[] ch_m = luatxt[k].stringActor.Substring(index + 11, 1).ToCharArray();
                                                char ch = ch_m[0];
                                                if (ch > 0x7E || ch == 0x20)
                                                {
                                                    string temp = luatxt[k].stringActor.Substring(0, index + 11);
                                                    temp += sym + luatxt[k].stringActor.Substring(index + 11, luatxt[k].stringActor.Length - (index + 11));
                                                    lua[l] = temp;
                                                }
                                            }
                                        }
                                        modified = true;
                                        hasnt_id = false;
                                    }
                                }
                            }
                        }
                    }
                    hasnt_id = true;
                }

                if (modified)
                {
                    File.WriteAllLines(fi1[i].FullName, lua);
                    Console.WriteLine("Файл " + fi1[i].Name + " модифицирован.");
                    lua = null;
                }
            }

            int c = 0;
            int d = 0;

            for (int j = 0; j < fi2.Length; j++) //Evp
            {
                string[] evp = File.ReadAllLines(fi2[j].FullName);

                hasnt_id = true;
                new_file = true;
                modified = false;

                if (evptxt == null || evptxt.Count <= 0)
                {
                    for (int k = 0; k < evp.Length; k++)
                    {
                        if (isDialogTag && (evp[k].IndexOf("SID_") > 0)
                            && (evp[k].IndexOf("<WhatToSay>") > 0))
                        {
                            string check = evp[k];

                            for (int l = 0; l < xmlex.Count; l++)
                            {
                                if (check.IndexOf("SID_" + xmlex[l].id + ":") > 0)
                                {
                                    if (!check_only)
                                    {
                                        int index = evp[k].IndexOf("SID_") + 9;
                                        string temp = evp[k].Substring(0, index) + xmlex[l].text + "</WhatToSay>";

                                        if (add_sym)
                                        {
                                            temp = evp[k].Substring(0, index) + sym + xmlex[l].text + "</WhatToSay>";
                                        }

                                        evp[k] = temp;
                                        modified = true;
                                    }

                                    hasnt_id = false;
                                }
                            }

                            if ((evp[k].IndexOf("SID_") > 0) && (evp[k].IndexOf("<WhatToSay>") > 0) && (hasnt_id) && (check_only))
                            {
                                if (new_file)
                                {
                                    //unused_evp += fi2[j].Name + "\r\n";
                                    unused_evp.Add(fi2[j].Name);
                                    new_file = false;
                                }

                                //unused_evp += evp[k] + "\r\n";
                                unused_evp.Add(evp[k]);
                            }
                            else if ((evp[k].IndexOf("SID_") > 0) && (evp[k].IndexOf("<WhatToSay>") > 0) && (hasnt_id) && (!check_only)
                                && (evptxt.Count > 0 || evptxt != null))
                            {
                                var handler = GetConsoleWindow();
                                ShowWindow(handler, SW_SHOW);

                                Console.WriteLine("Если вы видите эти сообщения, значит, что-то пошло не так. Сообщите мне.");
                            }
                        }
                        else if (!isDialogTag && (evp[k].IndexOf("<" + searchTag + ">") >= 0))
                        {
                                if (new_file)
                                {
                                    //unused_evp += fi2[j].Name + "\r\n";
                                    unused_evp.Add(fi2[j].Name);
                                    new_file = false;
                                }

                                //unused_evp += evp[k] + "\r\n";
                                unused_evp.Add(evp[k]);
                        }

                        hasnt_id = true;
                    }

                    if (modified)
                    {
                        File.WriteAllLines(fi2[j].FullName, evp);
                        Console.WriteLine("Файл {0} успешно модифицирован.", fi2[j].Name);
                    }
                }
                else
                {
                    d = 0;

                    while ((c < evptxt.Count) && (evptxt[c].fileName == fi2[j].Name))
                    {
                        int s_index = evptxt[c].stringActor.IndexOf("<") + 1;
                        int e_index = evptxt[c].stringActor.IndexOf(">");
                        searchTag = evptxt[c].stringActor.Substring(s_index, e_index - s_index);

                        while ((d < evp.Length) && (c < evptxt.Count))
                        {
                            if (evp[d].IndexOf("<" + searchTag + ">") >= 0)
                            {
                                evp[d] = evptxt[c].stringActor;
                                modified = true;
                                c++;
                            }
                            d++;
                        }
                    }

                    if(modified)
                    {
                        File.WriteAllLines(fi2[j].FullName, evp);
                        Console.WriteLine("Файл {0} успешно модифицирован.", fi2[j].Name);
                    }
                }
            }

            if (check_only) MakeTxt(unused_lua, unused_evp);
        }

        static string[] Bin2String(byte[] file, int encoding)
        {
            string text = null;
            int offset = 0;

            if (file.Length > 0)
            {
                while (offset <= file.Length)
                {
                    byte[] check = new byte[1];

                    Array.Copy(file, offset, check, 0, check.Length);

                    if (check[0] > 0x7F)
                    {
                        if ((check[0] >> 5) == 6)
                        {
                            check = new byte[2];
                            Array.Copy(file, offset, check, 0, check.Length);
                            if (((check[1] >> 6) != 2))
                            {
                                text += Encoding.GetEncoding(encoding).GetString(check);
                                offset += check.Length;
                            }
                            else
                            {
                                text += Encoding.UTF8.GetString(check);
                                offset += check.Length;
                            }

                        }
                        else if ((check[0] >> 4) == 14)
                        {
                            check = new byte[3];
                            Array.Copy(file, offset, check, 0, check.Length);
                            if (((check[1] >> 6) != 2) || ((check[2] >> 6) != 2))
                            {
                                text += Encoding.GetEncoding(encoding).GetString(check);
                                offset += check.Length;
                            }
                            else
                            {
                                text += Encoding.UTF8.GetString(check);
                                offset += check.Length;
                            }
                        }
                        else if ((check[0] >> 3) == 30)
                        {
                            check = new byte[4];
                            Array.Copy(file, offset, check, 0, check.Length);
                            if ((check[1] >> 6 != 2) || (check[2] >> 6 != 2) || (check[3] >> 6 != 2))
                            {
                                text += Encoding.GetEncoding(encoding).GetString(check);
                                offset += check.Length;
                            }
                            else
                            {
                                text += Encoding.UTF8.GetString(check);
                                offset += check.Length;
                            }
                        }
                        else
                        {
                            text += Encoding.GetEncoding(encoding).GetString(check);
                            offset += check.Length;
                        }
                    }
                    else
                    {
                        text += Encoding.GetEncoding(encoding).GetString(check);
                        offset += check.Length;
                    }

                    if (offset >= file.Length) break;
                }

                if (text.IndexOf("\r\n") >= 0) text = text.Replace("\r\n", "\n");

            }
            string[] result = null;
            if (text != null)
            {
                result = text.Split('\n');
                text = null;
            }

            return result;
        }

        static void readxml(ref List<xmldata> xml, string[] xmlstrs, char sym, string xmlFilePath)
        {
            bool modified = false;

            for (int j = 0; j < xmlstrs.Length; j++)
            {
                if (needChange)
                {
                    if (j == 5)
                    {
                        int puase = 1;
                    }
                    xmlstrs[j] = changechars(xmlstrs[j], strs[1], strs[0]);
                    modified = true;
                }

                if ((xmlstrs[j].IndexOf("<") > 0) && (xmlstrs[j].IndexOf(">") > 0) && (xmlstrs[j].IndexOf("/") > 0)
                    && (xmlstrs[j].IndexOf(":") > 0))
                {
                    string[] check = xmlstrs[j].Split('<', '>', '/', ':');
                    uint id = Convert.ToUInt32(check[1]);
                    check[1] = id.ToString("D4");

                    if (check[4].Length > 0)
                    {
                        string check_str = check[4];
                        char ch = (char)check_str[0];

                        for(int i = 0; i < syms.Length; i++)
                        {
                            if((ch == sym) || (ch == syms[i]))
                            {
                                check[4] = check_str.Remove(0, 1);
                                break;
                            }
                        }

                        /*if(((ch < 0x41) || ((ch > 0x5A) && (ch < 0x61)) || ((ch > 0x7A) && (ch < 0xBF)))
                            && (ch == sym))
                        {
                            check[4] = check_str.Remove(0, 1);
                        }*/

                        /*if (((ch > 0x41) && (ch < 0x5A)) || ((ch > 0x61) && (ch < 0x7A)) || ((ch > 0x7A) && (ch < 0xBF)))
                        {
                            check[4] = check_str;
                        }
                        else if (ch > 0xBF) check[4] = check_str;
                        else check[4] = check_str.Remove(0, 1);*/
                    }

                    xml.Add(new xmldata(check[2], check[1], check[4]));
                }
            }

            if(modified)
            {
                Encoding utf8WithoutBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                File.WriteAllLines(xmlFilePath, xmlstrs, utf8WithoutBOM);
                Console.WriteLine("Файл успешно модифицирован.");
            }
        }

        static List<stringsdata> GetStringsData(string[] strs, string format)
        {
            string filename = null;
            List<stringsdata> temp = new List<stringsdata>();

            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i].ToLower().IndexOf(format.ToLower()) > 0)
                {
                    filename = strs[i];
                }
                else
                {
                    temp.Add(new stringsdata(filename, strs[i]));
                }
            }

            return temp;
        }

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            //ShowWindow(handle, SW_HIDE);
            ShowWindow(handle, SW_SHOW);

            /*args = new string[7];
            args[0] = "addsym";
            args[1] = "~";
            args[2] = "import";
            args[3] = "C:\\Users\\123\\Desktop\\Doctor Who - The Adventure Game\\EP_5\\data\\Common\\Scripts\\EmmersionGameSidToActorScript.xml";
            args[4] = "C:\\Users\\123\\Desktop\\Doctor Who - The Adventure Game\\EP_5\\data";
            args[5] = "C:\\Users\\123\\Desktop\\Doctor Who\\Doctor Who\\DoctorWhoAGStringsImporter\\DoctorWhoAGStringsImporter\\bin\\Release\\unused_lua.txt";
            args[6] = "C:\\Users\\123\\Desktop\\Doctor Who\\Doctor Who\\DoctorWhoAGStringsImporter\\DoctorWhoAGStringsImporter\\bin\\Release\\unused_evp.txt";*/
            /*args = new string[4];
            args[0] = "check";
            args[1] = "Interact_Name";
            args[2] = "C:\\Users\\123\\Desktop\\Doctor Who - The Adventure Game\\EP_5\\data\\Common\\Scripts\\EmmersionGameSidToActorScript.xml";
            args[3] = "C:\\Users\\123\\Desktop\\Doctor Who - The Adventure Game\\EP_5\\data";*/
            /*args = new string[8];
            args[0] = "addsym";
            args[1] = "~";
            args[2] = "change";
            args[3] = "C:\\Users\\123\\Desktop\\Doctor Who\\Doctor Who\\Doctor Who adventure games RESOURCES\\Release\\Steam\\Series1\\replace.txt";
            args[4] = "import";
            args[5] = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Doctor Who The Adventure Games\\Series1\\Data\\Common\\Scripts\\EmmersionGameSidToActorScript.xml";
            args[6] = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Doctor Who The Adventure Games\\Series1\\Data";
            args[7] = "C:\\Users\\123\\Desktop\\Doctor Who\\Doctor Who\\Doctor Who adventure games RESOURCES\\Release\\Steam\\Series1\\unused_lua.txt";*/

            if (args.Length > 0)
            {
                FileInfo[] fi1, fi2;
                bool add_sym = false;
                bool add_xml_sym = false;
                string sym = null;
                int encoding_code = DEFAULT_ENCODING_CODE;

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "change":
                            if((i + 1 < args.Length) && File.Exists(args[i + 1]))
                            {
                                needChange = true;
                                strs = File.ReadAllLines(args[i + 1]);
                            }
                            break;

                        case "code":
                            if(i + 1 < args.Length)
                            {
                                try
                                {
                                    encoding_code = Convert.ToInt32(args[i + 1]);
                                }
                                catch
                                {
                                    encoding_code = DEFAULT_ENCODING_CODE;
                                }
                            }
                            break;

                        case "check":
                            if (i + 3 < args.Length)
                            {
                                string checkTag = args[i + 1];
                                string checkpath = args[i + 2];
                                string[] xmlstrs = null;
                                string checkdirectory = args[i + 3];
                                bool isDialogTag = checkTag == "WhatToSay";
								if (sym == null) sym = "$";

                                if (File.Exists(checkpath) && Directory.Exists(checkdirectory))
                                {
                                    if (checkpath.ToLower().IndexOf(".xml") > 0)
                                    {
                                        //xmlstrs = File.ReadAllLines(checkpath);

                                        byte[] tempxmlbin = File.ReadAllBytes(checkpath);
                                        xmlstrs = Bin2String(tempxmlbin, encoding_code);
                                        tempxmlbin = null;

                                        if (xmlstrs == null) xmlstrs = File.ReadAllLines(checkpath);

                                        //if ((checkdirectory.IndexOf("Series1") > 0) || (checkdirectory.IndexOf("Series2") > 0))
                                        //{
                                            DirectoryInfo di = new DirectoryInfo(checkdirectory);
                                            fi1 = di.GetFiles("*.lua", SearchOption.AllDirectories);
                                            fi2 = di.GetFiles("*.evp", SearchOption.AllDirectories);


										Console.WriteLine(fi1.Length.ToString() + " " + fi2.Length.ToString() + " " + xmlstrs.Length.ToString());

                                            if ((fi1.Length > 0) && (fi2.Length > 0))
                                            {
                                                List<xmldata> xml = new List<xmldata>();
                                                readxml(ref xml, xmlstrs, sym[0], checkpath);
                                                Console.Clear();
                                                checkstrings(xml, fi1, fi2, true, encoding_code, false, null, null, null, isDialogTag, checkTag);
                                            }
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                ShowWindow(handle, SW_SHOW);
                                Console.Clear();
                                Console.WriteLine("Введите команду: " + AppDomain.CurrentDomain.FriendlyName + " check example.xml \"<Путь к папке Series1 или Series2>\"" );
                            }
                            break;

                        case "addsym":
                            if (i + 1 < args.Length)
                            {
                                add_sym = true;
                                sym = args[i + 1];
                            }
                            break;

                        case "addxmlsym":
                            if((add_sym == true) && ((sym != null) || (sym != "")))
                            {
                                add_xml_sym = true;
                            }
                            break;

                        case "replace":
                            if(i + 2 < args.Length)
                            {
                                if (File.Exists(args[i + 1]) && Directory.Exists(args[i + 2]))
                                {
                                    string[] file = File.ReadAllLines(args[i + 1]);

                                    Array.Sort(file, StringComparer.InvariantCulture);

                                    DirectoryInfo di = new DirectoryInfo(args[i + 2]);
                                    FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                                    if (fi.Length > 0)
                                    {
                                        string next_file = "";
                                        string curr_file = "";

                                        List<string[]> mod_list = new List<string[]>();

                                        bool mod = false;

                                        for(int k = 0; k < file.Length; k++)
                                        {
                                            if (needChange) file[k] = changechars(file[k], strs[1], strs[0]);
                                            string[] par = file[k].Split('@');

                                            if (par.Length == 3)
                                            {
                                                curr_file = par[0];

                                                mod_list.Add(par);

                                                if (next_file != "") next_file = par[0];

                                                if (file.Length == 1) mod = true;

                                                if (curr_file != next_file)
                                                {
                                                    next_file = par[0];
                                                    mod = true;
                                                }
                                                
                                                if(mod)
                                                {
                                                    if (mod_list.Count > 0)
                                                    {
                                                        for (int f = 0; f < fi.Length; f++)
                                                        {
                                                            if (fi[f].FullName.IndexOf(mod_list[0][0]) > 0)
                                                            {
                                                                string[] file1 = File.ReadAllLines(fi[f].FullName);

                                                                for (int s = 0; s < mod_list.Count; s++)
                                                                {
                                                                    int idx = Convert.ToInt32(par[2]) - 1;

                                                                    if (add_sym)
                                                                    {
                                                                        int index = 0;

                                                                        while (index != par[1].Length && par[1][index] != ':')
                                                                        {
                                                                            index++;
                                                                        }

                                                                        if (index + 1 < par[1].Length && par[1][index + 1] == ':') index++;

                                                                        string tmp_str = par[1].Substring(0, index + 1) + sym + par[1].Substring(index + 1, par[1].Length - (index + 1));

                                                                        par[1] = tmp_str;
                                                                    }

                                                                    file1[idx] = par[1];
                                                                }

                                                                File.WriteAllLines(fi[f].FullName, file1);

                                                                Console.WriteLine("Файл " + fi[f].Name + " успешно модифицирован!");
                                                            }
                                                        }
                                                    }

                                                    mod = false;
                                                    mod_list.Clear();
                                                }
                                                
                                            }
                                        }
                                    }
                                    else Console.WriteLine("Directory doesn't have files.");
                                }
                                else Console.WriteLine("Check both file and directory path!");
                            }
                            break;

                        case "import":
                            if ((i + 3 < args.Length) || (i + 4 < args.Length))
                            {
                                string checkxmlpath, checkluatxt, checkevptxt, checkdirpath;
                                checkxmlpath = args[i + 1];
                                checkdirpath = args[i + 2];
                                checkluatxt = args[i + 3];
                                checkevptxt = null;
                                if (sym == null) sym = "$";

                                if (i + 4 < args.Length) checkevptxt = args[i + 4];

                                if(File.Exists(checkxmlpath) && File.Exists(checkluatxt)
                                    && Directory.Exists(checkdirpath)
                                    && (checkxmlpath.ToLower().IndexOf(".xml") > 0)
                                    && (checkluatxt.ToLower().IndexOf(".txt") > 0))
                                {
                                    List<xmldata> xml = new List<xmldata>();
                                    //string[] xmlstrs = File.ReadAllLines(checkxmlpath);
                                    //string[] luatxts = File.ReadAllLines(checkluatxt);
                                    byte[] tempxmlbin = File.ReadAllBytes(checkxmlpath);
                                    byte[] templuabin = File.ReadAllBytes(checkluatxt);

                                    string[] xmlstrs = Bin2String(tempxmlbin, encoding_code);
                                    string[] luatxts = Bin2String(templuabin, encoding_code);

                                    if (xmlstrs == null) xmlstrs = File.ReadAllLines(checkxmlpath);
                                    else if (luatxts == null) luatxts = File.ReadAllLines(checkluatxt);

                                    string[] evptxts = null;

                                    readxml(ref xml, xmlstrs, sym[0], checkxmlpath);

                                   //if (checkdirpath.IndexOf("Series1") > 0 || checkdirpath.IndexOf("Series2") > 0)
                                    //{
                                        DirectoryInfo di = new DirectoryInfo(checkdirpath);
                                        fi1 = di.GetFiles("*.lua", SearchOption.AllDirectories);
                                        fi2 = di.GetFiles("*.evp", SearchOption.AllDirectories);

                                        if ((checkevptxt != null) &&
                                            (File.Exists(checkevptxt))
                                            && (checkevptxt.ToLower().IndexOf(".txt") > 0))
                                        {
                                            evptxts = File.ReadAllLines(checkevptxt);
                                        }

                                        if ((fi1.Length > 0) && (fi2.Length > 0) && (xml.Count > 0))
                                        {
                                            List<stringsdata> luatxt = new List<stringsdata>();
                                            List<stringsdata> evptxt = new List<stringsdata>();

                                            if (luatxts.Length > 0)
                                            {
                                                luatxt = GetStringsData(luatxts, ".lua");
                                            }

                                            if ((evptxts != null) && evptxts.Length > 0)
                                            {
                                                evptxt = GetStringsData(evptxts, ".evp");
                                            }

                                            checkstrings(xml, fi1, fi2, false, encoding_code, add_sym, sym, luatxt, evptxt, false, "");

                                            if (add_xml_sym)
                                            {
                                                for(int x = 0; x < xmlstrs.Length; x++)
                                                {
                                                //if (needChange) xmlstrs[x] = changechars(xmlstrs[x], strs[1], strs[0]);
                                                    for(int z = 0; z < xml.Count; z++)
                                                    {
                                                        string id_str = Convert.ToInt32(xml[z].id).ToString();
                                                        if (xmlstrs[x].IndexOf("<" + id_str + ">") >= 0)
                                                        {
                                                            int idx1, idx2;
                                                            idx1 = xmlstrs[x].IndexOf("<" + id_str + ">") + 2 + id_str.Length + xml[z].actor.Length + 2;
                                                            idx2 = xmlstrs[x].IndexOf("</" + id_str + ">");

                                                            string temp = xmlstrs[x].Substring(0, idx1);

                                                            if (xml[z].text.Length > 0)
                                                            {
                                                                char ch = xml[z].text[0];

                                                                if (ch > 0x7E || ch == 0x20)
                                                                {
                                                                    temp += sym;
                                                                }
                                                            }

                                                            temp += xml[z].text + xmlstrs[x].Substring(idx2, xmlstrs[x].Length - idx2);
                                                            xmlstrs[x] = temp;
                                                        }
                                                    }
                                                }

                                                File.WriteAllLines(checkxmlpath, xmlstrs);

                                                xml.Clear();
                                                xmlstrs = null;
                                                templuabin = null;
                                                tempxmlbin = null;
                                            }
                                        }
                                    //}
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                ShowWindow(handle, SW_SHOW);

                Console.Write("Утилита по переносу строк из xml файла\r\nв lua скрипты и evp файлы для Doctor Who Adventure Games.\r\n");
                Console.Write("Команды:\r\ncode - Windows кодировка текста для ANSI текста.\r\ncheck - поиск строк в lua-скриптах, отсутствующих в xml-файле.\r\n");
                Console.Write("addsym %your_sym% - добавлять символ для правильного отображения текста в игре.\r\naddxmlsym - указывается после параметра addsym (пригодится для эпизода \"Пороховой заговор\"\r\n");
                Console.Write("import - замена строк в lua и evp файлах, используя xml файл и недостающие строки, переведённые в отдельном txt файле.");
                Console.Write(AppDomain.CurrentDomain.FriendlyName + " [command <file>] file.xml\r\n\r\n");
                Console.Write("Примеры:\r\n");
                Console.Write("Проверка текста:\r\n" + AppDomain.CurrentDomain.FriendlyName + " check ИскомыйТег \"C:\\Game Folder\\Series 1\\file.xml\" \"C:\\Game Folder\\Series 1\\\"\r\n");
                Console.Write("Замена текста:\r\nВариант 1:\r\n");
                Console.Write(AppDomain.CurrentDomain.FriendlyName + " addsym ~ import \"C:\\Game Folder\\Series 1\\file.xml\" \"C:\\Game Folder\\Series 1\\\" \"C:\\unused_lua.txt\" \"C:\\unused_evp.txt\"\r\n");
                Console.Write("Вариант 2:\r\n");
                Console.Write(AppDomain.CurrentDomain.FriendlyName + " addsym ~ import \"C:\\Game Folder\\Series 1\\file.xml\" \"C:\\Game Folder\\Series 1\\\" \"C:\\unused_lua.txt\"\r\n");
                Console.Write("Вариант 3:\r\n");
                Console.Write(AppDomain.CurrentDomain.FriendlyName + " addsym ~ addxmlsym import \"C:\\Game Folder\\Series 1\\file.xml\" \"C:\\Game Folder\\Series 1\\\" \"C:\\unused_lua.txt\"\r\n");
                Console.Write("Вариант 4:\r\n");
                Console.Write(AppDomain.CurrentDomain.FriendlyName + " code 1251 addsym ~ addxmlsym import \"C:\\Game Folder\\Series 1\\file.xml\" \"C:\\Game Folder\\Series 1\\\" \"C:\\unused_lua.txt\"");
                //Console.Read();
            }
        }
    }
}
