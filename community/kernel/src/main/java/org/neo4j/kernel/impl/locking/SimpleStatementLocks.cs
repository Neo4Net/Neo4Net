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
namespace Org.Neo4j.Kernel.impl.locking
{

	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;

	/// <summary>
	/// A <seealso cref="StatementLocks"/> implementation that uses given <seealso cref="Locks.Client"/> for both
	/// <seealso cref="optimistic() optimistic"/> and <seealso cref="pessimistic() pessimistic"/> locks.
	/// </summary>
	public class SimpleStatementLocks : StatementLocks
	{
		 private readonly Locks_Client _client;

		 public SimpleStatementLocks( Locks_Client client )
		 {
			  this._client = client;
		 }

		 public override Locks_Client Pessimistic()
		 {
			  return _client;
		 }

		 public override Locks_Client Optimistic()
		 {
			  return _client;
		 }

		 public override void PrepareForCommit( LockTracer lockTracer )
		 {
			  // Locks where grabbed eagerly by client so no need to prepare
			  _client.prepare();
		 }

		 public override void Stop()
		 {
			  _client.stop();
		 }

		 public override void Close()
		 {
			  _client.close();
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends ActiveLock> activeLocks()
		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return _client.activeLocks();
		 }

		 public override long ActiveLockCount()
		 {
			  return _client.activeLockCount();
		 }
	}

}