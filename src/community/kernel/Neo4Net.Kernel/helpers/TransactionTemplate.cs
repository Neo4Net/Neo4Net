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
namespace Neo4Net.Helpers
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using TransientFailureException = Neo4Net.GraphDb.TransientFailureException;

	/// <summary>
	/// Neo4Net transaction template that automates the retry-on-exception logic. It uses the builder
	/// pattern for configuration, with copy-semantics, so you can iteratively build up instances for
	/// different scenarios.
	/// <para>
	/// First instantiate and configure the template using the fluent API methods, and then
	/// invoke execute which will begin/commit transactions in a loop for the specified number of times.
	/// </para>
	/// <para>
	/// By default all exceptions (except Errors and TransactionTerminatedException) cause a retry,
	/// and the monitor does nothing, but these can be overridden with custom behavior.
	/// A bit more narrow and typical exception to retry on is <seealso cref="TransientFailureException"/>,
	/// which aims to represent exceptions that are most likely to succeed after a retry.
	/// </para>
	/// </summary>
	public class TransactionTemplate
	{
		 public interface Monitor
		 {
			  /// <summary>
			  /// Called when an exception occur from the transactions.
			  /// </summary>
			  /// <param name="ex"> the exception thrown. </param>
			  void Failure( Exception ex );

			  /// <summary>
			  /// Called when the whole retry logic fails. Can be that all retries have failed or that the executing thread was
			  /// interrupted.
			  /// </summary>
			  /// <param name="ex"> the last exception thrown when it failed. </param>
			  void Failed( Exception ex );

			  /// <summary>
			  /// Called when a retry is done.
			  /// </summary>
			  void Retrying();
		 }

		  public class Monitor_Adapter : Monitor
		  {
			  private readonly TransactionTemplate _outerInstance;

			  public Monitor_Adapter( TransactionTemplate outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override void Failure( Exception ex )
				{
				}

				public override void Failed( Exception ex )
				{
				}

				public override void Retrying()
				{
				}
		  }

		 private readonly IGraphDatabaseService _gds;
		 private readonly Monitor _monitor;
		 private readonly int _retries;
		 private readonly long _backoff;
		 private readonly System.Predicate<Exception> _retryPredicate;

		 /// <summary>
		 /// Creates a template for performing transactions with retry logic.
		 /// <para>
		 /// Default exceptions to retry on is everything except <seealso cref="System.Exception"/> and <seealso cref="TransactionTerminatedException"/>.
		 /// </para>
		 /// </summary>
		 public TransactionTemplate() : this((GraphDatabaseService) Proxy.newProxyInstance(typeof(GraphDatabaseService).ClassLoader, new Type[]{ typeof(GraphDatabaseService) }, (proxy, method, args) ->
		 {
					 {
						  throw new System.ArgumentException( "You need to call 'with(GraphDatabaseService)' on the template in order to use it" );
					 }), new Monitor.Adapter(), 0, 0, ex => !typeof(Exception).IsInstanceOfType(ex) && !typeof(TransactionTerminatedException).IsInstanceOfType(ex));
		 }

		 /// <summary>
		 /// Create a template for performing transaction with retry logic.
		 /// </summary>
		 /// <param name="gds"> graph database to execute on. </param>
		 /// <param name="monitor"> a monitor that can react to events. </param>
		 /// <param name="retries"> number of retries to try before failing. </param>
		 /// <param name="backoff"> milliseconds to wait between each retry. </param>
		 /// <param name="retryPredicate"> what <seealso cref="System.Exception"/>'s to retry on. </param>
		 public TransactionTemplate( IGraphDatabaseService gds, Monitor monitor, int retries, long backoff, System.Predicate<Exception> retryPredicate )
		 {
			  Objects.requireNonNull( gds );
			  Objects.requireNonNull( monitor );
			  if ( retries < 0 )
			  {
					throw new System.ArgumentException( "Number of retries must be greater than or equal to 0" );
			  }
			  if ( backoff < 0 )
			  {
					throw new System.ArgumentException( "Backoff time must be a positive number" );
			  }
			  Objects.requireNonNull( retryPredicate );

			  this._gds = gds;
			  this._monitor = monitor;
			  this._retries = retries;
			  this._backoff = backoff;
			  this._retryPredicate = retryPredicate;
		 }

		 public virtual TransactionTemplate With( IGraphDatabaseService gds )
		 {
			  return new TransactionTemplate( gds, _monitor, _retries, _backoff, _retryPredicate );
		 }

		 public virtual TransactionTemplate Retries( int retries )
		 {
			  return new TransactionTemplate( _gds, _monitor, retries, _backoff, _retryPredicate );
		 }

		 public virtual TransactionTemplate Backoff( long backoff, TimeUnit unit )
		 {
			  return new TransactionTemplate( _gds, _monitor, _retries, unit.toMillis( backoff ), _retryPredicate );
		 }

		 public virtual TransactionTemplate Monitor( Monitor monitor )
		 {
			  return new TransactionTemplate( _gds, monitor, _retries, _backoff, _retryPredicate );
		 }

		 public virtual TransactionTemplate RetryOn( System.Predicate<Exception> retryPredicate )
		 {
			  return new TransactionTemplate( _gds, _monitor, _retries, _backoff, retryPredicate );
		 }

		 /// <summary>
		 /// Executes a transaction with retry logic.
		 /// </summary>
		 /// <param name="txConsumer"> a consumer that takes transactions. </param>
		 /// <exception cref="TransactionFailureException"> if an error occurs other than those specified in <seealso cref="retryOn(Predicate)"/>
		 /// or if the retry count was exceeded. </exception>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void execute(final System.Action<org.Neo4Net.graphdb.Transaction> txConsumer)
		 public virtual void Execute( System.Action<Transaction> txConsumer )
		 {
			  Execute(transaction =>
			  {
				txConsumer( transaction );
				return null;
			  });
		 }

		 /// <summary>
		 /// Executes a transaction with retry logic returning a result.
		 /// </summary>
		 /// <param name="txFunction"> function taking a transaction and producing a result. </param>
		 /// @param <T> type of the result. </param>
		 /// <returns> the result with type {@code T}. </returns>
		 /// <exception cref="TransactionFailureException"> if an error occurs other than those specified in <seealso cref="retryOn(Predicate)"/>
		 /// or if the retry count was exceeded. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> T execute(System.Func<org.Neo4Net.graphdb.Transaction, T> txFunction) throws org.Neo4Net.graphdb.TransactionFailureException
		 public virtual T Execute<T>( System.Func<Transaction, T> txFunction )
		 {
			  Exception txEx;
			  int retriesLeft = _retries;
			  while ( true )
			  {
					try
					{
							using ( Transaction tx = _gds.beginTx() )
							{
							 T result = txFunction( tx );
							 tx.Success();
							 return result;
							}
					}
					catch ( Exception ex )
					{
						 _monitor.failure( ex );
						 txEx = ex;

						 if ( !_retryPredicate.test( ex ) )
						 {
							  break;
						 }
					}

					try
					{
						 Thread.Sleep( _backoff );
					}
					catch ( InterruptedException e )
					{
						 TransactionFailureException interrupted = new TransactionFailureException( "Interrupted", e );
						 _monitor.failed( interrupted );
						 throw interrupted;
					}

					if ( retriesLeft == 0 )
					{
						 break;
					}
					retriesLeft--;
					_monitor.retrying();
			  }

			  _monitor.failed( txEx );
			  throw new TransactionFailureException( "Failed", txEx );
		 }
	}

}