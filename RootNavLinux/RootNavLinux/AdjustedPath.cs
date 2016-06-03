using System;
using System.Windows;
using System.Collections.Generic;
using RootNav.Core.LiveWires;

namespace RootNavLinux
{
	public class AdjustedPath
	{
		private int sourceIndex = 0;

		public int SourceIndex
		{
			get { return sourceIndex; }
			set { sourceIndex = value; }
		}

		public TerminalType TypeSource;

		private List<Point> intermediatePoints = new List<Point>();

		public List<Point> IntermediatePoints
		{
			get { return intermediatePoints; }
			set { intermediatePoints = value; }
		}

		public Point StartPoint 
		{
			get;
			set;
		}

		public AdjustedPath ()
		{
			
		}
	}
}

