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
namespace Org.Neo4j.Diagnostics
{

	/// <summary>
	/// Tracks progress in an interactive way, relies on the fact that the {@code PrintStream} echoes to a terminal that can
	/// interpret the carrier return to reset the current line.
	/// </summary>
	public class InteractiveProgress : DiagnosticsReporterProgress
	{
		 private string _prefix;
		 private string _suffix;
		 private string _totalSteps = "?";
		 private readonly PrintStream @out;
		 private readonly bool _verbose;
		 private string _info = "";
		 private int _longestInfo;

		 public InteractiveProgress( PrintStream @out, bool verbose )
		 {
			  this.@out = @out;
			  this._verbose = verbose;
		 }

		 public override void PercentChanged( int percent )
		 {
			  @out.print( string.Format( "\r{0,8} [", _prefix ) );
			  int totalWidth = 20;

			  int numBars = totalWidth * percent / 100;
			  for ( int i = 0; i < totalWidth; i++ )
			  {
					if ( i < numBars )
					{
						 @out.print( '#' );
					}
					else
					{
						 @out.print( ' ' );
					}
			  }
			  @out.print( string.Format( "] {0,3}%   {1} {2}", percent, _suffix, _info ) );
		 }

		 public override void Started( long currentStepIndex, string target )
		 {
			  this._prefix = currentStepIndex + "/" + _totalSteps;
			  this._suffix = target;
			  PercentChanged( 0 );
		 }

		 public override void Finished()
		 {
			  // Pad string to erase info string
			  _info = string.join( "", Collections.nCopies( _longestInfo, " " ) );

			  PercentChanged( 100 );
			  @out.println();
		 }

		 public override void Info( string info )
		 {
			  this._info = info;
			  if ( info.Length > _longestInfo )
			  {
					_longestInfo = info.Length;
			  }
		 }

		 public override void Error( string msg, Exception throwable )
		 {
			  @out.println();
			  @out.println( "Error: " + msg );
			  if ( _verbose )
			  {
					throwable.printStackTrace( @out );
			  }
		 }

		 public virtual long TotalSteps
		 {
			 set
			 {
				  this._totalSteps = value.ToString();
			 }
		 }
	}

}