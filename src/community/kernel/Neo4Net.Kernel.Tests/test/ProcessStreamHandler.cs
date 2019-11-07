using System;
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

	using StreamExceptionHandler = Neo4Net.Test.StreamConsumer.StreamExceptionHandler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.StreamConsumer.IGNORE_FAILURES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.StreamConsumer.PRINT_FAILURES;

	/// <summary>
	/// Having trouble with your <seealso cref="Process"/>'s output and error streams?
	/// Are they getting filled up and your main thread hangs? Fear no more. Use this
	/// class to bundle everything up in one nifty little object that handles all
	/// your needs. Use the {@code ProcessStreamHandler#launch()} method to start
	/// the consumer threads and once you are {@code ProcessStreamHandler#done()}
	/// just say so.
	/// </summary>
	public class ProcessStreamHandler
	{
		 private readonly Thread @out;
		 private readonly Thread _err;
		 private readonly Process _process;

		 /// <summary>
		 /// Convenience constructor assuming the local output streams are
		 /// <seealso cref="System.out"/> and <seealso cref="System.err"/> for the process's OutputStream
		 /// and ErrorStream respectively.
		 /// <para>
		 /// Set quiet to true if you just want to consume the output to avoid locking up the process.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="process"> The process whose output to consume. </param>
		 public ProcessStreamHandler( Process process, bool quiet ) : this( process, quiet, "", quiet ? IGNORE_FAILURES : PRINT_FAILURES )
		 {
		 }

		 public ProcessStreamHandler( Process process, bool quiet, string prefix, StreamExceptionHandler failureHandler ) : this( process, quiet, prefix, failureHandler, System.out, System.err )
		 {
		 }

		 public ProcessStreamHandler( Process process, bool quiet, string prefix, StreamExceptionHandler failureHandler, PrintStream @out, PrintStream err )
		 {
			  this._process = process;

			  this.@out = new Thread( new StreamConsumer( process.InputStream, @out, quiet, prefix, failureHandler ) );
			  this._err = new Thread( new StreamConsumer( process.ErrorStream, err, quiet, prefix, failureHandler ) );
		 }

		 /// <summary>
		 /// Starts the consumer threads. Calls <seealso cref="Thread.start()"/>.
		 /// </summary>
		 public virtual void Launch()
		 {
			  @out.Start();
			  _err.Start();
		 }

		 /// <summary>
		 /// Joins with the consumer Threads. Calls <seealso cref="Thread.join()"/> on the two
		 /// consumers.
		 /// </summary>
		 public virtual void Done()
		 {
			  if ( _process.Alive )
			  {
					_process.destroyForcibly();
			  }
			  try
			  {
					@out.Join();
			  }
			  catch ( InterruptedException e )
			  {
					Thread.interrupted();
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
			  try
			  {
					_err.Join();
			  }
			  catch ( InterruptedException e )
			  {
					Thread.interrupted();
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
		 }

		 public virtual void Cancel()
		 {
			  @out.Interrupt();

			  _err.Interrupt();
		 }

		 public virtual int WaitForResult()
		 {
			  Launch();
			  try
			  {
					return _process.waitFor();
			  }
			  catch ( InterruptedException )
			  {
					Thread.interrupted();
					return 0;
			  }
			  finally
			  {
					Done();
			  }
		 }
	}

}