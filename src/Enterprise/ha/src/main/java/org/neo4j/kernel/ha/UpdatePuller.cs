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
namespace Neo4Net.Kernel.ha
{
	/// <summary>
	/// Puller of transactions updates from a different store. Pulls for updates and applies them into a current store.
	/// <para>
	/// On a running instance of a store there should be only one active implementation of this interface.
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= SlaveUpdatePuller </seealso>
	/// <seealso cref= MasterUpdatePuller </seealso>
	public interface UpdatePuller
	{
		 /// <summary>
		 /// Pull all available updates.
		 /// </summary>
		 /// <exception cref="InterruptedException"> in case if interrupted while waiting for updates </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void pullUpdates() throws InterruptedException;
		 void PullUpdates();

		 /// <summary>
		 /// Try to pull all updates
		 /// </summary>
		 /// <returns> true if all updates pulled, false if updater fail on update retrieval </returns>
		 /// <exception cref="InterruptedException"> in case if interrupted while waiting for updates </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean tryPullUpdates() throws InterruptedException;
		 bool TryPullUpdates();

		 /// <summary>
		 /// Start update pulling
		 /// </summary>
		 void Start();

		 /// <summary>
		 /// Terminate update pulling
		 /// </summary>
		 void Stop();

		 /// <summary>
		 /// Pull updates and waits for the supplied condition to be
		 /// fulfilled as part of the update pulling happening.
		 /// </summary>
		 /// <param name="condition"> <seealso cref="UpdatePuller.Condition"/> to wait for. </param>
		 /// <param name="assertPullerActive"> if {@code true} then observing an inactive update puller
		 /// will throw an <seealso cref="System.InvalidOperationException"/>, </param>
		 /// <exception cref="InterruptedException"> if we were interrupted while awaiting the condition. </exception>
		 /// <exception cref="IllegalStateException"> if {@code strictlyAssertActive} and the update puller
		 /// became inactive while awaiting the condition. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void pullUpdates(UpdatePuller_Condition condition, boolean assertPullerActive) throws InterruptedException;
		 void PullUpdates( UpdatePuller_Condition condition, bool assertPullerActive );

		 /// <summary>
		 /// Condition to be meet during update pulling.
		 /// </summary>

	}

	 public interface UpdatePuller_Condition
	 {
		  bool Evaluate( int currentTicket, int targetTicket );
	 }

}