using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.helper
{

	public class ErrorHandler : IDisposable
	{
		 private readonly IList<Exception> _throwables = new List<Exception>();
		 private readonly string _message;

		 /// <summary>
		 /// Ensures each action is executed. Any throwables will be saved and thrown after all actions have been executed. The first caught throwable will be cause
		 /// and any other will be added as suppressed.
		 /// </summary>
		 /// <param name="description"> The exception message if any are thrown. </param>
		 /// <param name="actions"> Throwing runnables to execute. </param>
		 /// <exception cref="RuntimeException"> thrown if any action throws after all have been executed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void runAll(String description, ThrowingRunnable... actions) throws RuntimeException
		 public static void RunAll( string description, params ThrowingRunnable[] actions )
		 {
			  using ( ErrorHandler errorHandler = new ErrorHandler( description ) )
			  {
					foreach ( ThrowingRunnable action in actions )
					{
						 errorHandler.Execute( action );
					}
			  }
		 }

		 public ErrorHandler( string message )
		 {
			  this._message = message;
		 }

		 public virtual void Execute( ThrowingRunnable throwingRunnable )
		 {
			  try
			  {
					throwingRunnable.Run();
			  }
			  catch ( Exception e )
			  {
					_throwables.Add( e );
			  }
		 }

		 public virtual void Add( Exception throwable )
		 {
			  _throwables.Add( throwable );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws RuntimeException
		 public override void Close()
		 {
			  ThrowIfException();
		 }

		 private void ThrowIfException()
		 {
			  if ( _throwables.Count > 0 )
			  {
					Exception runtimeException = null;
					foreach ( Exception throwable in _throwables )
					{
						 if ( runtimeException == null )
						 {
							  runtimeException = new Exception( _message, throwable );
						 }
						 else
						 {
							  runtimeException.addSuppressed( throwable );
						 }
					}
					throw runtimeException;
			  }
		 }

		 public interface ThrowingRunnable
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run() throws Throwable;
			  void Run();
		 }
	}

}