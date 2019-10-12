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
namespace Org.Neo4j.Kernel.impl.coreapi
{
	using DatabaseShutdownException = Org.Neo4j.Graphdb.DatabaseShutdownException;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using UnavailableException = Org.Neo4j.Kernel.availability.UnavailableException;

	/// <summary>
	/// This is a simple wrapper around <seealso cref="DatabaseAvailabilityGuard"/> that augments its behavior to match how
	/// availability errors and timeouts are handled in the Core API.
	/// </summary>
	public class CoreAPIAvailabilityGuard
	{
		 private readonly DatabaseAvailabilityGuard _guard;
		 private readonly long _timeout;

		 public CoreAPIAvailabilityGuard( DatabaseAvailabilityGuard guard, long timeout )
		 {
			  this._guard = guard;
			  this._timeout = timeout;
		 }

		 public virtual bool IsAvailable( long timeoutMillis )
		 {
			  return _guard.isAvailable( timeoutMillis );
		 }

		 public virtual void AssertDatabaseAvailable()
		 {
			  try
			  {
					_guard.await( _timeout );
			  }
			  catch ( UnavailableException e )
			  {
					if ( _guard.Shutdown )
					{
						 throw new DatabaseShutdownException();
					}
					throw new Org.Neo4j.Graphdb.TransactionFailureException( e.Message, e );
			  }
		 }
	}

}