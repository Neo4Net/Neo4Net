using System;
using System.Threading;

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
namespace Neo4Net.Test
{

	public class DoubleLatch
	{
		 private static readonly long _fiveMinutes = TimeUnit.MINUTES.toMillis( 5 );
		 private readonly System.Threading.CountdownEvent _startSignal;
		 private readonly System.Threading.CountdownEvent _finishSignal;
		 private readonly bool _awaitUninterruptibly;

		 public DoubleLatch() : this(1)
		 {
		 }

		 public DoubleLatch( int numberOfContestants ) : this( numberOfContestants, false )
		 {
		 }

		 public DoubleLatch( int numberOfContestants, bool awaitUninterruptibly )
		 {
			  this._startSignal = new System.Threading.CountdownEvent( numberOfContestants );
			  this._finishSignal = new System.Threading.CountdownEvent( numberOfContestants );
			  this._awaitUninterruptibly = awaitUninterruptibly;
		 }

		 public virtual void StartAndWaitForAllToStartAndFinish()
		 {
			  StartAndWaitForAllToStart();
			  WaitForAllToFinish();
		 }

		 public virtual void StartAndWaitForAllToStart()
		 {
			  Start();
			  WaitForAllToStart();
		 }

		 public virtual void Start()
		 {
			  _startSignal.Signal();
		 }

		 public virtual void WaitForAllToStart()
		 {
			  AwaitLatch( _startSignal, _awaitUninterruptibly );
		 }

		 public virtual void FinishAndWaitForAllToFinish()
		 {
			  Finish();
			  WaitForAllToFinish();
		 }

		 public virtual void Finish()
		 {
			  _finishSignal.Signal();
		 }

		 public virtual void WaitForAllToFinish()
		 {
			  AwaitLatch( _finishSignal, _awaitUninterruptibly );
		 }

		 public static void AwaitLatch( System.Threading.CountdownEvent latch )
		 {
			  AwaitLatch( latch, false );
		 }

		 public static void AwaitLatch( System.Threading.CountdownEvent latch, bool uninterruptedWaiting )
		 {
			  long now = DateTimeHelper.CurrentUnixTimeMillis();
			  long deadline = DateTimeHelper.CurrentUnixTimeMillis() + _fiveMinutes;

			  while ( now < deadline )
			  {
					try
					{

						 long waitingTime = Math.Min( Math.Max( 0, deadline - now ), 5000L );
						 if ( latch.await( waitingTime, TimeUnit.MILLISECONDS ) )
						 {
							  return;
						 }
						 else
						 {
							  Thread.yield();
						 }
					}
					catch ( InterruptedException e )
					{
						 Thread.interrupted();
						 if ( !uninterruptedWaiting )
						 {
							  throw new Exception( "Thread interrupted while waiting on latch", e );
						 }
					}
					now = DateTimeHelper.CurrentUnixTimeMillis();
			  }
			  throw new AssertionError( "Latch specified waiting time elapsed." );
		 }

		 public override string ToString()
		 {
			  return base.ToString() + "[Start[" + _startSignal.CurrentCount + "], Finish[" + _finishSignal.CurrentCount + "]]";
		 }
	}

}