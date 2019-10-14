using System;
using System.IO;
using System.Threading;

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
namespace Neo4Net.Test
{

	/// <summary>
	/// A simple Runnable that is meant to consume the output and error streams of a
	/// detached process, for debugging purposes.
	/// </summary>
	public class StreamConsumer : ThreadStart
	{
		 public interface StreamExceptionHandler
		 {
			  void Handle( IOException failure );
		 }

		 public static readonly StreamExceptionHandler PrintFailures = Exception.printStackTrace;

		 public static readonly StreamExceptionHandler IgnoreFailures = failure =>
		 {
		 };

		 private readonly StreamReader @in;
		 private readonly Writer @out;

		 private readonly bool _quiet;
		 private readonly string _prefix;

		 private readonly StreamExceptionHandler _failureHandler;
		 private readonly Exception _stackTraceOfOrigin;

		 public StreamConsumer( Stream @in, Stream @out, bool quiet ) : this( @in, @out, quiet, "", quiet ? IgnoreFailures : PrintFailures )
		 {
		 }

		 public StreamConsumer( Stream @in, Stream @out, bool quiet, string prefix, StreamExceptionHandler failureHandler )
		 {
			  this._quiet = quiet;
			  this._prefix = prefix;
			  this._failureHandler = failureHandler;
			  this.@in = new StreamReader( @in );
			  this.@out = new StreamWriter( @out );
			  this._stackTraceOfOrigin = new Exception( "Stack trace of thread that created this StreamConsumer" );
		 }

		 public override void Run()
		 {
			  try
			  {
					string line;
					while ( !string.ReferenceEquals( ( line = @in.ReadLine() ), null ) )
					{
						 if ( !_quiet )
						 {
							  @out.write( _prefix + line + "\n" );
							  @out.flush();
						 }
					}
			  }
			  catch ( IOException exc )
			  {
					exc.addSuppressed( _stackTraceOfOrigin );
					_failureHandler.handle( exc );
			  }
		 }
	}

}