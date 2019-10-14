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
namespace Neo4Net.Kernel.impl.util
{
	/// <summary>
	/// A non continuous, strictly monotonic queue of transaction ids. Equivalently, a queue where the head is always
	/// the minimum value.
	/// 
	/// Threads can wait for the minimal value to reach a specific value, upon which event they are woken up and can
	/// act. This notification happens only when the current minimal value (head) is removed, so care should be taken
	/// to remove it when done.
	/// </summary>
	public interface IdOrderingQueue
	{
		 /// <summary>
		 /// Adds this id at the tail of the queue. The argument must be larger than all previous arguments
		 /// passed to this method. </summary>
		 /// <param name="value"> The id to add </param>
		 void Offer( long value );

		 /// <summary>
		 /// Waits for the argument to become the head of the queue. This is a blocking operation and as such it may
		 /// throw InterruptedException. </summary>
		 /// <param name="value"> The id to wait for to become the head of the queue </param>
		 /// <exception cref="InterruptedException"> if interrupted while waiting. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void waitFor(long value) throws InterruptedException;
		 void WaitFor( long value );

		 /// <summary>
		 /// Remove the current minimum value, while ensuring that it the expected value. </summary>
		 /// <param name="expectedValue"> The value the minimum value is supposed to be - if the check fails,
		 ///                      an IllegalStateException will be thrown and the notification of waiting threads will not
		 ///                      happen. </param>
		 void RemoveChecked( long expectedValue );

		 bool Empty { get; }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 IdOrderingQueue BYPASS = new IdOrderingQueue()
	//	 {
	//		  @@Override public void offer(long value)
	//		  { // Just ignore, it's fine
	//		  }
	//
	//		  @@Override public void waitFor(long value)
	//		  { // Just ignore, it's fine
	//		  }
	//
	//		  @@Override public void removeChecked(long expectedValue)
	//		  { // Just ignore, it's fine
	//		  }
	//
	//		  @@Override public boolean isEmpty()
	//		  {
	//				return true;
	//		  }
	//	 };
	}

}