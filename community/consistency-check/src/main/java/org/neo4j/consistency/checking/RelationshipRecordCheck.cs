﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Consistency.checking
{
	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using CacheAccess_Client = Org.Neo4j.Consistency.checking.cache.CacheAccess_Client;
	using MultiPassStore = Org.Neo4j.Consistency.checking.full.MultiPassStore;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using Counts = Org.Neo4j.Consistency.statistics.Counts;
	using Org.Neo4j.Consistency.store;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Consistency.store;
	using ArrayUtil = Org.Neo4j.Helpers.ArrayUtil;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.NEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.PREV;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_REFERENCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_RELATIONSHIP_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_SOURCE_OR_TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SOURCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.union;

	public class RelationshipRecordCheck : PrimitiveRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>
	{
		 public RelationshipRecordCheck() : this(RelationshipTypeField.RelationshipType, NodeField.Source, RelationshipField.SourcePrev, RelationshipField.SourceNext, NodeField.Target, RelationshipField.TargetPrev, RelationshipField.TargetNext)
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs RelationshipRecordCheck(RecordField<org.neo4j.kernel.impl.store.record.RelationshipRecord,org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport>... fields)
		 internal RelationshipRecordCheck( params RecordField<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>[] fields ) : base( fields )
		 {
		 }

		 public static RelationshipRecordCheck RelationshipRecordCheckForwardPass()
		 {
			  return new RelationshipRecordCheck( RelationshipTypeField.RelationshipType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static RelationshipRecordCheck relationshipRecordCheckBackwardPass(RecordField<org.neo4j.kernel.impl.store.record.RelationshipRecord,org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport>... additional)
		 public static RelationshipRecordCheck RelationshipRecordCheckBackwardPass( params RecordField<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>[] additional )
		 {
			  return new RelationshipRecordCheck( union( ArrayUtil.array<RecordField<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>>( NodeField.Source, NodeField.Target ), additional ) );
		 }

		 public static RelationshipRecordCheck RelationshipRecordCheckSourceChain()
		 {
			  return new RelationshipRecordCheck( RelationshipField.SourceNext, RelationshipField.SourcePrev, RelationshipField.TargetNext, RelationshipField.TargetPrev, RelationshipField.CacheValues );
		 }

		 internal sealed class RelationshipTypeField : RecordField<RelationshipRecord, ConsistencyReport.RelationshipConsistencyReport>, ComparativeRecordChecker<RelationshipRecord, RelationshipTypeTokenRecord, ConsistencyReport.RelationshipConsistencyReport>
		 {
			  public static readonly RelationshipTypeField RelationshipType = new RelationshipTypeField( "RelationshipType", InnerEnum.RelationshipType );

			  private static readonly IList<RelationshipTypeField> valueList = new List<RelationshipTypeField>();

			  static RelationshipTypeField()
			  {
				  valueList.Add( RelationshipType );
			  }

			  public enum InnerEnum
			  {
				  RelationshipType
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private RelationshipTypeField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  public void CheckConsistency( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord record, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records )
			  {
					if ( record.Type < 0 )
					{
						 engine.Report().illegalRelationshipType();
					}
					else
					{
						 engine.ComparativeCheck( records.RelationshipType( record.Type ), this );
					}
			  }

			  public long ValueFrom( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord record )
			  {
					return record.Type;
			  }

			  public void CheckReference( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord record, Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord referred, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records )
			  {
					if ( !referred.InUse() )
					{
						 engine.Report().relationshipTypeNotInUse(referred);
					}
			  }

			 public static IList<RelationshipTypeField> values()
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

			 public static RelationshipTypeField valueOf( string name )
			 {
				 foreach ( RelationshipTypeField enumInstance in RelationshipTypeField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal abstract class RelationshipField : RecordField<RelationshipRecord, ConsistencyReport.RelationshipConsistencyReport>, ComparativeRecordChecker<RelationshipRecord, RelationshipRecord, ConsistencyReport.RelationshipConsistencyReport>
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SOURCE_PREV(NodeField.SOURCE) { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstPrevRel(); } long other(NodeField field, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return field.next(relationship); } void otherNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.sourcePrevReferencesOtherNodes(relationship); } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.sourcePrevDoesNotReferenceBack(relationship); } boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord record) { return NODE.isFirst(record); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.neo4j.kernel.impl.store.record.RelationshipRecord rel, org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.relSourcePrevCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SOURCE_NEXT(NodeField.SOURCE) { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstNextRel(); } long other(NodeField field, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return field.prev(relationship); } void otherNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.sourceNextReferencesOtherNodes(relationship); } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.sourceNextDoesNotReferenceBack(relationship); } boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord record) { return NODE.next(record) == org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.neo4j.kernel.impl.store.record.RelationshipRecord rel, org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.relSourceNextCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           TARGET_PREV(NodeField.TARGET) { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondPrevRel(); } long other(NodeField field, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return field.next(relationship); } void otherNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.targetPrevReferencesOtherNodes(relationship); } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.targetPrevDoesNotReferenceBack(relationship); } boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord record) { return NODE.isFirst(record); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.neo4j.kernel.impl.store.record.RelationshipRecord rel, org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.relTargetPrevCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           TARGET_NEXT(NodeField.TARGET) { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondNextRel(); } long other(NodeField field, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return field.prev(relationship); } void otherNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.targetNextReferencesOtherNodes(relationship); } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { report.targetNextDoesNotReferenceBack(relationship); } boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord record) { return NODE.next(record) == org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.neo4j.kernel.impl.store.record.RelationshipRecord rel, org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.relTargetNextCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CACHE_VALUES(null) { public long valueFrom(org.neo4j.kernel.impl.store.record.RelationshipRecord record) { return 0; } boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord record) { return false; } long other(NodeField field, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { return 0; } void otherNode(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { } void noBackReference(org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship) { } RelationshipRecord populateRelationshipFromCache(long nodeId, org.neo4j.kernel.impl.store.record.RelationshipRecord rel, org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { return null; } public void checkConsistency(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<org.neo4j.kernel.impl.store.record.RelationshipRecord, org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(!relationship.inUse()) { return; } org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); boolean cache1Free = cacheAccess.getFromCache(relationship.getFirstNode(), SLOT_RELATIONSHIP_ID) == -1; boolean cache2Free = cacheAccess.getFromCache(relationship.getSecondNode(), SLOT_RELATIONSHIP_ID) == -1; if(records.cacheAccess().isForward()) { if(cacheAccess.withinBounds(relationship.getFirstNode())) { cacheAccess.putToCache(relationship.getFirstNode(), relationship.getId(), relationship.getFirstPrevRel(), SOURCE, PREV, 1); updateCacheCounts(cache1Free, cacheAccess); } if(cacheAccess.withinBounds(relationship.getSecondNode())) { cacheAccess.putToCache(relationship.getSecondNode(), relationship.getId(), relationship.getSecondPrevRel(), TARGET, PREV, 1); updateCacheCounts(cache2Free, cacheAccess); } } else { if(cacheAccess.withinBounds(relationship.getFirstNode())) { cacheAccess.putToCache(relationship.getFirstNode(), relationship.getId(), relationship.getFirstNextRel(), SOURCE, NEXT, 1); updateCacheCounts(cache1Free, cacheAccess); } if(cacheAccess.withinBounds(relationship.getSecondNode())) { cacheAccess.putToCache(relationship.getSecondNode(), relationship.getId(), relationship.getSecondNextRel(), TARGET, NEXT, 1); updateCacheCounts(cache2Free, cacheAccess); } } } private void updateCacheCounts(boolean free, org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(!free) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.overwrite); } else { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.activeCache); } } void linkChecked(org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.relCacheCheck); } };

			  private static readonly IList<RelationshipField> valueList = new List<RelationshipField>();

			  static RelationshipField()
			  {
				  valueList.Add( SOURCE_PREV );
				  valueList.Add( SOURCE_NEXT );
				  valueList.Add( TARGET_PREV );
				  valueList.Add( TARGET_NEXT );
				  valueList.Add( CACHE_VALUES );
			  }

			  public enum InnerEnum
			  {
				  SOURCE_PREV,
				  SOURCE_NEXT,
				  TARGET_PREV,
				  TARGET_NEXT,
				  CACHE_VALUES
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private RelationshipField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal readonly NodeField NodeConflict;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: RelationshipField(NodeField node) { this.NODE = node; } private org.neo4j.consistency.store.RecordReference<org.neo4j.kernel.impl.store.record.RelationshipRecord> buildFromCache(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, long reference, long nodeId, org.neo4j.consistency.store.RecordAccess records) { org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(!cacheAccess.withinBounds(nodeId)) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.correctSkipCheck); return org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference(); } if(reference != cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID)) { if(referenceShouldBeSkipped(relationship, reference, records)) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.correctSkipCheck); return org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference(); } cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.missCheck); return records.relationship(reference); } org.neo4j.kernel.impl.store.record.RelationshipRecord rel = new org.neo4j.kernel.impl.store.record.RelationshipRecord(reference); rel.setCreated(); if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNode(nodeId); } else { rel.setSecondNode(nodeId); } rel = populateRelationshipFromCache(nodeId, rel, cacheAccess); return new org.neo4j.consistency.store.DirectRecordReference<>(rel, records); } private boolean referenceShouldBeSkipped(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, long reference, org.neo4j.consistency.store.RecordAccess records) { return(records.cacheAccess().isForward() && reference > relationship.getId()) || (!records.cacheAccess().isForward() && reference < relationship.getId()); } @Override public void checkConsistency(org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<org.neo4j.kernel.impl.store.record.RelationshipRecord,org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(!endOfChain(relationship)) { org.neo4j.consistency.store.RecordReference<org.neo4j.kernel.impl.store.record.RelationshipRecord> referred = null; long reference = valueFrom(relationship); long nodeId = -1; if(records.shouldCheck(reference, org.neo4j.consistency.checking.full.MultiPassStore.RELATIONSHIPS)) { nodeId = NODE == NodeField.SOURCE ? relationship.getFirstNode() : relationship.getSecondNode(); if(org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.is(cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID))) { referred = org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference(); cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.noCacheSkip); } else { referred = buildFromCache(relationship, reference, nodeId, records); if(referred == org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference<org.neo4j.kernel.impl.store.record.RelationshipRecord>()) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.skipCheck); } } } else { if(referenceShouldBeSkipped(relationship, reference, records)) { referred = org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference(); } } engine.comparativeCheck(referred, this); if(referred != org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference<org.neo4j.kernel.impl.store.record.RelationshipRecord>()) { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.checked); linkChecked(cacheAccess); } } else { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.checked); linkChecked(cacheAccess); } } @Override public void checkReference(org.neo4j.kernel.impl.store.record.RelationshipRecord record, org.neo4j.kernel.impl.store.record.RelationshipRecord referred, CheckerEngine<org.neo4j.kernel.impl.store.record.RelationshipRecord,org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { NodeField field = NodeField.select(referred, node(record)); if(field == null) { otherNode(engine.report(), referred); } else { org.neo4j.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(other(field, referred) != record.getId()) { if(referred.isCreated()) { org.neo4j.consistency.store.RecordReference<org.neo4j.kernel.impl.store.record.RelationshipRecord> refRel = records.relationship(referred.getId()); referred = (org.neo4j.kernel.impl.store.record.RelationshipRecord)((org.neo4j.consistency.store.DirectRecordReference) refRel).record(); checkReference(record, referred, engine, records); cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.skipBackup); } else { cacheAccess.incAndGetCount(org.neo4j.consistency.statistics.Counts_Type.checkErrors); noBackReference(engine == null ? null : engine.report(), referred); } } else { if(!referenceShouldBeSkipped(record, referred.getId(), records) && !referred.inUse()) { engine.report().notUsedRelationshipReferencedInChain(referred); } if(referred.isCreated()) { cacheAccess.clearCache(node(record)); } } } } abstract boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord record);
			  RelationshipField( NodeField node ) { this.NODE = node; } internal Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord> buildFromCache( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship, long reference, long nodeId, Org.Neo4j.Consistency.store.RecordAccess records )
			  {
				  Org.Neo4j.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (!cacheAccess.withinBounds(nodeId)) { cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.correctSkipCheck); return Org.Neo4j.Consistency.store.RecordReference_SkippingReference.skipReference(); } if (reference != cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID))
				  {
					  if ( referenceShouldBeSkipped( relationship, reference, records ) ) { cacheAccess.incAndGetCount( Org.Neo4j.Consistency.statistics.Counts_Type.correctSkipCheck ); return Org.Neo4j.Consistency.store.RecordReference_SkippingReference.skipReference(); } cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.missCheck); return records.relationship(reference);
				  }
				  Org.Neo4j.Kernel.impl.store.record.RelationshipRecord rel = new Org.Neo4j.Kernel.impl.store.record.RelationshipRecord( reference ); rel.setCreated(); if (cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNode(nodeId); } else { rel.setSecondNode(nodeId); } rel = populateRelationshipFromCache(nodeId, rel, cacheAccess); return new Org.Neo4j.Consistency.store.DirectRecordReference<>(rel, records);
			  }
			  private bool referenceShouldBeSkipped( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship, long reference, Org.Neo4j.Consistency.store.RecordAccess records ) { return( records.cacheAccess().isForward() && reference > relationship.getId() ) || (!records.cacheAccess().isForward() && reference < relationship.getId()); } public void checkConsistency(Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records)
			  {
				  Org.Neo4j.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (!endOfChain(relationship))
				  {
					  Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord> referred = null; long reference = valueFrom( relationship ); long nodeId = -1; if ( records.shouldCheck( reference, Org.Neo4j.Consistency.checking.full.MultiPassStore.RELATIONSHIPS ) )
					  {
						  nodeId = NODE == NodeField.SOURCE ? relationship.getFirstNode() : relationship.getSecondNode(); if (Org.Neo4j.Kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.@is(cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID))) { referred = Org.Neo4j.Consistency.store.RecordReference_SkippingReference.skipReference(); cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.noCacheSkip); } else
						  {
							  referred = buildFromCache( relationship, reference, nodeId, records ); if ( referred == Org.Neo4j.Consistency.store.RecordReference_SkippingReference.skipReference<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord>() ) { cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.skipCheck); }
						  }
					  }
					  else
					  {
						  if ( referenceShouldBeSkipped( relationship, reference, records ) ) { referred = Org.Neo4j.Consistency.store.RecordReference_SkippingReference.skipReference(); }
					  }
					  engine.comparativeCheck( referred, this ); if ( referred != Org.Neo4j.Consistency.store.RecordReference_SkippingReference.skipReference<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord>() ) { cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.@checked); linkChecked(cacheAccess); }
				  }
				  else { cacheAccess.incAndGetCount( Org.Neo4j.Consistency.statistics.Counts_Type.@checked ); linkChecked( cacheAccess ); }
			  }
			  public void checkReference( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord record, Org.Neo4j.Kernel.impl.store.record.RelationshipRecord referred, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records )
			  {
				  NodeField field = NodeField.select( referred, node( record ) ); if ( field == null ) { otherNode( engine.report(), referred ); } else
				  {
					  Org.Neo4j.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (other(field, referred) != record.getId())
					  {
						  if ( referred.isCreated() ) { Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord> refRel = records.relationship(referred.getId()); referred = (Org.Neo4j.Kernel.impl.store.record.RelationshipRecord)((Org.Neo4j.Consistency.store.DirectRecordReference) refRel).record(); checkReference(record, referred, engine, records); cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.skipBackup); } else { cacheAccess.incAndGetCount(Org.Neo4j.Consistency.statistics.Counts_Type.checkErrors); noBackReference(engine == null ? null : engine.report(), referred); }
					  }
					  else
					  {
						  if ( !referenceShouldBeSkipped( record, referred.getId(), records ) && !referred.inUse() ) { engine.report().notUsedRelationshipReferencedInChain(referred); } if (referred.isCreated()) { cacheAccess.clearCache(node(record)); }
					  }
				  }
			  }
			  abstract bool endOfChain( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord record );

			  internal abstract long other( NodeField field, Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship );

			  internal abstract void otherNode( Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship );

			  internal abstract void noBackReference( Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship );

			  internal abstract Org.Neo4j.Kernel.impl.store.record.RelationshipRecord populateRelationshipFromCache( long nodeId, Org.Neo4j.Kernel.impl.store.record.RelationshipRecord rel, Org.Neo4j.Consistency.checking.cache.CacheAccess_Client cacheAccess );

			  internal abstract void linkChecked( Org.Neo4j.Consistency.checking.cache.CacheAccess_Client cacheAccess );

			  internal long Node( Org.Neo4j.Kernel.impl.store.record.RelationshipRecord relationship )
			  {
					return NodeConflict.valueFrom( relationship );
			  }

			 public static IList<RelationshipField> values()
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

			 public static RelationshipField valueOf( string name )
			 {
				 foreach ( RelationshipField enumInstance in RelationshipField.valueList )
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

}