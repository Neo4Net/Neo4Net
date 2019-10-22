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
namespace Neo4Net.Helpers
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class TransactionTemplateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.EmbeddedDatabaseRule databaseRule = new org.Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DatabaseRule = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Expected = ExpectedException.none();

		 private TransactionTemplate _template;
		 private CountingMonitor _monitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _monitor = new CountingMonitor();
			  _template = ( new TransactionTemplate() ).With(DatabaseRule.GraphDatabaseAPI).monitor(_monitor).retries(5).backoff(3, TimeUnit.MILLISECONDS);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForceUserToCallWith()
		 public virtual void ShouldForceUserToCallWith()
		 {
			  Expected.expectCause( allOf( instanceOf( typeof( System.ArgumentException ) ), hasProperty( "message", @is( "You need to call 'with(GraphDatabaseService)' on the template in order to use it" ) ) ) );
			  TransactionTemplate transactionTemplate = new TransactionTemplate();
			  transactionTemplate.Execute( transaction => null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateGraphDatabaseService()
		 public virtual void ValidateGraphDatabaseService()
		 {
			  Expected.expect( typeof( System.NullReferenceException ) );
			  _template.with( null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateRetires()
		 public virtual void ValidateRetires()
		 {
			  Expected.expect( typeof( System.ArgumentException ) );
			  Expected.expectMessage( "Number of retries must be greater than or equal to 0" );
			  _template.retries( -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateBackoff()
		 public virtual void ValidateBackoff()
		 {
			  Expected.expect( typeof( System.ArgumentException ) );
			  Expected.expectMessage( "Backoff time must be a positive number" );
			  _template.backoff( -10, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateMonitor()
		 public virtual void ValidateMonitor()
		 {
			  Expected.expect( typeof( System.NullReferenceException ) );
			  _template.monitor( null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateRetryOn()
		 public virtual void ValidateRetryOn()
		 {
			  Expected.expect( typeof( System.NullReferenceException ) );
			  _template.retryOn( null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetryOnError()
		 public virtual void ShouldRetryOnError()
		 {
			  System.ArgumentException ex = new System.ArgumentException();
			  _template.execute( new FailingRetryConsumer( 3, ex ) );

			  assertThat( _monitor.numRetry, @is( 3 ) );
			  assertThat( _monitor.failures, contains( ex, ex, ex ) );
			  assertThat( _monitor.fails, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfAllRetiresFail()
		 public virtual void ShouldFailIfAllRetiresFail()
		 {
			  System.ArgumentException ex = new System.ArgumentException();
			  try
			  {
					_template.execute( new FailingRetryConsumer( 10, ex ) );
			  }
			  catch ( TransactionFailureException )
			  {
			  }

			  assertThat( _monitor.numRetry, @is( 5 ) );
			  assertThat( _monitor.failures, contains( ex, ex, ex, ex, ex, ex ) ); // 5 retires results in 6 total failures
			  assertThat( _monitor.fails, contains( ex ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultExceptionsForExit()
		 public virtual void DefaultExceptionsForExit()
		 {
			  Exception error = new Exception();
			  TransactionTerminatedException terminatedException = new TransactionTerminatedException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );

			  try
			  {
					_template.execute((System.Action<Transaction>) tx =>
					{
					 throw error;
					});
			  }
			  catch ( TransactionFailureException )
			  {
					// Expected
			  }

			  try
			  {
					_template.execute((System.Action<Transaction>) tx =>
					{
					 throw terminatedException;
					});
			  }
			  catch ( TransactionFailureException )
			  {
			  }

			  assertThat( _monitor.numRetry, @is( 0 ) );
			  assertThat( _monitor.failures, contains( error, terminatedException ) );
			  assertThat( _monitor.fails, contains( error, terminatedException ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void overrideRetryExceptions()
		 public virtual void OverrideRetryExceptions()
		 {
			  _template = _template.retryOn( e => !typeof( System.ArgumentException ).IsInstanceOfType( e ) );
			  System.ArgumentException e = new System.ArgumentException();
			  try
			  {
					_template.execute((System.Action<Transaction>) tx =>
					{
					 throw e;
					});
			  }
			  catch ( TransactionFailureException )
			  {
			  }

			  assertThat( _monitor.numRetry, @is( 0 ) );
			  assertThat( _monitor.failures, contains( e ) );
			  assertThat( _monitor.fails, contains( e ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void overrideRetryShouldOverrideDefaults()
		 public virtual void OverrideRetryShouldOverrideDefaults()
		 {
			  _template = _template.retryOn( e => !typeof( System.ArgumentException ).IsInstanceOfType( e ) );

			  TransactionTerminatedException fakeException = new TransactionTerminatedException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  _template.execute( new FailingRetryConsumer( 1, fakeException ) );

			  assertThat( _monitor.numRetry, @is( 1 ) );
			  assertThat( _monitor.failures, contains( fakeException ) );
			  assertThat( _monitor.fails, empty() );
		 }

		 private class FailingRetryConsumer : System.Action<Transaction>
		 {
			  internal readonly int SuccessAfter;
			  internal readonly Exception FakeException;
			  internal int Tries;

			  internal FailingRetryConsumer( int successAfter, Exception fakeException )
			  {
					this.SuccessAfter = successAfter;
					this.FakeException = fakeException;
			  }

			  public override void Accept( Transaction transaction )
			  {
					if ( Tries++ < SuccessAfter )
					{
						 throw FakeException;
					}
			  }
		 }

		 private class CountingMonitor : TransactionTemplate.Monitor
		 {
			  internal int NumRetry;
			  internal IList<Exception> Fails = new List<Exception>();
			  internal IList<Exception> Failures = new List<Exception>();

			  public override void Failure( Exception ex )
			  {
					Failures.Add( ex );
			  }

			  public override void Failed( Exception ex )
			  {
					Fails.Add( ex );
			  }

			  public override void Retrying()
			  {
					NumRetry++;
			  }
		 }
	}

}