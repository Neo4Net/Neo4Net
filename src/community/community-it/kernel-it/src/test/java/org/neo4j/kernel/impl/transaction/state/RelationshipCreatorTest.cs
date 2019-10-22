using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using NoOpClient = Neo4Net.Kernel.impl.locking.NoOpClient;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using RelationshipCreator = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RelationshipCreator;
	using RelationshipGroupGetter = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RelationshipGroupGetter;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRecord = Neo4Net.Kernel.Impl.Store.Records.SchemaRecord;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using DirectRecordAccessSet = Neo4Net.@unsafe.Batchinsert.Internal.DirectRecordAccessSet;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class RelationshipCreatorTest
	{

		 private const int DENSE_NODE_THRESHOLD = 5;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule dbRule = new org.Neo4Net.test.rule.ImpermanentDatabaseRule().withSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, String.valueOf(DENSE_NODE_THRESHOLD));
		 public readonly DatabaseRule DbRule = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.dense_node_threshold, DENSE_NODE_THRESHOLD.ToString());
		 private IdGeneratorFactory _idGeneratorFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _idGeneratorFactory = DbRule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( IdGeneratorFactory ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyChangeLockedRecordsWhenUpgradingToDenseNode()
		 public virtual void ShouldOnlyChangeLockedRecordsWhenUpgradingToDenseNode()
		 {
			  // GIVEN
			  long nodeId = CreateNodeWithRelationships( DENSE_NODE_THRESHOLD );
			  NeoStores neoStores = FlipToNeoStores();

			  Tracker tracker = new Tracker( neoStores );
			  RelationshipGroupGetter groupGetter = new RelationshipGroupGetter( neoStores.RelationshipGroupStore );
			  RelationshipCreator relationshipCreator = new RelationshipCreator( groupGetter, 5 );

			  // WHEN
			  relationshipCreator.RelationshipCreate( _idGeneratorFactory.get( IdType.RELATIONSHIP ).nextId(), 0, nodeId, nodeId, tracker, tracker );

			  // THEN
			  assertEquals( tracker.RelationshipLocksAcquired.Count, tracker.ChangedRelationships.Count );
			  assertFalse( tracker.RelationshipLocksAcquired.Count == 0 );
		 }

		 private NeoStores FlipToNeoStores()
		 {
			  return DbRule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
		 }

		 private long CreateNodeWithRelationships( int count )
		 {
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					for ( int i = 0; i < count; i++ )
					{
						 node.CreateRelationshipTo( Db.createNode(), MyRelTypes.TEST );
					}
					tx.Success();
					return node.Id;
			  }
		 }

		 internal class Tracker : NoOpClient, RecordAccessSet
		 {
			  internal readonly RecordAccessSet Delegate;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TrackingRecordAccess<RelationshipRecord, Void> RelRecordsConflict;
			  internal readonly ISet<long> RelationshipLocksAcquired = new HashSet<long>();
			  internal readonly ISet<long> ChangedRelationships = new HashSet<long>();

			  internal Tracker( NeoStores neoStores )
			  {
					this.Delegate = new DirectRecordAccessSet( neoStores );
					this.RelRecordsConflict = new TrackingRecordAccess<RelationshipRecord, Void>( Delegate.RelRecords, this );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.Neo4Net.storageengine.api.lock.LockTracer tracer, org.Neo4Net.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.Neo4Net.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					assertEquals( ResourceTypes.RELATIONSHIP, resourceType );
					foreach ( long resourceId in resourceIds )
					{
						 RelationshipLocksAcquired.Add( resourceId );
					}
			  }

			  protected internal virtual void ChangingRelationship( long relId )
			  { // Called by tracking record proxies
					assertTrue( "Tried to change relationship " + relId + " without this transaction having it locked", RelationshipLocksAcquired.Contains( relId ) );
					ChangedRelationships.Add( relId );
			  }

			  public virtual RecordAccess<NodeRecord, Void> NodeRecords
			  {
				  get
				  {
						return Delegate.NodeRecords;
				  }
			  }

			  public virtual RecordAccess<PropertyRecord, PrimitiveRecord> PropertyRecords
			  {
				  get
				  {
						return Delegate.PropertyRecords;
				  }
			  }

			  public virtual RecordAccess<RelationshipRecord, Void> RelRecords
			  {
				  get
				  {
						return RelRecordsConflict;
				  }
			  }

			  public virtual RecordAccess<RelationshipGroupRecord, int> RelGroupRecords
			  {
				  get
				  {
						return Delegate.RelGroupRecords;
				  }
			  }

			  public virtual RecordAccess<SchemaRecord, SchemaRule> SchemaRuleChanges
			  {
				  get
				  {
						return Delegate.SchemaRuleChanges;
				  }
			  }

			  public virtual RecordAccess<PropertyKeyTokenRecord, Void> PropertyKeyTokenChanges
			  {
				  get
				  {
						return Delegate.PropertyKeyTokenChanges;
				  }
			  }

			  public virtual RecordAccess<LabelTokenRecord, Void> LabelTokenChanges
			  {
				  get
				  {
						return Delegate.LabelTokenChanges;
				  }
			  }

			  public virtual RecordAccess<RelationshipTypeTokenRecord, Void> RelationshipTypeTokenChanges
			  {
				  get
				  {
						return Delegate.RelationshipTypeTokenChanges;
				  }
			  }

			  public override void Close()
			  {
					Delegate.close();
			  }

			  public override bool HasChanges()
			  {
					return Delegate.hasChanges();
			  }

			  public override int ChangeSize()
			  {
					return Delegate.changeSize();
			  }
		 }
	}

}