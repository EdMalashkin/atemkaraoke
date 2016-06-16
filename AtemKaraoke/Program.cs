﻿using System;
using AtemKaraoke.Lib;
using AtemKaraoke.Lib.Tools;

namespace AtemKaraoke
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Convertor c = new Convertor();
				//c.ConvertSongsToImages("C:\\Projects\\AtemKaraoke\\AtemKaraoke\\Songs\\", "*.txt", "C:\\Projects\\AtemKaraoke\\AtemKaraoke\\Songs\\");
				c.ConvertSongsToImages();
			}
			catch(Exception ex)
			{
				Console.Write(ex.Message);
				ExceptionHelper.HandleException(ex);
			}
		}
	}
}
