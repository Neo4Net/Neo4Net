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
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Consistency.store;
	using ArrayUtil = Org.Neo4j.Helpers.ArrayUtil;
	using DynamicNodeLabels = Org.Neo4j.Kernel.impl.store.DynamicNodeLabels;
	using NodeLabels = Org.Neo4j.Kernel.impl.store.NodeLabels;
	using NodeLabelsField = Org.Neo4j.Kernel.impl.store.NodeLabelsField;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.sort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.union;

	public class NodeRecordCheck : PrimitiveRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs static NodeRecordCheck forSparseNodes(RecordField<org.neo4j.kernel.impl.store.record.NodeRecord,org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport>... additional)
		 internal static NodeRecordCheck ForSparseNodes( params RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] additional )
		 {
			  RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] basic = ArrayUtil.array<RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>>( LabelsField.Labels );
			  return new NodeRecordCheck( union( basic, additional ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs static NodeRecordCheck forDenseNodes(RecordField<org.neo4j.kernel.impl.store.record.NodeRecord,org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport>... additional)
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
//ORIGINAL LINE: @SafeVarargs NodeRecordCheck(RecordField<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport>... fields)
		 internal NodeRecordCheck( params RecordField<NodeRecord, ConsistencyReport_NodeConsistencyReport>[] fields ) : base( fields )
		 {
		 }

		 public NodeRecordCheck() : this(RelationshipField.NextRel, LabelsField.Labels)
		 {
		 }

		 internal enum RelationshipGroupField
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: NEXT_GROUP { @Override public void checkConsistency(org.neo4j.kernel.impl.store.record.NodeRecord node, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(!org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.is(node.getNextRel())) { engine.comparativeCheck(records.relationshipGroup(node.getNextRel()), this); } } @Override public long valueFrom(org.neo4j.kernel.impl.store.record.NodeRecord node) { return node.getNextRel(); } @Override public void checkReference(org.neo4j.kernel.impl.store.record.NodeRecord record, org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(!group.inUse()) { engine.report().relationshipGroupNotInUse(group); } else { if(group.getOwningNode() != record.getId()) { engine.report().relationshipGroupHasOtherOwner(group); } } } }
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
//ORIGINAL LINE: NEXT_REL { @Override public void checkConsistency(org.neo4j.kernel.impl.store.record.NodeRecord node, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(!org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.is(node.getNextRel())) { engine.comparativeCheck(records.relationship(node.getNextRel()), this); } } @Override public void checkReference(org.neo4j.kernel.impl.store.record.NodeRecord node, org.neo4j.kernel.impl.store.record.RelationshipRecord relationship, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { if(!relationship.inUse()) { engine.report().relationshipNotInUse(relationship); } else { NodeField selectedField = NodeField.select(relationship, node); if(selectedField == null) { engine.report().relationshipForOtherNode(relationship); } else { NodeField[] fields; if(relationship.getFirstNode() == relationship.getSecondNode()) { fields = NodeField.values(); } else { fields = new NodeField[]{selectedField}; } for(NodeField field : fields) { if(!field.isFirst(relationship)) { field.notFirstInChain(engine.report(), relationship); } } } } } @Override public long valueFrom(org.neo4j.kernel.impl.store.record.NodeRecord record) { return record.getNextRel(); } }
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
//ORIGINAL LINE: LABELS { @Override public void checkConsistency(NodeRecord node, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records) { NodeLabels nodeLabels = NodeLabelsField.parseLabelsField(node); if(nodeLabels instanceof DynamicNodeLabels) { DynamicNodeLabels dynamicNodeLabels = (DynamicNodeLabels) nodeLabels; long firstRecordId = dynamicNodeLabels.getFirstDynamicRecordId(); RecordReference<DynamicRecord> firstRecordReference = records.nodeLabels(firstRecordId); engine.comparativeCheck(firstRecordReference, new LabelChainWalker<>(new NodeLabelsComparativeRecordChecker())); } else { validateLabelIds(nodeLabels.get(null), engine, records); } } private void validateLabelIds(long[] labelIds, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records) { for(long labelId : labelIds) { engine.comparativeCheck(records.label((int) labelId), this); } boolean outOfOrder = false; for(int i = 1; i < labelIds.length; i++) { if(labelIds[i - 1] > labelIds[i]) { engine.report().labelsOutOfOrder(labelIds[i - 1], labelIds[i]); outOfOrder = true; } } if(outOfOrder) { sort(labelIds); } for(int i = 1; i < labelIds.length; i++) { if(labelIds[i - 1] == labelIds[i]) { engine.report().labelDuplicate(labelIds[i]); } } } @Override public void checkReference(NodeRecord node, LabelTokenRecord labelTokenRecord, CheckerEngine<NodeRecord, ConsistencyReport.NodeConsistencyReport> engine, RecordAccess records) { if(!labelTokenRecord.inUse()) { engine.report().labelNotInUse(labelTokenRecord); } } @Override public long valueFrom(NodeRecord record) { return record.getLabelField(); } class NodeLabelsComparativeRecordChecker implements LabelChainWalker.Validator<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> { @Override public void onRecordNotInUse(org.neo4j.kernel.impl.store.record.DynamicRecord dynamicRecord, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine) { engine.report().dynamicLabelRecordNotInUse(dynamicRecord); } @Override public void onRecordChainCycle(org.neo4j.kernel.impl.store.record.DynamicRecord record, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord, org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine) { engine.report().dynamicRecordChainCycle(record); } @Override public void onWellFormedChain(long[] labelIds, CheckerEngine<org.neo4j.kernel.impl.store.record.NodeRecord,org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport> engine, org.neo4j.consistency.store.RecordAccess records) { validateLabelIds(labelIds, engine, records); } } }
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