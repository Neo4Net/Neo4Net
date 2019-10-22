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
	using CacheAccess_Client = Neo4Net.Consistency.checking.cache.CacheAccess_Client;
	using MultiPassStore = Neo4Net.Consistency.checking.full.MultiPassStore;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using Counts = Neo4Net.Consistency.statistics.Counts;
	using Neo4Net.Consistency.store;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.NEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.PREV;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_REFERENCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_RELATIONSHIP_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SLOT_SOURCE_OR_TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.SOURCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.CacheSlots_RelationshipLink_Fields.TARGET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.union;

	public class RelationshipRecordCheck : PrimitiveRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>
	{
		 public RelationshipRecordCheck() : this(RelationshipTypeField.RelationshipType, NodeField.Source, RelationshipField.SourcePrev, RelationshipField.SourceNext, NodeField.Target, RelationshipField.TargetPrev, RelationshipField.TargetNext)
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs RelationshipRecordCheck(RecordField<org.Neo4Net.kernel.impl.store.record.RelationshipRecord,org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport>... fields)
		 internal RelationshipRecordCheck( params RecordField<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>[] fields ) : base( fields )
		 {
		 }

		 public static RelationshipRecordCheck RelationshipRecordCheckForwardPass()
		 {
			  return new RelationshipRecordCheck( RelationshipTypeField.RelationshipType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static RelationshipRecordCheck relationshipRecordCheckBackwardPass(RecordField<org.Neo4Net.kernel.impl.store.record.RelationshipRecord,org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport>... additional)
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
			  public void CheckConsistency( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord record, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
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

			  public long ValueFrom( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord record )
			  {
					return record.Type;
			  }

			  public void CheckReference( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord record, Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord referred, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
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
//           SOURCE_PREV(NodeField.SOURCE) { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstPrevRel(); } long other(NodeField field, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return field.next(relationship); } void otherNode(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.sourcePrevReferencesOtherNodes(relationship); } void noBackReference(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.sourcePrevDoesNotReferenceBack(relationship); } boolean endOfChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) { return NODE.isFirst(record); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.relSourcePrevCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SOURCE_NEXT(NodeField.SOURCE) { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getFirstNextRel(); } long other(NodeField field, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return field.prev(relationship); } void otherNode(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.sourceNextReferencesOtherNodes(relationship); } void noBackReference(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.sourceNextDoesNotReferenceBack(relationship); } boolean endOfChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) { return NODE.next(record) == org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.relSourceNextCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           TARGET_PREV(NodeField.TARGET) { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondPrevRel(); } long other(NodeField field, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return field.next(relationship); } void otherNode(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.targetPrevReferencesOtherNodes(relationship); } void noBackReference(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.targetPrevDoesNotReferenceBack(relationship); } boolean endOfChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) { return NODE.isFirst(record); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondNextRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.relTargetPrevCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           TARGET_NEXT(NodeField.TARGET) { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.getSecondNextRel(); } long other(NodeField field, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return field.prev(relationship); } void otherNode(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.targetNextReferencesOtherNodes(relationship); } void noBackReference(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { report.targetNextDoesNotReferenceBack(relationship); } boolean endOfChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) { return NODE.next(record) == org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } RelationshipRecord populateRelationshipFromCache(long nodeId, org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } else { rel.setSecondPrevRel(cacheAccess.getFromCache(nodeId, SLOT_REFERENCE)); } rel.setInUse(cacheAccess.getBooleanFromCache(nodeId, SLOT_IN_USE)); return rel; } void linkChecked(org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.relTargetNextCheck); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CACHE_VALUES(null) { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) { return 0; } boolean endOfChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record) { return false; } long other(NodeField field, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return 0; } void otherNode(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { } void noBackReference(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport report, org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { } RelationshipRecord populateRelationshipFromCache(long nodeId, org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { return null; } public void checkConsistency(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<org.Neo4Net.kernel.impl.store.record.RelationshipRecord, org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.Neo4Net.consistency.store.RecordAccess records) { if(!relationship.inUse()) { return; } org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); boolean cache1Free = cacheAccess.getFromCache(relationship.getFirstNode(), SLOT_RELATIONSHIP_ID) == -1; boolean cache2Free = cacheAccess.getFromCache(relationship.getSecondNode(), SLOT_RELATIONSHIP_ID) == -1; if(records.cacheAccess().isForward()) { if(cacheAccess.withinBounds(relationship.getFirstNode())) { cacheAccess.putToCache(relationship.getFirstNode(), relationship.getId(), relationship.getFirstPrevRel(), SOURCE, PREV, 1); updateCacheCounts(cache1Free, cacheAccess); } if(cacheAccess.withinBounds(relationship.getSecondNode())) { cacheAccess.putToCache(relationship.getSecondNode(), relationship.getId(), relationship.getSecondPrevRel(), TARGET, PREV, 1); updateCacheCounts(cache2Free, cacheAccess); } } else { if(cacheAccess.withinBounds(relationship.getFirstNode())) { cacheAccess.putToCache(relationship.getFirstNode(), relationship.getId(), relationship.getFirstNextRel(), SOURCE, NEXT, 1); updateCacheCounts(cache1Free, cacheAccess); } if(cacheAccess.withinBounds(relationship.getSecondNode())) { cacheAccess.putToCache(relationship.getSecondNode(), relationship.getId(), relationship.getSecondNextRel(), TARGET, NEXT, 1); updateCacheCounts(cache2Free, cacheAccess); } } } private void updateCacheCounts(boolean free, org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { if(!free) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.overwrite); } else { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.activeCache); } } void linkChecked(org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.relCacheCheck); } };

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
//ORIGINAL LINE: RelationshipField(NodeField node) { this.NODE = node; } private org.Neo4Net.consistency.store.RecordReference<org.Neo4Net.kernel.impl.store.record.RelationshipRecord> buildFromCache(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship, long reference, long nodeId, org.Neo4Net.consistency.store.RecordAccess records) { org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(!cacheAccess.withinBounds(nodeId)) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.correctSkipCheck); return org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference(); } if(reference != cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID)) { if(referenceShouldBeSkipped(relationship, reference, records)) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.correctSkipCheck); return org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference(); } cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.missCheck); return records.relationship(reference); } org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(reference); rel.setCreated(); if(cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNode(nodeId); } else { rel.setSecondNode(nodeId); } rel = populateRelationshipFromCache(nodeId, rel, cacheAccess); return new org.Neo4Net.consistency.store.DirectRecordReference<>(rel, records); } private boolean referenceShouldBeSkipped(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship, long reference, org.Neo4Net.consistency.store.RecordAccess records) { return(records.cacheAccess().isForward() && reference > relationship.getId()) || (!records.cacheAccess().isForward() && reference < relationship.getId()); } @Override public void checkConsistency(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<org.Neo4Net.kernel.impl.store.record.RelationshipRecord,org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.Neo4Net.consistency.store.RecordAccess records) { org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(!endOfChain(relationship)) { org.Neo4Net.consistency.store.RecordReference<org.Neo4Net.kernel.impl.store.record.RelationshipRecord> referred = null; long reference = valueFrom(relationship); long nodeId = -1; if(records.shouldCheck(reference, org.Neo4Net.consistency.checking.full.MultiPassStore.RELATIONSHIPS)) { nodeId = NODE == NodeField.SOURCE ? relationship.getFirstNode() : relationship.getSecondNode(); if(org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.is(cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID))) { referred = org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference(); cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.noCacheSkip); } else { referred = buildFromCache(relationship, reference, nodeId, records); if(referred == org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference<org.Neo4Net.kernel.impl.store.record.RelationshipRecord>()) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.skipCheck); } } } else { if(referenceShouldBeSkipped(relationship, reference, records)) { referred = org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference(); } } engine.comparativeCheck(referred, this); if(referred != org.Neo4Net.consistency.store.RecordReference_SkippingReference.skipReference<org.Neo4Net.kernel.impl.store.record.RelationshipRecord>()) { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.checked); linkChecked(cacheAccess); } } else { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.checked); linkChecked(cacheAccess); } } @Override public void checkReference(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record, org.Neo4Net.kernel.impl.store.record.RelationshipRecord referred, CheckerEngine<org.Neo4Net.kernel.impl.store.record.RelationshipRecord,org.Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, org.Neo4Net.consistency.store.RecordAccess records) { NodeField field = NodeField.select(referred, node(record)); if(field == null) { otherNode(engine.report(), referred); } else { org.Neo4Net.consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if(other(field, referred) != record.getId()) { if(referred.isCreated()) { org.Neo4Net.consistency.store.RecordReference<org.Neo4Net.kernel.impl.store.record.RelationshipRecord> refRel = records.relationship(referred.getId()); referred = (org.Neo4Net.kernel.impl.store.record.RelationshipRecord)((org.Neo4Net.consistency.store.DirectRecordReference) refRel).record(); checkReference(record, referred, engine, records); cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.skipBackup); } else { cacheAccess.incAndGetCount(org.Neo4Net.consistency.statistics.Counts_Type.checkErrors); noBackReference(engine == null ? null : engine.report(), referred); } } else { if(!referenceShouldBeSkipped(record, referred.getId(), records) && !referred.inUse()) { engine.report().notUsedRelationshipReferencedInChain(referred); } if(referred.isCreated()) { cacheAccess.clearCache(node(record)); } } } } abstract boolean endOfChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord record);
			  RelationshipField( NodeField node ) { this.NODE = node; } internal Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord> buildFromCache( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, long reference, long nodeId, Neo4Net.Consistency.store.RecordAccess records )
			  {
				  Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (!cacheAccess.withinBounds(nodeId)) { cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.correctSkipCheck); return Neo4Net.Consistency.store.RecordReference_SkippingReference.skipReference(); } if (reference != cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID))
				  {
					  if ( referenceShouldBeSkipped( relationship, reference, records ) ) { cacheAccess.incAndGetCount( Neo4Net.Consistency.statistics.Counts_Type.correctSkipCheck ); return Neo4Net.Consistency.store.RecordReference_SkippingReference.skipReference(); } cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.missCheck); return records.relationship(reference);
				  }
				  Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel = new Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord( reference ); rel.setCreated(); if (cacheAccess.getFromCache(nodeId, SLOT_SOURCE_OR_TARGET) == SOURCE) { rel.setFirstNode(nodeId); } else { rel.setSecondNode(nodeId); } rel = populateRelationshipFromCache(nodeId, rel, cacheAccess); return new Neo4Net.Consistency.store.DirectRecordReference<>(rel, records);
			  }
			  private bool referenceShouldBeSkipped( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, long reference, Neo4Net.Consistency.store.RecordAccess records ) { return( records.cacheAccess().isForward() && reference > relationship.getId() ) || (!records.cacheAccess().isForward() && reference < relationship.getId()); } public void checkConsistency(Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records)
			  {
				  Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (!endOfChain(relationship))
				  {
					  Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord> referred = null; long reference = valueFrom( relationship ); long nodeId = -1; if ( records.shouldCheck( reference, Neo4Net.Consistency.checking.full.MultiPassStore.RELATIONSHIPS ) )
					  {
						  nodeId = NODE == NodeField.SOURCE ? relationship.getFirstNode() : relationship.getSecondNode(); if (Neo4Net.Kernel.Impl.Store.Records.Record.NO_NEXT_RELATIONSHIP.@is(cacheAccess.getFromCache(nodeId, SLOT_RELATIONSHIP_ID))) { referred = Neo4Net.Consistency.store.RecordReference_SkippingReference.skipReference(); cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.noCacheSkip); } else
						  {
							  referred = buildFromCache( relationship, reference, nodeId, records ); if ( referred == Neo4Net.Consistency.store.RecordReference_SkippingReference.skipReference<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord>() ) { cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.skipCheck); }
						  }
					  }
					  else
					  {
						  if ( referenceShouldBeSkipped( relationship, reference, records ) ) { referred = Neo4Net.Consistency.store.RecordReference_SkippingReference.skipReference(); }
					  }
					  engine.comparativeCheck( referred, this ); if ( referred != Neo4Net.Consistency.store.RecordReference_SkippingReference.skipReference<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord>() ) { cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.@checked); linkChecked(cacheAccess); }
				  }
				  else { cacheAccess.incAndGetCount( Neo4Net.Consistency.statistics.Counts_Type.@checked ); linkChecked( cacheAccess ); }
			  }
			  public void checkReference( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord record, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord referred, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
				  NodeField field = NodeField.select( referred, node( record ) ); if ( field == null ) { otherNode( engine.report(), referred ); } else
				  {
					  Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.cacheAccess().client(); if (other(field, referred) != record.getId())
					  {
						  if ( referred.isCreated() ) { Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord> refRel = records.relationship(referred.getId()); referred = (Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord)((Neo4Net.Consistency.store.DirectRecordReference) refRel).record(); checkReference(record, referred, engine, records); cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.skipBackup); } else { cacheAccess.incAndGetCount(Neo4Net.Consistency.statistics.Counts_Type.checkErrors); noBackReference(engine == null ? null : engine.report(), referred); }
					  }
					  else
					  {
						  if ( !referenceShouldBeSkipped( record, referred.getId(), records ) && !referred.inUse() ) { engine.report().notUsedRelationshipReferencedInChain(referred); } if (referred.isCreated()) { cacheAccess.clearCache(node(record)); }
					  }
				  }
			  }
			  abstract bool endOfChain( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord record );

			  internal abstract long other( NodeField field, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

			  internal abstract void otherNode( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

			  internal abstract void noBackReference( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship );

			  internal abstract Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord populateRelationshipFromCache( long nodeId, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel, Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess );

			  internal abstract void linkChecked( Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess );

			  internal long Node( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord relationship )
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