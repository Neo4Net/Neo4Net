using System.Collections.Generic;

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
namespace Neo4Net.Kernel.enterprise.builtinprocs
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;


	public class TransactionDependenciesResolver
	{
		 private readonly IDictionary<KernelTransactionHandle, IList<QuerySnapshot>> _handleSnapshotsMap;
		 private IDictionary<KernelTransactionHandle, ISet<KernelTransactionHandle>> _directDependencies;

		 internal TransactionDependenciesResolver( IDictionary<KernelTransactionHandle, IList<QuerySnapshot>> handleSnapshotsMap )
		 {
			  this._handleSnapshotsMap = handleSnapshotsMap;
			  this._directDependencies = InitDirectDependencies();
		 }

		 public virtual bool IsBlocked( KernelTransactionHandle handle )
		 {
			  return _directDependencies[handle] != null;
		 }

		 public virtual string DescribeBlockingTransactions( KernelTransactionHandle handle )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  ISet<KernelTransactionHandle> allBlockers = new SortedSet<KernelTransactionHandle>( System.Collections.IComparer.comparingLong( KernelTransactionHandle::getUserTransactionId ) );
			  ISet<KernelTransactionHandle> handles = _directDependencies[handle];
			  if ( handles != null )
			  {
					Deque<KernelTransactionHandle> blockerQueue = new LinkedList<KernelTransactionHandle>( handles );
					while ( !blockerQueue.Empty )
					{
						 KernelTransactionHandle transactionHandle = blockerQueue.pop();
						 if ( allBlockers.Add( transactionHandle ) )
						 {
							  ISet<KernelTransactionHandle> transactionHandleSet = _directDependencies[transactionHandle];
							  if ( transactionHandleSet != null )
							  {
									blockerQueue.addAll( transactionHandleSet );
							  }
						 }
					}
			  }
			  return Describe( allBlockers );
		 }

		 public virtual IDictionary<string, object> DescribeBlockingLocks( KernelTransactionHandle handle )
		 {
			  IList<QuerySnapshot> querySnapshots = _handleSnapshotsMap[handle];
			  if ( querySnapshots.Count > 0 )
			  {
					return querySnapshots[0].ResourceInformation();
			  }
			  return Collections.emptyMap();
		 }

		 private IDictionary<KernelTransactionHandle, ISet<KernelTransactionHandle>> InitDirectDependencies()
		 {
			  IDictionary<KernelTransactionHandle, ISet<KernelTransactionHandle>> directDependencies = new Dictionary<KernelTransactionHandle, ISet<KernelTransactionHandle>>();

			  IDictionary<KernelTransactionHandle, IList<ActiveLock>> transactionLocksMap = _handleSnapshotsMap.Keys.ToDictionary( identity(), TransactionLocks );

			  foreach ( KeyValuePair<KernelTransactionHandle, IList<QuerySnapshot>> entry in _handleSnapshotsMap.SetOfKeyValuePairs() )
			  {
					IList<QuerySnapshot> querySnapshots = entry.Value;
					if ( querySnapshots.Count > 0 )
					{
						 KernelTransactionHandle txHandle = entry.Key;
						 EvaluateDirectDependencies( directDependencies, transactionLocksMap, txHandle, querySnapshots[0] );
					}
			  }
			  return directDependencies;
		 }

		 private System.Func<KernelTransactionHandle, IList<ActiveLock>> TransactionLocks
		 {
			 get
			 {
				  return transactionHandle => transactionHandle.activeLocks().collect(toList());
			 }
		 }

		 private void EvaluateDirectDependencies( IDictionary<KernelTransactionHandle, ISet<KernelTransactionHandle>> directDependencies, IDictionary<KernelTransactionHandle, IList<ActiveLock>> handleLocksMap, KernelTransactionHandle txHandle, QuerySnapshot querySnapshot )
		 {
			  IList<ActiveLock> waitingOnLocks = querySnapshot.WaitingLocks();
			  foreach ( ActiveLock activeLock in waitingOnLocks )
			  {
					foreach ( KeyValuePair<KernelTransactionHandle, IList<ActiveLock>> handleListEntry in handleLocksMap.SetOfKeyValuePairs() )
					{
						 KernelTransactionHandle kernelTransactionHandle = handleListEntry.Key;
						 if ( !kernelTransactionHandle.Equals( txHandle ) )
						 {
							  if ( IsBlocked( activeLock, handleListEntry.Value ) )
							  {
									ISet<KernelTransactionHandle> kernelTransactionHandles = directDependencies.computeIfAbsent( txHandle, handle => new HashSet<KernelTransactionHandle>() );
									kernelTransactionHandles.Add( kernelTransactionHandle );
							  }
						 }
					}
			  }
		 }

		 private bool IsBlocked( ActiveLock activeLock, IList<ActiveLock> activeLocks )
		 {
			  return Neo4Net.Kernel.impl.locking.ActiveLock_Fields.EXCLUSIVE_MODE.Equals( activeLock.Mode() ) ? HaveAnyLocking(activeLocks, activeLock.ResourceType(), activeLock.ResourceId()) : HaveExclusiveLocking(activeLocks, activeLock.ResourceType(), activeLock.ResourceId());
		 }

		 private static bool HaveAnyLocking( IList<ActiveLock> locks, ResourceType resourceType, long resourceId )
		 {
			  return locks.Any( @lock => @lock.resourceId() == resourceId && @lock.resourceType() == resourceType );
		 }

		 private static bool HaveExclusiveLocking( IList<ActiveLock> locks, ResourceType resourceType, long resourceId )
		 {
			  return locks.Any( @lock => Neo4Net.Kernel.impl.locking.ActiveLock_Fields.EXCLUSIVE_MODE.Equals( @lock.mode() ) && @lock.resourceId() == resourceId && @lock.resourceType() == resourceType );
		 }

		 private string Describe( ISet<KernelTransactionHandle> allBlockers )
		 {
			  if ( allBlockers.Count == 0 )
			  {
					return StringUtils.EMPTY;
			  }
			  StringJoiner stringJoiner = new StringJoiner( ", ", "[", "]" );
			  foreach ( KernelTransactionHandle blocker in allBlockers )
			  {
					stringJoiner.add( blocker.UserTransactionName );
			  }
			  return stringJoiner.ToString();
		 }
	}

}