using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Helpers
{

	/// @deprecated Exception is not in use anymore 
	[Obsolete("Exception is not in use anymore")]
	public class ProcessFailureException : Exception
	{
		 [Serializable]
		 public sealed class Entry
		 {
			  internal readonly string Part;
			  internal readonly Exception Failure;

			  public Entry( string part, Exception failure )
			  {
					this.Part = part;
					this.Failure = failure;
			  }

			  public override string ToString()
			  {
					return "In '" + Part + "': " + Failure;
			  }
		 }

		 private readonly Entry[] _causes;

		 public ProcessFailureException( IList<Entry> causes ) : base( "Monitored process failed" + Message( causes ), Cause( causes ) )
		 {
			  this._causes = causes.ToArray();
		 }

		 private static string Message( IList<Entry> causes )
		 {
			  if ( causes.Count == 0 )
			  {
					return ".";
			  }
			  if ( causes.Count == 1 )
			  {
					return " in '" + causes[0].part + "'.";
			  }
			  StringBuilder result = new StringBuilder( ":" );
			  foreach ( Entry entry in causes )
			  {
					result.Append( "\n\t" ).Append( entry );
			  }
			  return result.ToString();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ThrowableResultOfMethodCallIgnored") private static Throwable cause(java.util.List<Entry> causes)
		 private static Exception Cause( IList<Entry> causes )
		 {
			  return causes.Count >= 1 ? causes[0].failure : null;
		 }

		 public override void PrintStackTrace( PrintStream s )
		 {
			  base.PrintStackTrace( s );
			  PrintAllCauses( new PrintWriter( s, true ) );
		 }

		 public override void PrintStackTrace( PrintWriter s )
		 {
			  base.PrintStackTrace( s );
			  PrintAllCauses( s );
		 }

		 public virtual void PrintAllCauses( PrintWriter writer )
		 {
			  if ( Cause == null )
			  {
					foreach ( Entry entry in _causes )
					{
						 entry.Failure.printStackTrace( writer );
					}
					writer.flush();
			  }
		 }
	}

}