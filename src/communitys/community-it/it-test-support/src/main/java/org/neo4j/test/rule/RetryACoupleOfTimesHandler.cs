using System;

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
namespace Neo4Net.Test.rule
{

	using TransientFailureException = Neo4Net.Graphdb.TransientFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Retries a transaction a couple of times, with a delay in between.
	/// </summary>
	public class RetryACoupleOfTimesHandler : RetryHandler
	{
		 private readonly System.Predicate<Exception> _retriable;
		 private readonly int _maxRetryCount;
		 private readonly long _timeBetweenTries;
		 private readonly TimeUnit _unit;
		 private int _retries;

		 public RetryACoupleOfTimesHandler( System.Predicate<Exception> retriable ) : this( retriable, 5, 1, TimeUnit.SECONDS )
		 {
		 }

		 public RetryACoupleOfTimesHandler( System.Predicate<Exception> retriable, int maxRetryCount, long timeBetweenTries, TimeUnit unit )
		 {
			  this._retriable = retriable;
			  this._maxRetryCount = maxRetryCount;
			  this._timeBetweenTries = timeBetweenTries;
			  this._unit = unit;
		 }

		 public override bool RetryOn( Exception t )
		 {
			  if ( _retriable.test( t ) )
			  {
					LockSupport.parkNanos( _unit.toNanos( _timeBetweenTries ) );
					return _retries++ < _maxRetryCount;
			  }
			  return false;
		 }

		 /// <summary>
		 /// Retries on <seealso cref="TransientFailureException"/> and any <seealso cref="System.Exception"/> implementing
		 /// <seealso cref="org.neo4j.kernel.api.exceptions.Status.HasStatus"/> with
		 /// <seealso cref="org.neo4j.kernel.api.exceptions.Status.Classification.TransientError"/> classification.
		 /// a configurable number of times and with a configurable delay between retries.
		 /// </summary>
		 public static readonly System.Predicate<Exception> TransientErrors = t =>
		 {
		  if ( t is TransientFailureException )
		  {
				return true;
		  }
		  if ( t is Status.HasStatus )
		  {
				Status status = ( ( Status.HasStatus ) t ).status();
				if ( status.code().classification() == Status.Classification.TransientError )
				{
					 return true;
				}
		  }
		  return false;
		 };

		 /// <summary>
		 /// Retries on any <seealso cref="System.Exception"/>, i.e. not <seealso cref="System.OutOfMemoryException"/> or similar.
		 /// </summary>
		 public static readonly System.Predicate<Exception> AnyException = t =>
		 {
		  return t is Exception; // i.e. excluding OutOfMemory and more sever errors.
		 };

		 public static RetryHandler RetryACoupleOfTimesOn( System.Predicate<Exception> retriable )
		 {
			  return new RetryACoupleOfTimesHandler( retriable );
		 }

		 public static RetryHandler RetryACoupleOfTimesOn( System.Predicate<Exception> retriable, int maxRetryCount, long timeBetweenTries, TimeUnit unit )
		 {
			  return new RetryACoupleOfTimesHandler( retriable, maxRetryCount, timeBetweenTries, unit );
		 }
	}

}