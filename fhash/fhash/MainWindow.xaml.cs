using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace fhash
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public event EventHandler<TextBoxUpdater> TBupdate;

		public MainWindow()
		{
			InitializeComponent();
			TBupdate += HandleTextBoxUpdate;
			checkBox1.IsChecked = Properties.Settings.Default.SHA512;
			checkBox2.IsChecked = Properties.Settings.Default.SHA256;
			checkBox3.IsChecked = Properties.Settings.Default.SHA128;
			checkBox4.IsChecked = Properties.Settings.Default.MD5;
		}

		private void textBox1_PreviewDragOver(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.Copy;
			e.Handled = true;
		}

		public void HandleTextBoxUpdate(object sender, TextBoxUpdater e)
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>((a) => {
				string fn = (string)a;
				textBox1.Text = fn; 
			}),e.Text);
		}
		private void textBox1_PreviewDrop(object sender, DragEventArgs e)
		{
			e.Handled = true;
			if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
			{
				string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

				textBox1.Text = "Computing...";

				var task = new Task((obj) =>
				{
					myText mt = (myText)obj;
					var handler = TBupdate;
					if (handler != null)
					{
						string output = "";
						for (int i = 0; i < mt.FileNames.Length; i++ )
						{
							FileInfo info = new FileInfo(mt.FileNames[i]);
							if (info.Attributes != FileAttributes.Directory)
							{
								try { output = output + info.FullName + " (" + info.Length + " bytes)" + "\n"; }
								catch (Exception ioex) { output = output + ioex.Message + "\n"; }
								
								//textBox1.Text = textBox1.Text + "SHA-512:\n";
								if (mt.sha512)
								{
									try
									{
										output = output + "\n";
										using (FileStream stream = File.Open(mt.FileNames[i], FileMode.Open))
										{
											output = output + "SHA-512:\n";
											SHA512CryptoServiceProvider csp = new SHA512CryptoServiceProvider();
											byte[] result = csp.ComputeHash(stream);
											output = output + PrintByteArray(result);
											output = output + "\n";
										}
									}
									catch (Exception ioex)
									{
										output = output + "SHA-512: " + ioex.Message + "\n";
									}
								}
								if (mt.sha256)
								{
									try
									{
										output = output + "\n";
										using (FileStream stream = File.Open(mt.FileNames[i], FileMode.Open))
										{
											output = output + "SHA-256:\n";
											SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider();
											byte[] result = csp.ComputeHash(stream);
											output = output + PrintByteArray(result);
											output = output + "\n";
										}
									}
									catch (Exception ioex)
									{
										output = output + "SHA-256: "+ ioex.Message + "\n";
									}
								}
								if (mt.sha128)
								{
									try
									{
										output = output + "\n";
										using (FileStream stream = File.Open(mt.FileNames[i], FileMode.Open))
										{
											output = output + "SHA-128:\n";
											SHA1CryptoServiceProvider csp = new SHA1CryptoServiceProvider();
											byte[] result = csp.ComputeHash(stream);
											output = output + PrintByteArray(result);
											output = output + "\n";
										}
									}
									catch (Exception ioex)
									{
										output = output + "SHA-128: " + ioex.Message + "\n";
									}
								}
								if (mt.md5)
								{
									try
									{									
										output = output + "\n";
										using (FileStream stream = File.Open(mt.FileNames[i], FileMode.Open))
										{
											output = output + "MD5:\n";
											MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();
											byte[] result = csp.ComputeHash(stream);
											output = output + PrintByteArray(result);
											output = output + "\n";
										}
									}
									catch (Exception ioex)
									{
										output = output + "MD5: " + ioex.Message + "\n";
									}

								}

							}
							else
							{
								output = output + info.FullName + " is a directory\n";
							}
							output = output + "\n";
						}							
						handler(this, new TextBoxUpdater(output));

					}

				}, new myText() { FileNames = fileNames, sha512 = (bool)checkBox1.IsChecked, sha256 = (bool)checkBox2.IsChecked, sha128 = (bool)checkBox3.IsChecked, md5 = (bool)checkBox4.IsChecked}
				);
				task.Start();

			}
		}

		public static string PrintByteArray(byte[] array)
		{
			string result = "";

			int i;
			for (i = 0; i < array.Length; i++)
			{
				result = result + String.Format("{0:X2}", array[i]);
				if ((i % 4) == 3) result = result + " ";
			}
			Console.WriteLine();
			return result;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.SHA512 = (bool)checkBox1.IsChecked;
			Properties.Settings.Default.SHA256 = (bool)checkBox2.IsChecked;
			Properties.Settings.Default.SHA128 = (bool)checkBox3.IsChecked;
			Properties.Settings.Default.MD5 = (bool)checkBox4.IsChecked;

			Properties.Settings.Default.Save();
		}

	}

	public class TextBoxUpdater : EventArgs
	{
		public string Text { get; private set; }
		
		public TextBoxUpdater(string str)
		{
				  Text = str;
		}
	}
	public class myText
	{
		public string[] FileNames;
		public bool sha512;
		public bool sha256;
		public bool sha128;
		public bool md5;
	}
}
