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
namespace Neo4Net.Kernel
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using PageCacheWarmerMonitor = Neo4Net.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitor;
	using PageCacheWarmerMonitorAdapter = Neo4Net.Kernel.impl.pagecache.monitor.PageCacheWarmerMonitorAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

	internal class PageCacheWarmupTestSupport
	{
		 internal static void CreateTestData( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Label label = Label.label( "Label" );
					RelationshipType relationshipType = RelationshipType.withName( "REL" );
					long[] largeValue = new long[1024];
					for ( int i = 0; i < 1000; i++ )
					{
						 Node node = Db.createNode( label );
						 node.SetProperty( "Niels", "Borh" );
						 node.SetProperty( "Albert", largeValue );
						 for ( int j = 0; j < 30; j++ )
						 {
							  Relationship rel = node.CreateRelationshipTo( node, relationshipType );
							  rel.SetProperty( "Max", "Planck" );
						 }
					}
					tx.Success();
			  }
		 }

		 private class EnterpriseDatabaseRuleAnonymousInnerClass : Neo4Net.Test.rule.EnterpriseDatabaseRule
		 {
			 public EnterpriseDatabaseRuleAnonymousInnerClass( Neo4Net.Test.rule.TestDirectory testDirectory ) : base( testDirectory )
			 {
			 }

			 protected internal override void configure( GraphDatabaseFactory databaseFactory )
			 {
				  base.configure( databaseFactory );
				  ( ( TestGraphDatabaseFactory ) databaseFactory ).InternalLogProvider = logProvider;
			 }
		 }

		 private class RealOutsideWorldAnonymousInnerClass : Neo4Net.CommandLine.Admin.RealOutsideWorld
		 {
			 private readonly PageCacheWarmupEnterpriseEditionIT outerInstance;

			 public RealOutsideWorldAnonymousInnerClass( PageCacheWarmupEnterpriseEditionIT outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void exit( int status )
			 {
				  assertThat( "exit code", status, @is( 0 ) );
			 }
		 }

		 internal static long WaitForCacheProfile( Monitors monitors )
		 {
			  AtomicLong pageCount = new AtomicLong();
			  BinaryLatch profileLatch = new BinaryLatch();
			  PageCacheWarmerMonitor listener = new AwaitProfileMonitor( pageCount, profileLatch );
			  monitors.AddMonitorListener( listener );
			  profileLatch.Await();
			  monitors.RemoveMonitorListener( listener );
			  return pageCount.get();
		 }

		 internal static BinaryLatch PauseProfile( Monitors monitors )
		 {
			  return new PauseProfileMonitor( monitors );
		 }

		 private class AwaitProfileMonitor : PageCacheWarmerMonitorAdapter
		 {
			  internal readonly AtomicLong PageCount;
			  internal readonly BinaryLatch ProfileLatch;

			  internal AwaitProfileMonitor( AtomicLong pageCount, BinaryLatch profileLatch )
			  {
					this.PageCount = pageCount;
					this.ProfileLatch = profileLatch;
			  }

			  public override void ProfileCompleted( long pagesInMemory )
			  {
					PageCount.set( pagesInMemory );
					ProfileLatch.release();
			  }
		 }

		 private class PauseProfileMonitor : BinaryLatch, PageCacheWarmerMonitor
		 {
			  internal readonly Monitors Monitors;

			  internal PauseProfileMonitor( Monitors monitors )
			  {
					this.Monitors = monitors;
					monitors.AddMonitorListener( this );
			  }

			  public override void WarmupStarted()
			  {
					//nothing
			  }

			  public override void WarmupCompleted( long pagesLoaded )
			  {
					//nothing
			  }

			  public override void ProfileCompleted( long pagesInMemory )
			  {
					Await();
					Monitors.removeMonitorListener( this );
			  }
		 }
	}

}