using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{

	using Result = Neo4Net.Graphdb.Result;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using Neo4Net.Test;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	internal class ThreadedTransaction<S>
	{
		 private volatile Future<Exception> _done;
		 private readonly NeoInteractionLevel<S> _neo;
		 private readonly DoubleLatch _latch;

		 internal ThreadedTransaction( NeoInteractionLevel<S> neo, DoubleLatch latch )
		 {
			  this._neo = neo;
			  this._latch = latch;
		 }

		 internal virtual string ExecuteCreateNode( ThreadingRule threading, S subject )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String query = "CREATE (:Test { name: '" + neo.nameOf(subject) + "-node'})";
			  string query = "CREATE (:Test { name: '" + _neo.nameOf( subject ) + "-node'})";
			  return Execute( threading, subject, query );
		 }

		 internal virtual string Execute( ThreadingRule threading, S subject, string query )
		 {
			  return DoExecute( threading, subject, KernelTransaction.Type.@explicit, false, query )[0];
		 }

		 internal virtual string[] Execute( ThreadingRule threading, S subject, params string[] queries )
		 {
			  return DoExecute( threading, subject, KernelTransaction.Type.@explicit, false, queries );
		 }

		 internal virtual string ExecuteEarly( ThreadingRule threading, S subject, KernelTransaction.Type txType, string query )
		 {
			  return DoExecute( threading, subject, txType, true, query )[0];
		 }

		 internal virtual string[] ExecuteEarly( ThreadingRule threading, S subject, KernelTransaction.Type txType, params string[] queries )
		 {
			  return DoExecute( threading, subject, txType, true, queries );
		 }

		 private string[] DoExecute( ThreadingRule threading, S subject, KernelTransaction.Type txType, bool startEarly, params string[] queries )
		 {
			  NamedFunction<S, Exception> startTransaction = new NamedFunctionAnonymousInnerClass( this, subject, txType, startEarly, queries );

			  _done = threading.Execute( startTransaction, subject );
			  return queries;
		 }

		 private class NamedFunctionAnonymousInnerClass : NamedFunction<S, Exception>
		 {
			 private readonly ThreadedTransaction<S> _outerInstance;

			 private S _subject;
			 private KernelTransaction.Type _txType;
			 private bool _startEarly;
			 private string[] _queries;

			 public NamedFunctionAnonymousInnerClass( ThreadedTransaction<S> outerInstance, S subject, KernelTransaction.Type txType, bool startEarly, string[] queries ) : base( "threaded-transaction-" + Arrays.GetHashCode( queries ) )
			 {
				 this.outerInstance = outerInstance;
				 this._subject = subject;
				 this._txType = txType;
				 this._startEarly = startEarly;
				 this._queries = queries;
			 }

			 public override Exception apply( S subject )
			 {
				  try
				  {
						  using ( InternalTransaction tx = _outerInstance.neo.beginLocalTransactionAsUser( subject, _txType ) )
						  {
							Result result = null;
							try
							{
								 if ( _startEarly )
								 {
									  _outerInstance.latch.start();
								 }
								 foreach ( string query in _queries )
								 {
									  if ( result != null )
									  {
											result.Close();
									  }
									  result = _outerInstance.neo.LocalGraph.execute( query );
								 }
								 if ( !_startEarly )
								 {
									  _outerInstance.latch.startAndWaitForAllToStart();
								 }
							}
							finally
							{
								 if ( !_startEarly )
								 {
									  _outerInstance.latch.start();
								 }
								 _outerInstance.latch.finishAndWaitForAllToFinish();
							}
							result.Close();
							tx.Success();
							return null;
						  }
				  }
				  catch ( Exception t )
				  {
						return t;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void closeAndAssertSuccess() throws Throwable
		 internal virtual void CloseAndAssertSuccess()
		 {
			  Exception exceptionInOtherThread = Join();
			  if ( exceptionInOtherThread != null )
			  {
					throw new AssertionError( "Expected no exception in ThreadCreate, but got one.", exceptionInOtherThread );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void closeAndAssertExplicitTermination() throws Throwable
		 internal virtual void CloseAndAssertExplicitTermination()
		 {
			  Exception exceptionInOtherThread = Join();
			  if ( exceptionInOtherThread == null )
			  {
					fail( "Expected explicit TransactionTerminatedException in the threaded transaction, " + "but no exception was raised" );
			  }
			  assertThat( Exceptions.stringify( exceptionInOtherThread ), exceptionInOtherThread.Message, containsString( "Explicitly terminated by the user." ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void closeAndAssertSomeTermination() throws Throwable
		 internal virtual void CloseAndAssertSomeTermination()
		 {
			  Exception exceptionInOtherThread = Join();
			  if ( exceptionInOtherThread == null )
			  {
					fail( "Expected a TransactionTerminatedException in the threaded transaction, but no exception was raised" );
			  }
			  assertThat( Exceptions.stringify( exceptionInOtherThread ), exceptionInOtherThread, instanceOf( typeof( TransactionTerminatedException ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Throwable join() throws java.util.concurrent.ExecutionException, InterruptedException
		 private Exception Join()
		 {
			  return _done.get();
		 }
	}

}