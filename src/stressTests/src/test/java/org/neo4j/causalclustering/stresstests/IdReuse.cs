using System;
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
namespace Neo4Net.causalclustering.stresstests
{

	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using WriteOperationsNotAllowedException = Neo4Net.GraphDb.security.WriteOperationsNotAllowedException;
	using Workload = Neo4Net.helper.Workload;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IdContainer = Neo4Net.Kernel.impl.store.id.IdContainer;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.stresstests.TxHelp.isInterrupted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.stresstests.TxHelp.isTransient;

	/// <summary>
	/// Resources for stress testing ID-reuse scenarios.
	/// </summary>
	internal class IdReuse
	{
		 private static readonly RelationshipType _relationshipType = RelationshipType.withName( "testType" );

		 /// <summary>
		 /// Validate free ids. All must be unique.
		 /// </summary>
		 internal class UniqueFreeIds : Validation
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
			  internal readonly Cluster<object> Cluster;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly Log Log;

			  internal UniqueFreeIds( Resources resources ) : base()
			  {
					this.Cluster = resources.Cluster();
					this.Fs = resources.FileSystem();
					this.Log = resources.LogProvider().getLog(this.GetType());
			  }

			  protected internal override void Validate()
			  {
					IEnumerable<ClusterMember> members = Iterables.concat( Cluster.coreMembers(), Cluster.readReplicas() );
					ISet<long> unusedIds = new HashSet<long>();
					ISet<long> nonUniqueIds = new HashSet<long>();

					foreach ( ClusterMember member in members )
					{
						 VisitAllIds(member, id =>
						 {
						  if ( !unusedIds.Add( id ) )
						  {
								nonUniqueIds.Add( id );
						  }
						 });
					}

					if ( nonUniqueIds.Count != 0 )
					{
						 foreach ( ClusterMember member in members )
						 {
							  VisitAllIds(member, id =>
							  {
								if ( nonUniqueIds.Contains( id ) )
								{
									 Log.error( member + " has non-unique free ID: " + id );
								}
							  });
						 }

						 throw new System.InvalidOperationException( "Non-unique IDs found: " + nonUniqueIds );
					}

					Log.info( "Total of " + unusedIds.Count + " reusable ids found" );
			  }

			  internal virtual void VisitAllIds( ClusterMember member, System.Action<long> idConsumer )
			  {
					DatabaseLayout databaseLayout = DatabaseLayout.of( member.databaseDirectory() );
					File idFile = databaseLayout.IdNodeStore();
					IdContainer idContainer = new IdContainer( Fs, idFile, 1024, true );
					idContainer.Init();
					Log.info( idFile.AbsolutePath + " has " + idContainer.FreeIdCount + " free ids" );

					long id = idContainer.ReusableId;
					while ( id != IdContainer.NO_RESULT )
					{
						 idConsumer( id );
						 id = idContainer.ReusableId;
					}

					idContainer.Close( 0 );
			  }
		 }

		 internal class IdReuseSetup : Preparation
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
			  internal readonly Cluster<object> Cluster;

			  internal IdReuseSetup( Resources resources ) : base()
			  {
					Cluster = resources.Cluster();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void prepare() throws Exception
			  protected internal override void Prepare()
			  {
					for ( int i = 0; i < 1_000; i++ )
					{
						 try
						 {
							  Cluster.coreTx((db, tx) =>
							  {
							  for ( int j = 0; j < 1_000; j++ )
							  {
								  Node start = Db.createNode();
								  Node end = Db.createNode();
								  start.createRelationshipTo( end, _relationshipType );
							  }
							  tx.success();
							  });
						 }
						 catch ( WriteOperationsNotAllowedException )
						 {
							  // skip
						 }
					}
			  }
		 }

		 internal class InsertionWorkload : Workload
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
			  internal Cluster<object> Cluster;

			  internal InsertionWorkload( Control control, Resources resources ) : base( control )
			  {
					this.Cluster = resources.Cluster();
			  }

			  protected internal override void DoWork()
			  {
					try
					{
						 Cluster.coreTx((db, tx) =>
						 {
						 Node nodeStart = Db.createNode();
						 Node nodeEnd = Db.createNode();
						 nodeStart.createRelationshipTo( nodeEnd, _relationshipType );
						 tx.success();
						 });
					}
					catch ( Exception e )
					{
						 if ( isInterrupted( e ) || isTransient( e ) )
						 {
							  // whatever let's go on with the workload
							  return;
						 }

						 throw new Exception( "InsertionWorkload", e );
					}
			  }
		 }

		 internal class ReelectionWorkload : Workload
		 {
			  internal readonly long ReelectIntervalSeconds;
			  internal readonly Log Log;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
			  internal Cluster<object> Cluster;

			  internal ReelectionWorkload( Control control, Resources resources, Config config ) : base( control )
			  {
					this.Cluster = resources.Cluster();
					this.ReelectIntervalSeconds = config.ReelectIntervalSeconds();
					this.Log = config.LogProvider().getLog(this.GetType());
			  }

			  protected internal override void DoWork()
			  {
					try
					{
						 CoreClusterMember leader = Cluster.awaitLeader();
						 leader.Shutdown();
						 leader.Start();
						 Log.info( "Restarting leader" );
						 TimeUnit.SECONDS.sleep( ReelectIntervalSeconds );
					}
					catch ( Exception e )
					{
						 if ( isInterrupted( e ) || isTransient( e ) )
						 {
							  // whatever let's go on with the workload
							  return;
						 }

						 throw new Exception( "ReelectionWorkload", e );
					}
			  }
		 }

		 internal class DeletionWorkload : Workload
		 {
			  internal readonly SecureRandom Rnd = new SecureRandom();
			  internal readonly int IdHighRange;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
			  internal Cluster<object> Cluster;

			  internal DeletionWorkload( Control control, Resources resources ) : base( control )
			  {
					this.Cluster = resources.Cluster();
					this.IdHighRange = 2_000_000;
			  }

			  protected internal override void DoWork()
			  {
					try
					{
						 Cluster.coreTx((db, tx) =>
						 {
						 Node node = Db.getNodeById( Rnd.Next( IdHighRange ) );
						 Iterables.stream( node.Relationships ).forEach( Relationship.delete );
						 node.delete();
						 tx.success();
						 });
					}
					catch ( NotFoundException )
					{
						 // Expected
					}
					catch ( Exception e )
					{
						 if ( isInterrupted( e ) || isTransient( e ) )
						 {
							  // whatever let's go on with the workload
							  return;
						 }

						 throw new Exception( "DeletionWorkload", e );
					}
			  }
		 }
	}

}