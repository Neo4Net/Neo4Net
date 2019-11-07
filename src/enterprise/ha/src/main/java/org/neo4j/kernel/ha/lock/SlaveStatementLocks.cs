/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha.@lock
{

	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;

	/// <summary>
	/// Slave specific statement locks that in addition to standard functionality provided by configured delegate
	/// will grab selected shared locks on master during prepareForCommit phase.
	/// <para>
	/// In order to prevent various indexes collisions on master during transaction commit that originate on one of the
	/// slaves we need to grab same locks on <seealso cref="ResourceTypes.LABEL"/> and <seealso cref="ResourceTypes.RELATIONSHIP_TYPE"/> that
	/// where obtained on origin. To be able to do that and also prevent shared locks to be propagates to master in cases of
	/// read only transactions we will postpone obtaining them till we know that we participating in a
	/// transaction that performs modifications. That's why we will obtain them only as
	/// part of <seealso cref="prepareForCommit(LockTracer)"/> call.
	/// </para>
	/// </summary>
	public class SlaveStatementLocks : StatementLocks
	{
		 private readonly StatementLocks @delegate;

		 internal SlaveStatementLocks( StatementLocks @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override Neo4Net.Kernel.impl.locking.Locks_Client Pessimistic()
		 {
			  return @delegate.Pessimistic();
		 }

		 public override Neo4Net.Kernel.impl.locking.Locks_Client Optimistic()
		 {
			  return @delegate.Optimistic();
		 }

		 public override void PrepareForCommit( LockTracer lockTracer )
		 {
			  @delegate.PrepareForCommit( lockTracer );
			  ( ( SlaveLocksClient ) Optimistic() ).acquireDeferredSharedLocks(lockTracer);
		 }

		 public override void Stop()
		 {
			  @delegate.Stop();
		 }

		 public override void Close()
		 {
			  @delegate.Close();
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends Neo4Net.kernel.impl.locking.ActiveLock> activeLocks()
		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return @delegate.ActiveLocks();
		 }

		 public override long ActiveLockCount()
		 {
			  return @delegate.ActiveLockCount();
		 }
	}

}