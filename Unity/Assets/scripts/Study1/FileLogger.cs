using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class FileLogger 
{
	private string root;
	private string path;
	private string filename;
	private StreamWriter sw;
	private bool enabled;
	public FileLogger(bool enabled)
	{
		if (enabled)
		{
			this.root = Application.dataPath + "\\Logs";
			// If directory does not exist, don't even try   
			if (!Directory.Exists(this.root))
			{
				Directory.CreateDirectory(this.root);
			}
			this.filename = System.DateTime.UtcNow.ToString("yyyy_MM_dd'T'HH_mm_ss_ff") + ".log";
			this.path = this.root + "\\" + this.filename;
			this.sw = new StreamWriter(this.path, true);
		}
	}

	public void Log(string msg)
	{
		if (enabled) sw.WriteLine(System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff") + "|" + msg);
	}
	public void Close()
	{
		if (enabled) this.sw.Close();
	}
}
