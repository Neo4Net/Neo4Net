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
namespace Neo4Net.Consistency.checking
{
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using MultiPassStore = Neo4Net.Consistency.checking.full.MultiPassStore;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using Neo4Net.Consistency.store;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_NextRelationship_Fields.SLOT_FIRST_IN_SOURCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_NextRelationship_Fields.SLOT_FIRST_IN_TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.NEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_RELATIONSHIP_ID;

	internal abstract class NodeField : RecordField<RelationshipRecord, ConsistencyReport.RelationshipConsistencyReport>, ComparativeRecordChecker<RelationshipRecord, NodeRecord, ConsistencyReport.RelationshipConsistencyReport>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SOURCE { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstNode(); } public long prev(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstPrevRel(); } public long next(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstNextRel(); } public boolean isFirst(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.isFirstInFirstChain(); } void illegalNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report) { report.illegalSourceNode(); } void nodeNotInUse(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.NodeRecord node) { report.sourceNodeNotInUse(node); } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.NodeRecord node) { report.sourceNodeDoesNotReferenceBack(node); } void noChain(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.NodeRecord node) { report.sourceNodeHasNoRelationships(node); } void notFirstInChain(org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.relationshipNotFirstInSourceChain(relationship); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TARGET { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondNode(); } public long prev(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondPrevRel(); } public long next(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondNextRel(); } public boolean isFirst(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.isFirstInSecondChain(); } void illegalNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report) { report.illegalTargetNode(); } void nodeNotInUse(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.NodeRecord node) { report.targetNodeNotInUse(node); } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.NodeRecord node) { report.targetNodeDoesNotReferenceBack(node); } void noChain(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.NodeRecord node) { report.targetNodeHasNoRelationships(node); } void notFirstInChain(org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.relationshipNotFirstInTargetChain(relationship); } };

		 private static readonly IList<NodeField> valueList = new List<NodeField>();

		 static NodeField()
		 {
			 valueList.Add( SOURCE );
			 valueList.Add( TARGET );
		 }

		 public enum InnerEnum
		 {
			 SOURCE,
			 TARGET
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private NodeField( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract long valueFrom( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

		 public static NodeField select( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, Neo4Net.Kernel.Impl.Store.Records.NodeRecord node ) { return select( relationship, node.getId() ); } public static NodeField select(Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, long nodeId)
		 {
			 if ( relationship.getFirstNode() == nodeId ) { return SOURCE; } else if (relationship.getSecondNode() == nodeId) { return TARGET; } else { return null; }
		 }
		 public abstract long prev( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

		 public abstract long next( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

		 public abstract bool isFirst( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public boolean hasRelationship(org.neo4j.kernel.impl.store.record.NodeRecord node) { return node.getNextRel() != org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } @Override public void checkConsistency(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<org.neo4j.kernel.impl.store.record.RelationshipRecord, org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(valueFrom(relationship) < 0) { illegalNode(engine.report()); } else { org.neo4j.kernel.impl.store.record.NodeRecord node = new org.neo4j.kernel.impl.store.record.NodeRecord(valueFrom(relationship)); org.neo4j.consistency.checking.cache.CacheAccess_Client client = records.cacheAccess().client(); node.setInUse(client.getBooleanFromCache(node.getId(), SLOT_FIRST_IN_SOURCE)); node.setNextRel(client.getFromCache(node.getId(), SLOT_RELATIONSHIP_ID)); node.setCreated(); if(records.shouldCheck(node.getId(), org.neo4j.consistency.checking.full.MultiPassStore.NODES)) { engine.comparativeCheck(new org.neo4j.consistency.store.DirectRecordReference<>(node, records), this); } } } @Override public void checkReference(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, org.neo4j.kernel.impl.store.record.NodeRecord node, CheckerEngine<org.neo4j.kernel.impl.store.record.RelationshipRecord, org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(!node.inUse()) { nodeNotInUse(engine.report(), node); } else { if(isFirst(relationship)) { org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(node.getNextRel() != relationship.getId()) { node = ((org.neo4j.consistency.store.DirectRecordReference<org.neo4j.kernel.impl.store.record.NodeRecord>)records.node(node.getId())).record(); if(node.isDense()) { } else { noBackReference(engine.report(), node); } } else { if(relationship.getFirstNode() != relationship.getSecondNode()) { cacheAccess.putToCacheSingle(node.getId(), SLOT_FIRST_IN_TARGET, NEXT); } } } else { if(!hasRelationship(node)) { noChain(engine.report(), node); } } } } abstract void illegalNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report);
		 public bool hasRelationship( Neo4Net.Kernel.Impl.Store.Records.NodeRecord node ) { return node.getNextRel() != Neo4Net.Kernel.Impl.Store.Records.Record.NO_NEXT_RELATIONSHIP.intValue(); } public void checkConsistency(Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records) { if (valueFrom(relationship) < 0) { illegalNode(engine.report()); } else { Neo4Net.Kernel.Impl.Store.Records.NodeRecord node = new Neo4Net.Kernel.Impl.Store.Records.NodeRecord(valueFrom(relationship)); Neo4Net.Consistency.checking.cache.CacheAccess_Client client = records.cacheAccess().client(); node.setInUse(client.getBooleanFromCache(node.getId(), SLOT_FIRST_IN_SOURCE)); node.setNextRel(client.getFromCache(node.getId(), SLOT_RELATIONSHIP_ID)); node.setCreated(); if (records.shouldCheck(node.getId(), Neo4Net.Consistency.checking.full.MultiPassStore.NODES)) { engine.comparativeCheck(new Neo4Net.Consistency.store.DirectRecordReference<>(node, records), this); } } } public void checkReference(Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, Neo4Net.Kernel.Impl.Store.Records.NodeRecord node, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records) { if (!node.inUse()) { nodeNotInUse(engine.report(), node); } else { if (isFirst(relationship)) { Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (node.getNextRel() != relationship.getId()) { node = ((Neo4Net.Consistency.store.DirectRecordReference<Neo4Net.Kernel.Impl.Store.Records.NodeRecord>)records.node(node.getId())).record(); if (node.isDense()) { } else { noBackReference(engine.report(), node); } } else { if (relationship.getFirstNode() != relationship.getSecondNode()) { cacheAccess.putToCacheSingle(node.getId(), SLOT_FIRST_IN_TARGET, NEXT); } } } else { if (!hasRelationship(node)) { noChain(engine.report(), node); } } } } abstract void illegalNode(Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report);

		 internal abstract void nodeNotInUse( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Neo4Net.Kernel.Impl.Store.Records.NodeRecord node );

		 internal abstract void noBackReference( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Neo4Net.Kernel.Impl.Store.Records.NodeRecord node );

		 internal abstract void noChain( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Neo4Net.Kernel.Impl.Store.Records.NodeRecord node );

		 internal abstract void notFirstInChain( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

		public static IList<NodeField> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static NodeField valueOf( string name )
		{
			foreach ( NodeField enumInstance in NodeField.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}