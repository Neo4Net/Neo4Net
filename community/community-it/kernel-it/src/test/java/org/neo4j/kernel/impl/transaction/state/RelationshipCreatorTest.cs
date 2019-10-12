using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using NoOpClient = Org.Neo4j.Kernel.impl.locking.NoOpClient;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using RelationshipCreator = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RelationshipCreator;
	using RelationshipGroupGetter = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RelationshipGroupGetter;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRecord = Org.Neo4j.Kernel.impl.store.record.SchemaRecord;
	using AcquireLockTimeoutException = Org.Neo4j.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using DirectRecordAccessSet = Org.Neo4j.@unsafe.Batchinsert.@internal.DirectRecordAccessSet;

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
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, String.valueOf(DENSE_NODE_THRESHOLD));
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
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
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
//ORIGINAL LINE: public void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
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