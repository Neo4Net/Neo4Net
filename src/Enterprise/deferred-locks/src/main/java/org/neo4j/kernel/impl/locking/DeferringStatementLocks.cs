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
namespace Neo4Net.Kernel.impl.locking
{

	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;

	/// <summary>
	/// A <seealso cref="StatementLocks"/> implementation that defers <seealso cref="optimistic() optimistic"/>
	/// locks using <seealso cref="DeferringLockClient"/>.
	/// </summary>
	public class DeferringStatementLocks : StatementLocks
	{
		 private readonly Locks_Client @explicit;
		 private readonly DeferringLockClient @implicit;

		 public DeferringStatementLocks( Locks_Client @explicit )
		 {
			  this.@explicit = @explicit;
			  this.@implicit = new DeferringLockClient( this.@explicit );
		 }

		 public override Locks_Client Pessimistic()
		 {
			  return @explicit;
		 }

		 public override Locks_Client Optimistic()
		 {
			  return @implicit;
		 }

		 public override void PrepareForCommit( LockTracer lockTracer )
		 {
			  @implicit.AcquireDeferredLocks( lockTracer );
			  @explicit.Prepare();
		 }

		 public override void Stop()
		 {
			  @implicit.Stop();
		 }

		 public override void Close()
		 {
			  @implicit.Close();
		 }

		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return Stream.concat( @explicit.ActiveLocks(), @implicit.ActiveLocks() );
		 }

		 public override long ActiveLockCount()
		 {
			  return @explicit.ActiveLockCount() + @implicit.ActiveLockCount();
		 }
	}

}