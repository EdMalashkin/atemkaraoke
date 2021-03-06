﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace AtemKaraoke.Core
{
    [Serializable]
    public class VerseDrawing
    {
        Verse _verse;
        public VerseDrawing(Verse verse)
        {
            _verse = verse;
        }

        public Verse Verse
        {
            get
            {
                return _verse;
            }
        }

        public Bitmap Image
        {
            get
            {
                return GetImage(Text);
            }
        }

        public string Text
        {
            get
            {
                return CutComments(_verse.Text);
            }
        }

        private string CutComments(string verseText)
        {
            StringBuilder result = new StringBuilder();
            string[] rows = Regex.Split(verseText, Environment.NewLine);
            for (int i = 0; i < rows.Count(); i++)
            {
                if (!rows[i].TrimStart().StartsWith(Config.Default.CommentSign)) // if the row is not a comment as whole
                {
					if (rows[i].Contains(Config.Default.CommentSign)) // if still contains inline comment
					{
						result.Append(rows[i].Substring (0, rows[i].IndexOf(Config.Default.CommentSign)).TrimEnd());
					}
					else
					{
						result.Append(rows[i]);
					}
                    result.Append(Environment.NewLine);
                }
            }
            return result.ToString().TrimEnd();
        }

        private Bitmap GetImage(string verseText)
        {
            using (Font font = new Font(Config.Default.FontName, Config.Default.FontSize, Config.Default.FontStyle, GraphicsUnit.Pixel))
            using (StringFormat stringFormat = new StringFormat())
            using (GraphicsPath graphicsPath = new GraphicsPath())
            using (Pen pen = new Pen(Config.Default.FontBorderColor, Config.Default.FontBorderSize))
            {
                SetStringFormat(stringFormat);

                int x = Config.Default.Padding;
                int y = Config.Default.Padding;
                int width = Config.Default.HorizontalResolution - Config.Default.Padding * 2;
                int height = Config.Default.VerticalResolution - Config.Default.Padding * 2; ;
                Rectangle rectangle = new Rectangle(x, y, width, height);

                graphicsPath.AddString(
                    verseText,                  // text to draw
                    font.FontFamily,            // or any other font family
                    (int)font.Style,            // font style (bold, italic, etc.)
                    font.Size,                  // em size
                    rectangle,                  // a rectangle where the text is drawn in
                    stringFormat);              // set options here (e.g. center alignment)

                Type t = typeof(Brushes);
                Brush brush = (Brush)t.GetProperty(Config.Default.FontColor).GetValue(null, null);

                Bitmap bmp = new Bitmap(Config.Default.HorizontalResolution, Config.Default.VerticalResolution);

                //http://stackoverflow.com/questions/4200843/outline-text-with-system-drawing
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.High;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawPath(pen, graphicsPath);
                g.FillPath(brush, graphicsPath);
                g.Flush();
                g.Dispose();

                return bmp;
            }
        }

        private StringFormat SetStringFormat(StringFormat stringFormat)
        {
            switch (Config.Default.VerticalAlignment)
            {
                case "Top":
                    stringFormat.LineAlignment = StringAlignment.Near;
                    break;
                case "Center":
                    stringFormat.LineAlignment = StringAlignment.Center;
                    break;
                default:
                    stringFormat.LineAlignment = StringAlignment.Far;
                    break;
            }

            switch (Config.Default.HorizontalAlignment)
            {
                case "Left":
                    stringFormat.Alignment = StringAlignment.Near;
                    break;
                case "Right":
                    stringFormat.Alignment = StringAlignment.Far;
                    break;
                default:
                    stringFormat.Alignment = StringAlignment.Center;
                    break;
            }

            return stringFormat;
        }

    }
}
