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
namespace Org.Neo4j.Graphdb
{
	/// <summary>
	/// An acquired lock on an entity for a transaction, acquired from
	/// <seealso cref="Transaction.acquireWriteLock(PropertyContainer)"/> or
	/// <seealso cref="Transaction.acquireReadLock(PropertyContainer)"/> this lock
	/// can be released manually using <seealso cref="release()"/>. If not released
	/// manually it will be automatically released when the transaction owning
	/// it finishes.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public interface Lock
	{
		 /// <summary>
		 /// Releases this lock before the transaction finishes. It is an optional
		 /// operation and if not called, this lock will be released when the owning
		 /// transaction finishes.
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if this lock has already been released. </exception>
		 void Release();
	}

}