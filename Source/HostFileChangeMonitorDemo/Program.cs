using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostFileChangeMonitorDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			FileContentProvider textFile = new FileContentProvider(Path.Combine(Environment.CurrentDirectory, @"test.txt"));
			Stopwatch sw = new Stopwatch();
			while (true)
			{
				sw.Reset();
				sw.Start();
				Console.WriteLine(textFile.Content);
				sw.Stop();
				Console.WriteLine("Elapsed Time: {0} ms", sw.ElapsedMilliseconds);
				Console.WriteLine(new string('=', 50));
				Console.ReadLine();
			}
		}
	}

	public class FileContentProvider
	{
		public String File { get; private set; }
		public String Content
		{
			get
			{
				const string CACHE_KEY = "Content";
				string content = m_Cache[CACHE_KEY] as string;
				if (content == null)
				{
					CacheItemPolicy policy = new CacheItemPolicy();
					policy.SlidingExpiration = TimeSpan.FromMilliseconds(1500);

					var changeMonitor = new HostFileChangeMonitor(new List<string> { File });

					policy.ChangeMonitors.Add(changeMonitor);
					
					content = System.IO.File.ReadAllText(File);
					Thread.Sleep(1000);
					m_Cache.Set(CACHE_KEY, content, policy);
				}
				return content;
			}
		}

		private ObjectCache _cache;
		private ObjectCache m_Cache
		{
			get
			{
				if (_cache == null)
					_cache = MemoryCache.Default;
				return _cache;
			}
		}

		public FileContentProvider(String file)
		{
			this.File = file;
		}
	}
}
