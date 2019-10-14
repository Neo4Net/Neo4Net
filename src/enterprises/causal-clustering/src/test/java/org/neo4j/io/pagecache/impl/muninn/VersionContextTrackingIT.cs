using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using Neo4Net.Functions;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class VersionContextTrackingIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();
		 private const int NUMBER_OF_TRANSACTIONS = 3;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.withSharedCoreParam( GraphDatabaseSettings.snapshot_query, TRUE ).withSharedReadReplicaParam( GraphDatabaseSettings.snapshot_query, TRUE ).startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void coreMemberTransactionIdPageTracking() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CoreMemberTransactionIdPageTracking()
		 {
			  long baseTxId = BaseTransactionId;
			  for ( int i = 1; i < 4; i++ )
			  {
					GenerateData();
					long expectedLatestPageVersion = GetExpectedLatestPageVersion( baseTxId, i );
					ThrowingSupplier<long, Exception> anyCoreSupplier = () => GetLatestPageVersion(AnyCore);
					assertEventually( "Any core page version should match to expected page version.", anyCoreSupplier, @is( expectedLatestPageVersion ), 2, MINUTES );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readReplicatesTransactionIdPageTracking() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadReplicatesTransactionIdPageTracking()
		 {
			  long baseTxId = BaseTransactionId;
			  for ( int i = 1; i < 4; i++ )
			  {
					GenerateData();
					long expectedLatestPageVersion = GetExpectedLatestPageVersion( baseTxId, i );
					ThrowingSupplier<long, Exception> replicateVersionSupplier = () => GetLatestPageVersion(AnyReadReplica);
					assertEventually( "Read replica page version should match to core page version.", replicateVersionSupplier, @is( expectedLatestPageVersion ), 2, MINUTES );
			  }
		 }

		 private long GetExpectedLatestPageVersion( long baseTxId, int round )
		 {
			  return baseTxId + round * NUMBER_OF_TRANSACTIONS;
		 }

		 private long BaseTransactionId
		 {
			 get
			 {
				  DependencyResolver dependencyResolver = AnyCore.DependencyResolver;
				  TransactionIdStore transactionIdStore = dependencyResolver.ResolveDependency( typeof( TransactionIdStore ) );
				  return transactionIdStore.LastClosedTransactionId;
			 }
		 }

		 private CoreClusterMember AnyCoreClusterMember()
		 {
			  return _cluster.coreMembers().GetEnumerator().next();
		 }

		 private CoreGraphDatabase AnyCore
		 {
			 get
			 {
				  return AnyCoreClusterMember().database();
			 }
		 }

		 private ReadReplicaGraphDatabase AnyReadReplica
		 {
			 get
			 {
				  return _cluster.findAnyReadReplica().database();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long getLatestPageVersion(org.neo4j.kernel.impl.factory.GraphDatabaseFacade databaseFacade) throws java.io.IOException
		 private static long GetLatestPageVersion( GraphDatabaseFacade databaseFacade )
		 {
			  DependencyResolver dependencyResolver = databaseFacade.DependencyResolver;
			  PageCache pageCache = dependencyResolver.ResolveDependency( typeof( PageCache ) );
			  NeoStores neoStores = dependencyResolver.ResolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  File storeFile = neoStores.NodeStore.StorageFile;
			  long maxTransactionId = long.MinValue;
			  using ( PagedFile pageFile = pageCache.GetExistingMapping( storeFile ).get() )
			  {
					long lastPageId = pageFile.LastPageId;
					for ( int i = 0; i <= lastPageId; i++ )
					{
						 using ( CursorPageAccessor pageCursor = new CursorPageAccessor( ( MuninnPageCursor ) pageFile.Io( i, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) ) )
						 {
							  if ( pageCursor.Next() )
							  {
									maxTransactionId = Math.Max( maxTransactionId, pageCursor.LastTxModifierId() );
							  }
						 }
					}
			  }
			  return maxTransactionId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void generateData() throws Exception
		 private void GenerateData()
		 {
			  for ( int i = 0; i < NUMBER_OF_TRANSACTIONS; i++ )
			  {
					_cluster.coreTx((coreGraphDatabase, transaction) =>
					{
					 coreGraphDatabase.createNode();
					 transaction.success();
					});
			  }
		 }

		 private class CursorPageAccessor : MuninnPageCursor
		 {

			  internal MuninnPageCursor Delegate;

			  internal CursorPageAccessor( MuninnPageCursor @delegate ) : base( -1, PageCursorTracer.NULL, EmptyVersionContextSupplier.EMPTY )
			  {
					this.Delegate = @delegate;
			  }

			  internal virtual long LastTxModifierId()
			  {
					return Delegate.pagedFile.getLastModifiedTxId( Delegate.pinnedPageRef );
			  }

			  protected internal override void UnpinCurrentPage()
			  {
					Delegate.unpinCurrentPage();
			  }

			  protected internal override void ConvertPageFaultLock( long pageRef )
			  {
					Delegate.convertPageFaultLock( pageRef );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void pinCursorToPage(long pageRef, long filePageId, org.neo4j.io.pagecache.PageSwapper swapper) throws org.neo4j.io.pagecache.impl.FileIsNotMappedException
			  protected internal override void PinCursorToPage( long pageRef, long filePageId, PageSwapper swapper )
			  {
					Delegate.pinCursorToPage( pageRef, filePageId, swapper );
			  }

			  protected internal override bool TryLockPage( long pageRef )
			  {
					return Delegate.tryLockPage( pageRef );
			  }

			  protected internal override void UnlockPage( long pageRef )
			  {
					Delegate.unlockPage( pageRef );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
			  public override bool Next()
			  {
					return Delegate.next();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
			  public override bool ShouldRetry()
			  {
					return Delegate.shouldRetry();
			  }
		 }
	}

}