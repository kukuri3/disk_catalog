using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;


//using System.IO;



namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string gDrive;
        string gSerial;
        int gLabelcount = 0;

        public Form1()
        {
            InitializeComponent();
            // 選択モードを行単位での選択のみにする
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void xGetDriveInfo()
        {
                        //ドライブの情報を取得する
            
            gDrive = driveListBox1.SelectedItem.ToString().Substring(0,2);  //ドライブ文字を取得
            
            System.Management.ManagementObject mo =
            new System.Management.ManagementObject("Win32_LogicalDisk=\"" + gDrive + "\"");


            //名前（"C:"など）
            textBox1.AppendText("名前:{0}" +
                (string)mo.Properties["Name"].Value + "\r\n");
            //ドライブタイプ
            string typeDescription = "";
            switch ((uint)mo.Properties["DriveType"].Value)
            {
                case 5:
                    typeDescription = "光ディスクドライブ(CD-ROM,DVD-ROMなど)";
                    break;
                case 3:
                    typeDescription = "固定ディスク";
                    break;
                case 4:
                    typeDescription = "ネットワークドライブ";
                    break;
                case 1:
                    typeDescription = "ルートディレクトリがない";
                    break;
                case 6:
                    typeDescription = "RAMディスク";
                    break;
                case 2:
                    typeDescription = "リムーバブルストレージデバイス" +
                        "フロッピーディスクドライブ、USBフラッシュドライブなど";
                    break;
                case 0:
                    typeDescription = "不明";
                    break;
            }
            textBox1.AppendText("ドライブタイプ:{0}" + typeDescription + "\n");
            //ボリュームラベル
            textBox1.AppendText("ボリュームラベル:{0}" +
                (string)mo.Properties["VolumeName"].Value + "\n");
            //ボリュームシリアルナンバー
            textBox1.AppendText("ボリュームシリアルナンバー:{0}" +
                (string)mo.Properties["VolumeSerialNumber"].Value + "\n");
            gSerial = (string)mo.Properties["VolumeSerialNumber"].Value;
            //ファイルシステム(NTFS、FAT32など)
            textBox1.AppendText("ファイルシステム:{0}" +
                (string)mo.Properties["FileSystem"].Value + "\n");
            //説明（「ローカル固定ディスク」など）
            textBox1.AppendText("説明:{0}" +
                (string)mo.Properties["Description"].Value + "\n");

            //ドライブの容量を取得する
            ulong ts = (ulong)mo.Properties["Size"].Value;
            textBox1.AppendText("サイズGB" + ts / 1000000000 + "\n");
            //C:のドライブの空き容量を取得する
            ulong fs = (ulong)mo.Properties["FreeSpace"].Value;
            textBox1.AppendText("空き容量GB" + fs / 1000000000 + "\n");
            mo.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            //ドライブ情報を取得
            xGetDriveInfo();

            //ドライブ内のファイル情報を取得
            string[] stFilePathes;
            string ss = "";
            int i = 0;

            DateTime now;
            TimeSpan elapsed;

            now = DateTime.Now;
            textBox1.AppendText("start time=" + now+"\n");
            stFilePathes = GetFilesMostDeep(gDrive + "\\", "*.*");  //探索する
            elapsed = DateTime.Now - now;
            textBox1.AppendText("elapsed="+elapsed+"\n");
            int num = stFilePathes.Length;

            
            textBox1.AppendText("num=" + num + "\n");
            textBox1.Refresh();
            // 取得したファイル名を列挙する
            label1.Text = "登録中..";
            int j = 0;
            foreach (string stFilePath in stFilePathes)
            {
                string ext = System.IO.Path.GetExtension(stFilePath);   //ext
                string fn = System.IO.Path.GetFileName(stFilePath);   //fn
                string fn2 = System.IO.Path.GetFileName(stFilePath);   //fn2
                string fullpath = System.IO.Path.GetFullPath(stFilePath).Substring(2); //fullpathドライブ名を除いたパス
                DateTime dtUpdate = System.IO.File.GetLastWriteTime(stFilePath);    //date
                System.IO.FileInfo fi = new System.IO.FileInfo(stFilePath);
                long filesize = fi.Length;
                int wasuu = 0;


//                if (true)
                    if (ext.Equals(".txt"))
                    {
  
                    //datagridviewに追加。
                    //no,volume,name,ext,date,size,fullpath
                        dataGridView1.Rows.Add(i, gSerial, fn, fn2, wasuu,ext, dtUpdate, filesize, fullpath, "-", "-");
                    i++;
                    
                }
                if ((j % 100) == 0)
                {
                    progressBar1.Value = j * 100 / num;
                    progressBar1.Refresh();
                    // メッセージ・キューにあるWindowsメッセージをすべて処理する(応答なしの回避)
                    Application.DoEvents();
                }
                j++;

            }
            textBox1.AppendText("total " + i + "\n");
            textBox1.AppendText("linenum=" + dataGridView1.RowCount+"\n");

        }
        /// ---------------------------------------------------------------------------------------
        /// <summary>
        ///     指定した検索パターンに一致するファイルを最下層まで検索しすべて返します。</summary>
        /// <param name="stRootPath">
        ///     検索を開始する最上層のディレクトリへのパス。</param>
        /// <param name="stPattern">
        ///     パス内のファイル名と対応させる検索文字列。</param>
        /// <returns>
        ///     検索パターンに一致したすべてのファイルパス。</returns>
        /// ---------------------------------------------------------------------------------------
        public string[] GetFilesMostDeep(string stRootPath, string stPattern)
        {
            System.Collections.Specialized.StringCollection hStringCollection = (
                new System.Collections.Specialized.StringCollection()
            );

            // このディレクトリ内のすべてのファイルを検索する
            try
            {
                foreach (string stFilePath in System.IO.Directory.GetFiles(stRootPath, stPattern))
                {

                    hStringCollection.Add(stFilePath);
                    if ((gLabelcount % 100) == 0)  //時々表示
                    {
                        label1.Text = "探索中.."+stFilePath;
                        label1.Refresh();
                        // メッセージ・キューにあるWindowsメッセージをすべて処理する(応答なしの回避)
                        Application.DoEvents();
                    }
                    gLabelcount++;
                }

            }
            catch (Exception ex)
            {
            }


            // このディレクトリ内のすべてのサブディレクトリを検索する (再帰)
            try
            {
                foreach (string stDirPath in System.IO.Directory.GetDirectories(stRootPath))
                {
                    string[] stFilePathes = GetFilesMostDeep(stDirPath, stPattern);

                    // 条件に合致したファイルがあった場合は、ArrayList に加える
                    if (stFilePathes != null)
                    {
                        hStringCollection.AddRange(stFilePathes);

                    }
                }
            }
            catch (Exception ex)
            {
            }

            // StringCollection を 1 次元の String 配列にして返す
            string[] stReturns = new string[hStringCollection.Count];
            hStringCollection.CopyTo(stReturns, 0);

            return stReturns;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            // 保存用のファイルを開く(true=追記モード)
            using (StreamWriter writer = new StreamWriter("test2.csv", true, Encoding.GetEncoding("shift_jis")))
            {

                int rowCount = dataGridView1.Rows.Count;

                xLog("保存中...");

                // ユーザによる行追加が許可されている場合は、最後に新規入力用の
                // 1行分を差し引く
                if (dataGridView1.AllowUserToAddRows == true)
                {
                    rowCount = rowCount - 1;
                }

                // 行
                for (int i = 0; i < rowCount; i++)
                {
                    // リストの初期化
                    List<String> strList = new List<String>();

                    // 列
                    String strCsvData = "";
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        strCsvData+="\""+(dataGridView1[j, i].Value.ToString())+"\""+",";
                    }
 
                    writer.WriteLine(strCsvData);
                    progressBar1.Value = ((i+1)*100) / rowCount;
                    progressBar1.Update();
                }
                xLog("終了。\r\n");

            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            //csvファイルのload

            
            TextFieldParser parser = new TextFieldParser("test2.csv", Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ

            string[] setLines;

            //予め行数を取得
            string fileName = "test2.csv";
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("shift_jis");

            setLines = System.IO.File.ReadAllLines(fileName, enc);

            int lines = setLines.Length;
            xLog("lines=" + lines);



            // データをすべてクリア
            dataGridView1.Rows.Clear();

            int linenum = 0;

            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields(); // 1行読み込み
                // 読み込んだデータ(1行をDataGridViewに表示する)
                dataGridView1.Rows.Add(row);
                linenum++;
                progressBar1.Value = ((linenum*100 + 1) / lines);
                progressBar1.Update();
                
            }
        }
        private void xDispProcessInfo()
        {
            // 自プロセスを取得
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

            // リフレッシュしないとプロセスの各種情報が最新情報に更新されない
            currentProcess.Refresh();

            // 各種プロセス情報を出力する

            label2.Text = "メモリ使用量=" + currentProcess.WorkingSet64 / 1000000 + "MB";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            xDispProcessInfo();
        }

        private void xLog(String s)
        {
            //ログ出力
            DateTime dt = DateTime.Now;
            textBox1.AppendText(dt.ToString() + " " + s + "\n");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(@"log.txt", true, System.Text.Encoding.GetEncoding("shift_jis"));
            sw.Write(dt.ToString() + s + "\r\n");
            sw.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            xDupCheck();
        }
        private void xDupCheck()
        {
            //dgv内の重複をチェック
            //行数を取得
            int rowCount = dataGridView1.Rows.Count;
            // ユーザによる行追加が許可されている場合は、最後に新規入力用の
            // 1行分を差し引く
            if (dataGridView1.AllowUserToAddRows == true)
            {
                rowCount = rowCount - 1;
            }

            int dupcount = 0;
            for (int i = 0; i < rowCount; i++)
            {
                progressBar1.Value = (i*100 + 1) / rowCount;
                progressBar1.Refresh();
                for (int j = i+1; j < rowCount; j++)
                {
                    string fn0 = dataGridView1["fullpath", i].Value.ToString();    
                    string fn1 = dataGridView1["fullpath", j].Value.ToString();    
                    string vol0 = dataGridView1["volume", i].Value.ToString();
                    string vol1 = dataGridView1["volume", j].Value.ToString();
                    if ((fn0 == fn1) && (vol0 == vol1))
                    {
                        dupcount++;
                        dataGridView1["list_dup", i].Value = "o";
                    }
                }
            }
            xLog("重複数="+dupcount);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            xDupDelete();
        }
        private void xDupDelete()
        {
            //行数を取得
            int rowCount = dataGridView1.Rows.Count;
            // ユーザによる行追加が許可されている場合は、最後に新規入力用の
            // 1行分を差し引く
            if (dataGridView1.AllowUserToAddRows == true)
            {
                rowCount = rowCount - 1;
            }

            //dgv内の重複を削除
            for (int i = rowCount-1; i >=0; i--)
            {
                progressBar1.Value = (i * 100 + 1) / rowCount;
                progressBar1.Refresh();
                if (dataGridView1["list_dup",i].Value == "o")
                {
                    dataGridView1.Rows.RemoveAt(i);
                }
            }
        }
    }

}
