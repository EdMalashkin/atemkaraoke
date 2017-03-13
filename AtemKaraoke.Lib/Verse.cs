﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtemKaraoke.Lib
{
	public class Verse
	{
		private string _Text;
		public string Text
		{
			get
			{
				return _Text;
			}
			set
			{
				_Text = TrimRows(value);
			}
		}

		private string _Name;
		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				_Name = value;
			}
		}

		private int _Number;
		public int Number
		{
			get
			{
				return _Number;
			}
			set
			{
				_Number = value;
			}
		}

		private int _startPosition;
		public int StartPosition
		{
			get
			{
				return _startPosition;
			}
			set
			{
				_startPosition = value;
			}
		}

		public int EndPosition
		{
			get
			{
				return StartPosition + Text.Length;
			}
		}

		private string _FilePath;
		public string FilePath
		{
			get
			{
				return _FilePath;
			}
			set
			{
				_FilePath = value;
			}
		}

		private string TrimRows(string text)
		{
			string[] rows = Regex.Split(text, Environment.NewLine);
			string newText = "";
			foreach (string r in rows)
			{
				newText += r.Trim() + Environment.NewLine;
			}
			return newText.Trim().Replace("  ", " ");
		}


	}
}