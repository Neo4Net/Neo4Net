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
namespace Org.Neo4j.Kernel.availability
{
	/// <summary>
	/// The availability guard ensures that the database will only take calls when it is in an ok state.
	/// It tracks a set of requirements (added via <seealso cref="require(AvailabilityRequirement)"/>) that must all be marked
	/// as fulfilled (using <seealso cref="fulfill(AvailabilityRequirement)"/>) before the database is considered available again.
	/// Consumers determine if it is ok to call the database using <seealso cref="isAvailable()"/>,
	/// or await availability using <seealso cref="isAvailable(long)"/>.
	/// </summary>
	public interface AvailabilityGuard
	{
		 /// <summary>
		 /// Indicate a requirement that must be fulfilled before the database is considered available.
		 /// </summary>
		 /// <param name="requirement"> the requirement object </param>
		 void Require( AvailabilityRequirement requirement );

		 /// <summary>
		 /// Indicate that a requirement has been fulfilled.
		 /// </summary>
		 /// <param name="requirement"> the requirement object </param>
		 void Fulfill( AvailabilityRequirement requirement );

		 /// <summary>
		 /// Shutdown the guard. After this method is invoked, the availability guard will always be considered unavailable.
		 /// </summary>
		 void Shutdown();

		 /// <summary>
		 /// Check if the database is available for transactions to use.
		 /// </summary>
		 /// <returns> true if there are no requirements waiting to be fulfilled and the guard has not been shutdown </returns>
		 bool Available { get; }

		 /// <summary>
		 /// Check if the database has been shut down.
		 /// </summary>
		 bool Shutdown { get; }

		 /// <summary>
		 /// Check if the database is available for transactions to use.
		 /// </summary>
		 /// <param name="millis"> to wait for availability </param>
		 /// <returns> true if there are no requirements waiting to be fulfilled and the guard has not been shutdown </returns>
		 bool IsAvailable( long millis );

		 /// <summary>
		 /// Checks if available. If not then an <seealso cref="UnavailableException"/> is thrown describing why.
		 /// This methods doesn't wait like <seealso cref="await(long)"/> does.
		 /// </summary>
		 /// <exception cref="UnavailableException"> if not available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkAvailable() throws UnavailableException;
		 void CheckAvailable();

		 /// <summary>
		 /// Await the database becoming available.
		 /// </summary>
		 /// <param name="millis"> to wait for availability </param>
		 /// <exception cref="UnavailableException"> thrown when the timeout has been exceeded or the guard has been shutdown </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void await(long millis) throws UnavailableException;
		 void Await( long millis );

		 /// <summary>
		 /// Add a listener for changes to availability.
		 /// </summary>
		 /// <param name="listener"> the listener to receive callbacks when availability changes </param>
		 void AddListener( AvailabilityListener listener );

		 /// <summary>
		 /// Remove a listener for changes to availability.
		 /// </summary>
		 /// <param name="listener"> the listener to remove </param>
		 void RemoveListener( AvailabilityListener listener );
	}

}