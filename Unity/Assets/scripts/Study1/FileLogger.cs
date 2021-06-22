using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class FileLogger 
{
	private string path;
	private string filename;
	private StreamWriter sw;
	public FileLogger()
	{
		this.filename = System.DateTime.UtcNow.ToString("yyyy_MM_dd'T'HH_mm_ss_ff") + ".log";
		this.path = Application.dataPath + "\\Logs" + "\\" + this.filename;
		this.sw = new StreamWriter(this.path, true);
	}

	public void Log(string msg)
	{
		string timeStamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
		sw.WriteLine(timeStamp + "|" + msg);
	}
	public void Close()
	{
		this.sw.Close();
	}
}
