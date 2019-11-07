﻿/*
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
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using Neo4Net.Consistency.Store;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using DynamicNodeLabels = Neo4Net.Kernel.impl.store.DynamicNodeLabels;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.sort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.union;

	public class NodeRecordCheck : PrimitiveRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs static NodeRecordCheck forSparseNodes(RecordField<Neo4Net.kernel.impl.store.record.NodeRecord,Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport>... additional)
		 internal static NodeRecordCheck ForSparseNodes( params RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] additional )
		 {
			  RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] basic = ArrayUtil.array<RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>>( LabelsField.Labels );
			  return new NodeRecordCheck( union( basic, additional ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs static NodeRecordCheck forDenseNodes(RecordField<Neo4Net.kernel.impl.store.record.NodeRecord,Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport>... additional)
		 internal static NodeRecordCheck ForDenseNodes( params RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] additional )
		 {
			  RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] basic = ArrayUtil.array<RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>>( RelationshipGroupField.NextGroup, LabelsField.Labels );
			  return new NodeRecordCheck( union( basic, additional ) );
		 }

		 public static NodeRecordCheck ToCheckNextRel()
		 {
			  return new NodeRecordCheck( RelationshipField.NextRel );
		 }

		 public static NodeRecordCheck ToCheckNextRelationshipGroup()
		 {
			  return new NodeRecordCheck( RelationshipGroupField.NextGroup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs NodeRecordCheck(RecordField<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport>... fields)
		 internal NodeRecordCheck( params RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] fields ) : base( fields )
		 {
		 }

		 public NodeRecordCheck() : this(RelationshipField.NextRel, LabelsField.Labels)
		 {
		 }

		 internal enum RelationshipGroupField
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: NEXT_GROUP { @Override public void checkConsistency(Neo4Net.kernel.impl.store.record.NodeRecord node, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, Neo4Net.consistency.store.RecordAccess records) { if(!Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.is(node.getNextRel())) { engine.comparativeCheck(records.relationshipGroup(node.getNextRel()), this); } } @Override public long valueFrom(Neo4Net.kernel.impl.store.record.NodeRecord node) { return node.getNextRel(); } @Override public void checkReference(Neo4Net.kernel.impl.store.record.NodeRecord record, Neo4Net.kernel.impl.store.record.RelationshipGroupRecord group, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, Neo4Net.consistency.store.RecordAccess records) { if(!group.inUse()) { engine.report().relationshipGroupNotInUse(group); } else { if(group.getOwningNode() != record.getId()) { engine.report().relationshipGroupHasOtherOwner(group); } } } }
			  NEXT_GROUP
			  {
				  public void checkConsistency( NodeRecord node, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
				  {
					  if ( !Record.NO_NEXT_RELATIONSHIP.@is( node.NextRel ) ) { engine.comparativeCheck( records.relationshipGroup( node.NextRel ), this ); }
				  }
				  public long valueFrom( NodeRecord node ) { return node.NextRel; } public void checkReference( NodeRecord record, RelationshipGroupRecord group, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
				  {
					  if ( !group.inUse() ) { engine.report().relationshipGroupNotInUse(group); } else
					  {
						  if ( group.OwningNode != record.Id ) { engine.report().relationshipGroupHasOtherOwner(group); }
					  }
				  }
			  }
		 }

		 enum RelationshipField implements RecordField<NodeRecord, ConsistencyReport.NodeConsistencyReport>, ComparativeRecordChecker<NodeRecord, RelationshipRecord, ConsistencyReport.NodeConsistencyReport>
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: NEXT_REL { @Override public void checkConsistency(Neo4Net.kernel.impl.store.record.NodeRecord node, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, Neo4Net.consistency.store.RecordAccess records) { if(!Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.is(node.getNextRel())) { engine.comparativeCheck(records.relationship(node.getNextRel()), this); } } @Override public void checkReference(Neo4Net.kernel.impl.store.record.NodeRecord node, Neo4Net.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, Neo4Net.consistency.store.RecordAccess records) { if(!relationship.inUse()) { engine.report().relationshipNotInUse(relationship); } else { NodeField selectedField = NodeField.select(relationship, node); if(selectedField == null) { engine.report().relationshipForOtherNode(relationship); } else { NodeField[] fields; if(relationship.getFirstNode() == relationship.getSecondNode()) { fields = NodeField.values(); } else { fields = new NodeField[]{selectedField}; } for(NodeField field : fields) { if(!field.isFirst(relationship)) { field.notFirstInChain(engine.report(), relationship); } } } } } @Override public long valueFrom(Neo4Net.kernel.impl.store.record.NodeRecord record) { return record.getNextRel(); } }
			  NEXT_REL
			  {
				  public void checkConsistency( NodeRecord node, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
				  {
					  if ( !Record.NO_NEXT_RELATIONSHIP.@is( node.NextRel ) ) { engine.comparativeCheck( records.relationship( node.NextRel ), this ); }
				  }
				  public void checkReference( NodeRecord node, RelationshipRecord relationship, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
				  {
					  if ( !relationship.inUse() ) { engine.report().relationshipNotInUse(relationship); } else
					  {
						  NodeField selectedField = NodeField.select( relationship, node ); if ( selectedField == null ) { engine.report().relationshipForOtherNode(relationship); } else
						  {
							  NodeField[] fields; if ( relationship.FirstNode == relationship.SecondNode ) { fields = NodeField.values(); } else
							  {
								  fields = new NodeField[]{ selectedField };
							  }
							  for ( NodeField field : fields )
							  {
								  if ( !field.isFirst( relationship ) ) { field.notFirstInChain( engine.report(), relationship ); }
							  }
						  }
					  }
				  }
				  public long valueFrom( NodeRecord record ) { return record.NextRel; }
			  }
		 }

		 enum LabelsField implements RecordField<NodeRecord, ConsistencyReport.NodeConsistencyReport>, ComparativeRecordChecker<NodeRecord, LabelTokenRecord, ConsistencyReport.NodeConsistencyReport>
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: LABELS { @Override public void checkConsistency(NodeRecord node, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records) { NodeLabels nodeLabels = NodeLabelsField.parseLabelsField(node); if(nodeLabels instanceof DynamicNodeLabels) { DynamicNodeLabels dynamicNodeLabels = (DynamicNodeLabels) nodeLabels; long firstRecordId = dynamicNodeLabels.getFirstDynamicRecordId(); RecordReference<DynamicRecord> firstRecordReference = records.nodeLabels(firstRecordId); engine.comparativeCheck(firstRecordReference, new LabelChainWalker<>(new NodeLabelsComparativeRecordChecker())); } else { validateLabelIds(nodeLabels.get(null), engine, records); } } private void validateLabelIds(long[] labelIds, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records) { for(long labelId : labelIds) { engine.comparativeCheck(records.label((int) labelId), this); } boolean outOfOrder = false; for(int i = 1; i < labelIds.length; i++) { if(labelIds[i - 1] > labelIds[i]) { engine.report().labelsOutOfOrder(labelIds[i - 1], labelIds[i]); outOfOrder = true; } } if(outOfOrder) { sort(labelIds); } for(int i = 1; i < labelIds.length; i++) { if(labelIds[i - 1] == labelIds[i]) { engine.report().labelDuplicate(labelIds[i]); } } } @Override public void checkReference(NodeRecord node, LabelTokenRecord labelTokenRecord, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records) { if(!labelTokenRecord.inUse()) { engine.report().labelNotInUse(labelTokenRecord); } } @Override public long valueFrom(NodeRecord record) { return record.getLabelField(); } class NodeLabelsComparativeRecordChecker implements LabelChainWalker.Validator<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> { @Override public void onRecordNotInUse(Neo4Net.kernel.impl.store.record.DynamicRecord dynamicRecord, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine) { engine.report().dynamicLabelRecordNotInUse(dynamicRecord); } @Override public void onRecordChainCycle(Neo4Net.kernel.impl.store.record.DynamicRecord record, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord, Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine) { engine.report().dynamicRecordChainCycle(record); } @Override public void onWellFormedChain(long[] labelIds, CheckerEngine<Neo4Net.kernel.impl.store.record.NodeRecord,Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, Neo4Net.consistency.store.RecordAccess records) { validateLabelIds(labelIds, engine, records); } } }
			  LABELS
			  {
				  public void checkConsistency( NodeRecord node, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records )
				  {
					  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node ); if ( nodeLabels is DynamicNodeLabels ) { DynamicNodeLabels dynamicNodeLabels = ( DynamicNodeLabels ) nodeLabels; long firstRecordId = dynamicNodeLabels.FirstDynamicRecordId; RecordReference<DynamicRecord> firstRecordReference = records.nodeLabels( firstRecordId ); engine.comparativeCheck( firstRecordReference, new LabelChainWalker<>( new NodeLabelsComparativeRecordChecker() ) ); } else { validateLabelIds(nodeLabels.get(null), engine, records); }
				  }
				  private void validateLabelIds( long[] labelIds, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records )
				  {
					  for ( long labelId : labelIds ) { engine.comparativeCheck( records.label( ( int ) labelId ), this ); } bool outOfOrder = false; for ( int i = 1; i < labelIds.length; i++ )
					  {
						  if ( labelIds[i - 1] > labelIds[i] ) { engine.report().labelsOutOfOrder(labelIds[i - 1], labelIds[i]); outOfOrder = true; }
					  }
					  if ( outOfOrder ) { sort( labelIds ); } for ( int i = 1; i < labelIds.length; i++ )
					  {
						  if ( labelIds[i - 1] == labelIds[i] ) { engine.report().labelDuplicate(labelIds[i]); }
					  }
				  }
				  public void checkReference( NodeRecord node, LabelTokenRecord labelTokenRecord, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records )
				  {
					  if ( !labelTokenRecord.inUse() ) { engine.report().labelNotInUse(labelTokenRecord); }
				  }
				  public long valueFrom( NodeRecord record ) { return record.LabelField; } class NodeLabelsComparativeRecordChecker implements LabelChainWalker.Validator<NodeRecord, ConsistencyReport_NodeConsistencyReport>
				  {
					  public void onRecordNotInUse( DynamicRecord dynamicRecord, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine ) { engine.report().dynamicLabelRecordNotInUse(dynamicRecord); } public void onRecordChainCycle(DynamicRecord record, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine) { engine.report().dynamicRecordChainCycle(record); } public void onWellFormedChain(long[] labelIds, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records) { validateLabelIds(labelIds, engine, records); }
				  }
			  }
		 }
	}

}