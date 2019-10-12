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
namespace Neo4Net.Diagnostics
{

	public class NonInteractiveProgress : DiagnosticsReporterProgress
	{
		 private string _totalSteps = "?";
		 private readonly PrintStream @out;
		 private readonly bool _verbose;
		 private int _lastPercentage;

		 public NonInteractiveProgress( PrintStream @out, bool verbose )
		 {
			  this.@out = @out;
			  this._verbose = verbose;
		 }

		 public override void PercentChanged( int percent )
		 {
			  for ( int i = _lastPercentage + 1; i <= percent; i++ )
			  {
					@out.print( '.' );
					if ( i % 20 == 0 )
					{
						 @out.printf( " %3d%%%n", i );
					}
			  }
			  _lastPercentage = percent;
			  @out.flush();
		 }

		 public override void Started( long currentStepIndex, string target )
		 {
			  @out.println( currentStepIndex + "/" + _totalSteps + " " + target );
			  _lastPercentage = 0;
		 }

		 public override void Finished()
		 {
			  PercentChanged( 100 );
			  @out.println();
		 }

		 public override void Info( string info )
		 {
			  // Ignore info message
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