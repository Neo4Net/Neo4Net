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
namespace Org.Neo4j.Adversaries
{

	/// <summary>
	/// An adversary that inject failures after a configured count of invocations.
	/// </summary>
	public class CountingAdversary : AbstractAdversary
	{
		 private readonly AtomicInteger _countDown;
		 private readonly int _startingCount;
		 private readonly bool _resetCountDownOnFailure;

		 public CountingAdversary( int countDownTillFailure, bool resetCountDownOnFailure )
		 {
			  this._startingCount = countDownTillFailure;
			  this._resetCountDownOnFailure = resetCountDownOnFailure;
			  _countDown = new AtomicInteger( countDownTillFailure );
		 }

		 public override void InjectFailure( params Type[] failureTypes )
		 {
			  int count;
			  int newCount;
			  do
			  {
					count = _countDown.get();
					newCount = count - 1;
			  } while ( !_countDown.compareAndSet( count, newCount ) );

			  if ( _resetCountDownOnFailure && newCount < 1 )
			  {
					Reset();
			  }

			  if ( newCount == 0 )
			  {
					try
					{
						 Thread.Sleep( 10 );
					}
					catch ( InterruptedException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
					ThrowOneOf( failureTypes );
			  }
		 }

		 public override bool InjectFailureOrMischief( params Type[] failureTypes )
		 {
			  InjectFailure( failureTypes );
			  return false;
		 }

		 private void Reset()
		 {
			  // The current count is going to be either zero or negative when we get here.
			  int count;
			  do
			  {
					count = _countDown.get();
			  } while ( count < 1 && !_countDown.compareAndSet( count, _startingCount + count ) );
		 }
	}

}