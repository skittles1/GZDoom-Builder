﻿//Downloaded from
//Visual C# Kicks - http://vckicks.110mb.com
//The Code Project - http://www.codeproject.com

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.GZBuilder.Controls
{
	public partial class AngleControl : UserControl
	{
		private int angle;

		private Rectangle drawRegion;
		private Point origin;

		//UI colors
		private readonly Color fillColor = Color.FromArgb(90, 255, 255, 255);
		private readonly Color fillInactiveColor = SystemColors.InactiveCaption;
		private readonly Color outlineColor = Color.FromArgb(86, 103, 141);
		private readonly Color outlineInactiveColor = SystemColors.InactiveBorder;

		public AngleControl()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
		}

		private void AngleSelector_Load(object sender, EventArgs e)
		{
			setDrawRegion();
		}

		private void AngleSelector_SizeChanged(object sender, EventArgs e)
		{
			this.Height = this.Width; //Keep it a square
			setDrawRegion();
		}

		private void setDrawRegion()
		{
			drawRegion = new Rectangle(0, 0, this.Width, this.Height);
			drawRegion.X += 2;
			drawRegion.Y += 2;
			drawRegion.Width -= 4;
			drawRegion.Height -= 4;

			int offset = 2;
			origin = new Point(drawRegion.Width / 2 + offset, drawRegion.Height / 2 + offset);

			this.Refresh();
		}

		public int Angle
		{
			get { return angle; }
			set
			{
				angle = value;
				this.Refresh();
			}
		}

		public delegate void AngleChangedDelegate();
		public event AngleChangedDelegate AngleChanged;

		private PointF DegreesToXY(float degrees, float radius, Point origin)
		{
			PointF xy = new PointF();
			double radians = degrees * Math.PI / 180.0;

			xy.X = (float)Math.Cos(radians) * radius + origin.X;
			xy.Y = (float)Math.Sin(-radians) * radius + origin.Y;

			return xy;
		}

		private int XYToDegrees(Point xy, Point origin)
		{
			float xDiff = xy.X - origin.X;
			float yDiff = xy.Y - origin.Y;
			return (int)Math.Round(Math.Atan2(-yDiff, xDiff) * 180.0 / Math.PI);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			Pen outline;
			Pen needle;
			SolidBrush fill;
			Brush center;

			if (this.Enabled) {
				outline = new Pen(outlineColor, 2.0f);
				fill = new SolidBrush(fillColor);
				needle = Pens.Black;
				center = Brushes.Black;
			} else {
				outline = new Pen(outlineInactiveColor, 2.0f);
				fill = new SolidBrush(fillInactiveColor);
				needle = Pens.DarkGray;
				center = Brushes.DarkGray;
			}

			PointF anglePoint = DegreesToXY(angle, origin.X - 2, origin);
			Rectangle originSquare = new Rectangle(origin.X - 1, origin.Y - 1, 3, 3);

			//Draw
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.DrawEllipse(outline, drawRegion);
			g.FillEllipse(fill, drawRegion);
			g.DrawLine(needle, origin, anglePoint);

			g.SmoothingMode = SmoothingMode.HighSpeed; //Make the square edges sharp
			g.FillRectangle(center, originSquare);

			fill.Dispose();
			outline.Dispose();

			base.OnPaint(e);
		}

		private void AngleSelector_MouseDown(object sender, MouseEventArgs e) {
			int thisAngle = XYToDegrees(new Point(e.X, e.Y), origin);

			if (e.Button == MouseButtons.Left) {
				thisAngle = (int)Math.Round(thisAngle / 45f) * 45;
			}

			if(thisAngle != this.Angle) {
				this.Angle = thisAngle;
				if(!this.DesignMode && AngleChanged != null) AngleChanged(); //Raise event
				this.Refresh();
			}
		}

		private void AngleSelector_MouseMove(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) {
				int thisAngle = XYToDegrees(new Point(e.X, e.Y), origin);

				if(e.Button == MouseButtons.Left) {
					thisAngle = (int)Math.Round(thisAngle / 45f) * 45;
				}

				if(thisAngle != this.Angle) {
					this.Angle = thisAngle;
					if(!this.DesignMode && AngleChanged != null) AngleChanged(); //Raise event
					this.Refresh();
				}
			}
		}
	}
}