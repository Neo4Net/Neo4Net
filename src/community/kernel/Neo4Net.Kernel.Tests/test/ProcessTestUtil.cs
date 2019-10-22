using System;
using System.Collections.Generic;

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

	using Neo4Net.Helpers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getClassPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getJavaExecutable;

	public class ProcessTestUtil
	{
		 private ProcessTestUtil()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void executeSubProcess(Class mainClass, long timeout, java.util.concurrent.TimeUnit unit, String... arguments) throws Exception
		 public static void ExecuteSubProcess( Type mainClass, long timeout, TimeUnit unit, params string[] arguments )
		 {
			  Future<int> future = StartSubProcess( mainClass, arguments );
			  int result = future.get( timeout, unit );
			  if ( result != 0 )
			  {
					throw new Exception( "Process for " + mainClass + " with arguments " + Arrays.ToString( arguments ) + " failed, returned exit value " + result );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.concurrent.Future<int> startSubProcess(Class mainClass, String... arguments) throws java.io.IOException
		 public static Future<int> StartSubProcess( Type mainClass, params string[] arguments )
		 {
			  IList<string> args = new List<string>();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  ( ( IList<string> )args ).AddRange( asList( JavaExecutable.ToString(), "-cp", ClassPath, mainClass.FullName ) );
			  ( ( IList<string> )args ).AddRange( asList( arguments ) );
			  Process process = Runtime.Runtime.exec( args.ToArray() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ProcessStreamHandler processOutput = new ProcessStreamHandler(process, false);
			  ProcessStreamHandler processOutput = new ProcessStreamHandler( process, false );
			  processOutput.Launch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Future<int> realFuture = org.Neo4Net.helpers.FutureAdapter.processFuture(process);
			  Future<int> realFuture = FutureAdapter.processFuture( process );
			  return new FutureAnonymousInnerClass( processOutput, realFuture );
		 }

		 private class FutureAnonymousInnerClass : Future<int>
		 {
			 private Neo4Net.Test.ProcessStreamHandler _processOutput;
			 private Future<int> _realFuture;

			 public FutureAnonymousInnerClass( Neo4Net.Test.ProcessStreamHandler processOutput, Future<int> realFuture )
			 {
				 this._processOutput = processOutput;
				 this._realFuture = realFuture;
			 }

			 public override bool cancel( bool mayInterruptIfRunning )
			 {
				  try
				  {
						return _realFuture.cancel( mayInterruptIfRunning );
				  }
				  finally
				  {
						_processOutput.done();
				  }
			 }

			 public override bool Cancelled
			 {
				 get
				 {
					  return _realFuture.Cancelled;
				 }
			 }

			 public override bool Done
			 {
				 get
				 {
					  return _realFuture.Done;
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<int> get() throws InterruptedException, java.util.concurrent.ExecutionException
			 public override int? get()
			 {
				  try
				  {
						return _realFuture.get();
				  }
				  finally
				  {
						_processOutput.done();
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<int> get(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
			 public override int? get( long timeout, TimeUnit unit )
			 {
				  try
				  {
						return _realFuture.get( timeout, unit );
				  }
				  finally
				  {
						_processOutput.done();
				  }
			 }
		 }
	}

}