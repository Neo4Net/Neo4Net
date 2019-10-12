using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Helpers.progress
{

	[Obsolete]
	public abstract class Indicator
	{
		 [Obsolete]
		 public static readonly Indicator NONE = new IndicatorAnonymousInnerClass();

		 private class IndicatorAnonymousInnerClass : Indicator
		 {
			 public IndicatorAnonymousInnerClass() : base(1)
			 {
			 }

			 protected internal override void progress( int from, int to )
			 {
			 }
		 }

		 private readonly int _reportResolution;

		 [Obsolete]
		 public Indicator( int reportResolution )
		 {
			  this._reportResolution = reportResolution;
		 }

		 protected internal abstract void Progress( int from, int to );

		 internal virtual int ReportResolution()
		 {
			  return _reportResolution;
		 }

		 [Obsolete]
		 public virtual void StartProcess( long totalCount )
		 {
		 }

		 [Obsolete]
		 public virtual void StartPart( string part, long totalCount )
		 {
		 }

		 [Obsolete]
		 public virtual void CompletePart( string part )
		 {
		 }

		 [Obsolete]
		 public virtual void CompleteProcess()
		 {
		 }

		 [Obsolete]
		 public virtual void Failure( Exception cause )
		 {
		 }

		 internal class Textual : Indicator
		 {
			  internal readonly string Process;
			  internal readonly PrintWriter Out;

			  internal Textual( string process, PrintWriter @out ) : base( 200 )
			  {
					this.Process = process;
					this.Out = @out;
			  }

			  public override void StartProcess( long totalCount )
			  {
					Out.println( Process );
					Out.flush();
			  }

			  protected internal override void Progress( int from, int to )
			  {
					for ( int i = from; i < to; )
					{
						 PrintProgress( ++i );
					}
					Out.flush();
			  }

			  public override void Failure( Exception cause )
			  {
					cause.printStackTrace( Out );
			  }

			  internal virtual void PrintProgress( int progress )
			  {
					Out.print( '.' );
					if ( progress % 20 == 0 )
					{
						 Out.printf( " %3d%%%n", progress / 2 );
					}
			  }
		 }
	}

}