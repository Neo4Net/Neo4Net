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
namespace Org.Neo4j.Consistency.report
{

	using Org.Neo4j.Consistency.checking;
	using CountsEntry = Org.Neo4j.Consistency.store.synthetic.CountsEntry;
	using IndexEntry = Org.Neo4j.Consistency.store.synthetic.IndexEntry;
	using LabelScanDocument = Org.Neo4j.Consistency.store.synthetic.LabelScanDocument;
	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

	public interface ConsistencyReport
	{
	}

	 public class ConsistencyReport_Warning : System.Attribute
	 {
	 }

	 public interface ConsistencyReport_Reporter
	 {
		  void ForSchema( DynamicRecord schema, RecordCheck<DynamicRecord, ConsistencyReport_SchemaConsistencyReport> checker );

		  void ForNode( NodeRecord node, RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker );

		  void ForRelationship( RelationshipRecord relationship, RecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker );

		  void ForProperty( PropertyRecord property, RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker );

		  void ForRelationshipTypeName( RelationshipTypeTokenRecord relationshipType, RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker );

		  void ForLabelName( LabelTokenRecord label, RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker );

		  void ForPropertyKey( PropertyKeyTokenRecord key, RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker );

		  void ForDynamicBlock( RecordType type, DynamicRecord record, RecordCheck<DynamicRecord, ConsistencyReport_DynamicConsistencyReport> checker );

		  void ForDynamicLabelBlock( RecordType type, DynamicRecord record, RecordCheck<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport> checker );

		  void ForNodeLabelScan( LabelScanDocument document, RecordCheck<LabelScanDocument, ConsistencyReport_LabelScanConsistencyReport> checker );

		  void ForIndexEntry( IndexEntry entry, RecordCheck<IndexEntry, ConsistencyReport_IndexConsistencyReport> checker );

		  void ForRelationshipGroup( RelationshipGroupRecord record, RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker );

		  void ForCounts( CountsEntry countsEntry, RecordCheck<CountsEntry, ConsistencyReport_CountsConsistencyReport> checker );
	 }

	 public interface ConsistencyReport_PrimitiveConsistencyReport : ConsistencyReport
	 {
		  [Documented("The referenced property record is not in use.")]
		  void PropertyNotInUse( PropertyRecord property );

		  [Documented("The referenced property record is not the first in its property chain.")]
		  void PropertyNotFirstInChain( PropertyRecord property );

		  [Documented("The referenced property is owned by another Node.")]
		  void MultipleOwners( NodeRecord node );

		  [Documented("The referenced property is owned by another Relationship.")]
		  void MultipleOwners( RelationshipRecord relationship );

		  [Documented("The referenced property is owned by the neo store (graph global property).")]
		  void MultipleOwners( NeoStoreRecord neoStore );

		  [Documented("The property chain contains multiple properties that have the same property key id, " + "which means that the entity has at least one duplicate property.")]
		  void PropertyKeyNotUniqueInChain();

		  [Documented("The property chain does not contain a property that is mandatory for this entity.")]
		  void MissingMandatoryProperty( int key );

		  [Documented("The property record points to a previous record in the chain, making it a circular reference.")]
		  void PropertyChainContainsCircularReference( PropertyRecord propertyRecord );
	 }

	 public interface ConsistencyReport_NeoStoreConsistencyReport : ConsistencyReport_PrimitiveConsistencyReport
	 {
	 }

	 public interface ConsistencyReport_SchemaConsistencyReport : ConsistencyReport
	 {
		  [Documented("The label token record referenced from the schema is not in use.")]
		  void LabelNotInUse( LabelTokenRecord label );

		  [Documented("The relationship type token record referenced from the schema is not in use.")]
		  void RelationshipTypeNotInUse( RelationshipTypeTokenRecord relationshipType );

		  [Documented("The property key token record is not in use.")]
		  void PropertyKeyNotInUse( PropertyKeyTokenRecord propertyKey );

		  [Documented("The uniqueness constraint does not reference back to the given record")]
		  void UniquenessConstraintNotReferencingBack( DynamicRecord ruleRecord );

		  [Documented("The constraint index does not reference back to the given record")]
		  void ConstraintIndexRuleNotReferencingBack( DynamicRecord ruleRecord );

		  [Documented("This record is required to reference some other record of the given kind but no such obligation " + "was found")]
		  void MissingObligation( Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind kind );

		  [Documented("This record requires some other record to reference back to it but there already was such " + "a conflicting obligation created by the record given as a parameter")]
		  void DuplicateObligation( DynamicRecord record );

		  [Documented("This record contains a schema rule which has the same content as the schema rule contained " + "in the record given as parameter")]
		  void DuplicateRuleContent( DynamicRecord record );

		  [Documented("The schema rule contained in the DynamicRecord chain is malformed (not deserializable)")]
		  void MalformedSchemaRule();

		  [Documented("The schema rule contained in the DynamicRecord chain is of an unrecognized Kind")]
		  void UnsupportedSchemaRuleKind( Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind kind );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("The schema rule contained in the DynamicRecord chain has a reference to a schema rule that is not online.") void schemaRuleNotOnline(org.neo4j.storageengine.api.schema.SchemaRule schemaRule);
		  [Documented("The schema rule contained in the DynamicRecord chain has a reference to a schema rule that is not online.")]
		  void SchemaRuleNotOnline( SchemaRule schemaRule );
	 }

	 public interface ConsistencyReport_NodeConsistencyReport : ConsistencyReport_PrimitiveConsistencyReport
	 {
		  [Documented("The referenced relationship record is not in use.")]
		  void RelationshipNotInUse( RelationshipRecord referenced );

		  [Documented("The referenced relationship record is a relationship between two other nodes.")]
		  void RelationshipForOtherNode( RelationshipRecord relationship );

		  [Documented("The referenced relationship record is not the first in the relationship chain where this node " + "is source.")]
		  void RelationshipNotFirstInSourceChain( RelationshipRecord relationship );

		  [Documented("The referenced relationship record is not the first in the relationship chain where this node " + "is target.")]
		  void RelationshipNotFirstInTargetChain( RelationshipRecord relationship );

		  [Documented("The label token record referenced from a node record is not in use.")]
		  void LabelNotInUse( LabelTokenRecord label );

		  [Documented("The label token record is referenced twice from the same node.")]
		  void LabelDuplicate( long labelId );

		  [Documented("The label id array is not ordered")]
		  void LabelsOutOfOrder( long largest, long smallest );

		  [Documented("The dynamic label record is not in use.")]
		  void DynamicLabelRecordNotInUse( DynamicRecord record );

		  [Documented("This record points to a next record that was already part of this dynamic record chain.")]
		  void DynamicRecordChainCycle( DynamicRecord nextRecord );

		  [Documented("This node was not found in the expected index.")]
		  void NotIndexed( StoreIndexDescriptor index, object[] propertyValues );

		  [Documented("This node was found in the expected index, although multiple times")]
		  void IndexedMultipleTimes( StoreIndexDescriptor index, object[] propertyValues, long count );

		  [Documented("There is another node in the unique index with the same property value(s).")]
		  void UniqueIndexNotUnique( StoreIndexDescriptor index, object[] propertyValues, long duplicateNodeId );

		  [Documented("The referenced relationship group record is not in use.")]
		  void RelationshipGroupNotInUse( RelationshipGroupRecord group );

		  [Documented("The first relationship group record has another node set as owner.")]
		  void RelationshipGroupHasOtherOwner( RelationshipGroupRecord group );
	 }

	 public interface ConsistencyReport_RelationshipConsistencyReport : ConsistencyReport_PrimitiveConsistencyReport
	 {
		  [Documented("The relationship record is not in use, but referenced from relationships chain.")]
		  void NotUsedRelationshipReferencedInChain( RelationshipRecord relationshipRecord );

		  [Documented("The relationship type field has an illegal value.")]
		  void IllegalRelationshipType();

		  [Documented("The relationship type record is not in use.")]
		  void RelationshipTypeNotInUse( RelationshipTypeTokenRecord relationshipType );

		  [Documented("The source node field has an illegal value.")]
		  void IllegalSourceNode();

		  [Documented("The target node field has an illegal value.")]
		  void IllegalTargetNode();

		  [Documented("The source node is not in use.")]
		  void SourceNodeNotInUse( NodeRecord node );

		  [Documented("The target node is not in use.")]
		  void TargetNodeNotInUse( NodeRecord node );

		  [Documented("This record should be the first in the source chain, but the source node does not reference this record.")]
		  void SourceNodeDoesNotReferenceBack( NodeRecord node );

		  [Documented("This record should be the first in the target chain, but the target node does not reference this record.")]
		  void TargetNodeDoesNotReferenceBack( NodeRecord node );

		  [Documented("The source node does not have a relationship chain.")]
		  void SourceNodeHasNoRelationships( NodeRecord source );

		  [Documented("The target node does not have a relationship chain.")]
		  void TargetNodeHasNoRelationships( NodeRecord source );

		  [Documented("The previous record in the source chain is a relationship between two other nodes.")]
		  void SourcePrevReferencesOtherNodes( RelationshipRecord relationship );

		  [Documented("The next record in the source chain is a relationship between two other nodes.")]
		  void SourceNextReferencesOtherNodes( RelationshipRecord relationship );

		  [Documented("The previous record in the target chain is a relationship between two other nodes.")]
		  void TargetPrevReferencesOtherNodes( RelationshipRecord relationship );

		  [Documented("The next record in the target chain is a relationship between two other nodes.")]
		  void TargetNextReferencesOtherNodes( RelationshipRecord relationship );

		  [Documented("The previous record in the source chain does not have this record as its next record.")]
		  void SourcePrevDoesNotReferenceBack( RelationshipRecord relationship );

		  [Documented("The next record in the source chain does not have this record as its previous record.")]
		  void SourceNextDoesNotReferenceBack( RelationshipRecord relationship );

		  [Documented("The previous record in the target chain does not have this record as its next record.")]
		  void TargetPrevDoesNotReferenceBack( RelationshipRecord relationship );

		  [Documented("The next record in the target chain does not have this record as its previous record.")]
		  void TargetNextDoesNotReferenceBack( RelationshipRecord relationship );

		  [Documented("This relationship was not found in the expected index.")]
		  void NotIndexed( StoreIndexDescriptor index, object[] propertyValues );

		  [Documented("This relationship was found in the expected index, although multiple times")]
		  void IndexedMultipleTimes( StoreIndexDescriptor index, object[] propertyValues, long count );
	 }

	 public interface ConsistencyReport_PropertyConsistencyReport : ConsistencyReport
	 {
		  [Documented("The property key as an invalid value.")]
		  void InvalidPropertyKey( PropertyBlock block );

		  [Documented("The key for this property is not in use.")]
		  void KeyNotInUse( PropertyBlock block, PropertyKeyTokenRecord key );

		  [Documented("The previous property record is not in use.")]
		  void PrevNotInUse( PropertyRecord property );

		  [Documented("The next property record is not in use.")]
		  void NextNotInUse( PropertyRecord property );

		  [Documented("The previous property record does not have this record as its next record.")]
		  void PreviousDoesNotReferenceBack( PropertyRecord property );

		  [Documented("The next property record does not have this record as its previous record.")]
		  void NextDoesNotReferenceBack( PropertyRecord property );

		  [Documented("The type of this property is invalid.")]
		  void InvalidPropertyType( PropertyBlock block );

		  [Documented("The string block is not in use.")]
		  void StringNotInUse( PropertyBlock block, DynamicRecord value );

		  [Documented("The array block is not in use.")]
		  void ArrayNotInUse( PropertyBlock block, DynamicRecord value );

		  [Documented("The string block is empty.")]
		  void StringEmpty( PropertyBlock block, DynamicRecord value );

		  [Documented("The array block is empty.")]
		  void ArrayEmpty( PropertyBlock block, DynamicRecord value );

		  [Documented("The property value is invalid.")]
		  void InvalidPropertyValue( PropertyBlock block );

		  [Documented("This record is first in a property chain, but no Node or Relationship records reference this record.")]
		  void OrphanPropertyChain();

		  [Documented("The string property is not referenced anymore, but the corresponding block has not been deleted.")]
		  void StringUnreferencedButNotDeleted( PropertyBlock block );

		  [Documented("The array property is not referenced anymore, but the corresponding block as not been deleted.")]
		  void ArrayUnreferencedButNotDeleted( PropertyBlock block );

		  [Documented("This property was declared to be changed for a node or relationship, but that node or relationship " + "does not contain this property in its property chain.")]
		  void OwnerDoesNotReferenceBack();

		  [Documented("This property was declared to be changed for a node or relationship, but that node or relationship " + "did not contain this property in its property chain prior to the change. The property is referenced by another owner.")]
		  void ChangedForWrongOwner();

		  [Documented("The string record referred from this property is also referred from a another property.")]
		  void StringMultipleOwners( PropertyRecord otherOwner );

		  [Documented("The array record referred from this property is also referred from a another property.")]
		  void ArrayMultipleOwners( PropertyRecord otherOwner );

		  [Documented("The string record referred from this property is also referred from a another string record.")]
		  void StringMultipleOwners( DynamicRecord dynamic );

		  [Documented("The array record referred from this property is also referred from a another array record.")]
		  void ArrayMultipleOwners( DynamicRecord dynamic );
	 }

	 public interface ConsistencyReport_NameConsistencyReport : ConsistencyReport
	 {
		  [Documented("The name block is not in use.")]
		  void NameBlockNotInUse( DynamicRecord record );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("The token name is empty. Empty token names are discouraged and also prevented in version 2.0.x and " + "above, but they can be accessed just like any other tokens. It's possible that this token have been " + "created in an earlier version where there were no checks for name being empty.") void emptyName(org.neo4j.kernel.impl.store.record.DynamicRecord name);
		  [Documented("The token name is empty. Empty token names are discouraged and also prevented in version 2.0.x and " + "above, but they can be accessed just like any other tokens. It's possible that this token have been " + "created in an earlier version where there were no checks for name being empty.")]
		  void EmptyName( DynamicRecord name );

		  [Documented("The string record referred from this name record is also referred from a another string record.")]
		  void NameMultipleOwners( DynamicRecord otherOwner );
	 }

	 public interface ConsistencyReport_RelationshipTypeConsistencyReport : ConsistencyReport_NameConsistencyReport
	 {
		  [Documented("The string record referred from this relationship type is also referred from a another relationship type.")]
		  void NameMultipleOwners( RelationshipTypeTokenRecord otherOwner );
	 }

	 public interface ConsistencyReport_LabelTokenConsistencyReport : ConsistencyReport_NameConsistencyReport
	 {
		  [Documented("The string record referred from this label name is also referred from a another label name.")]
		  void NameMultipleOwners( LabelTokenRecord otherOwner );
	 }

	 public interface ConsistencyReport_PropertyKeyTokenConsistencyReport : ConsistencyReport_NameConsistencyReport
	 {
		  [Documented("The string record referred from this key is also referred from a another key.")]
		  void NameMultipleOwners( PropertyKeyTokenRecord otherOwner );
	 }

	 public interface ConsistencyReport_RelationshipGroupConsistencyReport : ConsistencyReport
	 {
		  [Documented("The relationship type field has an illegal value.")]
		  void IllegalRelationshipType();

		  [Documented("The relationship type record is not in use.")]
		  void RelationshipTypeNotInUse( RelationshipTypeTokenRecord referred );

		  [Documented("The next relationship group is not in use.")]
		  void NextGroupNotInUse();

		  [Documented("The location of group in the chain is invalid, should be sorted by type ascending.")]
		  void InvalidTypeSortOrder();

		  [Documented("The first outgoing relationship is not in use.")]
		  void FirstOutgoingRelationshipNotInUse();

		  [Documented("The first incoming relationship is not in use.")]
		  void FirstIncomingRelationshipNotInUse();

		  [Documented("The first loop relationship is not in use.")]
		  void FirstLoopRelationshipNotInUse();

		  [Documented("The first outgoing relationship is not the first in its chain.")]
		  void FirstOutgoingRelationshipNotFirstInChain();

		  [Documented("The first incoming relationship is not the first in its chain.")]
		  void FirstIncomingRelationshipNotFirstInChain();

		  [Documented("The first loop relationship is not the first in its chain.")]
		  void FirstLoopRelationshipNotFirstInChain();

		  [Documented("The first outgoing relationship is of a different type than its group.")]
		  void FirstOutgoingRelationshipOfOfOtherType();

		  [Documented("The first incoming relationship is of a different type than its group.")]
		  void FirstIncomingRelationshipOfOfOtherType();

		  [Documented("The first loop relationship is of a different type than its group.")]
		  void FirstLoopRelationshipOfOfOtherType();

		  [Documented("The owner of the relationship group is not in use.")]
		  void OwnerNotInUse();

		  [Documented("Illegal owner value.")]
		  void IllegalOwner();

		  [Documented("Next chained relationship group has another owner.")]
		  void NextHasOtherOwner( RelationshipGroupRecord referred );
	 }

	 public interface ConsistencyReport_DynamicConsistencyReport : ConsistencyReport
	 {
		  [Documented("The next block is not in use.")]
		  void NextNotInUse( DynamicRecord next );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("The record is not full, but references a next block.") void recordNotFullReferencesNext();
		  [Documented("The record is not full, but references a next block.")]
		  void RecordNotFullReferencesNext();

		  [Documented("The length of the block is invalid.")]
		  void InvalidLength();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("The block is empty.") void emptyBlock();
		  [Documented("The block is empty.")]
		  void EmptyBlock();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("The next block is empty.") void emptyNextBlock(org.neo4j.kernel.impl.store.record.DynamicRecord next);
		  [Documented("The next block is empty.")]
		  void EmptyNextBlock( DynamicRecord next );

		  [Documented("The next block references this (the same) record.")]
		  void SelfReferentialNext();

		  [Documented("The next block of this record is also referenced by another dynamic record.")]
		  void NextMultipleOwners( DynamicRecord otherOwner );

		  [Documented("The next block of this record is also referenced by a property record.")]
		  void NextMultipleOwners( PropertyRecord otherOwner );

		  [Documented("The next block of this record is also referenced by a relationship type.")]
		  void NextMultipleOwners( RelationshipTypeTokenRecord otherOwner );

		  [Documented("The next block of this record is also referenced by a property key.")]
		  void NextMultipleOwners( PropertyKeyTokenRecord otherOwner );

		  [Documented("This record not referenced from any other dynamic block, or from any property or name record.")]
		  void OrphanDynamicRecord();
	 }

	 public interface ConsistencyReport_DynamicLabelConsistencyReport : ConsistencyReport
	 {
		  [Documented("This label record is not referenced by its owning node record or that record is not in use.")]
		  void OrphanDynamicLabelRecordDueToInvalidOwner( NodeRecord owningNodeRecord );

		  [Documented("This label record does not have an owning node record.")]
		  void OrphanDynamicLabelRecord();
	 }

	 public interface ConsistencyReport_NodeInUseWithCorrectLabelsReport : ConsistencyReport
	 {
		  void NodeNotInUse( NodeRecord referredNodeRecord );

		  void NodeDoesNotHaveExpectedLabel( NodeRecord referredNodeRecord, long expectedLabelId );

		  void NodeLabelNotInIndex( NodeRecord referredNodeRecord, long missingLabelId );
	 }

	 public interface ConsistencyReport_RelationshipInUseWithCorrectRelationshipTypeReport : ConsistencyReport
	 {
		  void RelationshipNotInUse( RelationshipRecord referredRelationshipRecord );

		  void RelationshipDoesNotHaveExpectedRelationshipType( RelationshipRecord referredRelationshipRecord, long expectedRelationshipTypeId );
	 }

	 public interface ConsistencyReport_LabelScanConsistencyReport : ConsistencyReport_NodeInUseWithCorrectLabelsReport
	 {
		  [Documented("This label scan document refers to a node record that is not in use.")]
		  void NodeNotInUse( NodeRecord referredNodeRecord );

		  [Documented("This label scan document refers to a node that does not have the expected label.")]
		  void NodeDoesNotHaveExpectedLabel( NodeRecord referredNodeRecord, long expectedLabelId );

		  [Documented("This node record has a label that is not found in the label scan store entry for this node")]
		  void NodeLabelNotInIndex( NodeRecord referredNodeRecord, long missingLabelId );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("Label index was not properly shutdown and rebuild is required.") void dirtyIndex();
		  [Documented("Label index was not properly shutdown and rebuild is required.")]
		  void DirtyIndex();
	 }

	 public interface ConsistencyReport_IndexConsistencyReport : ConsistencyReport_NodeInUseWithCorrectLabelsReport, ConsistencyReport_RelationshipInUseWithCorrectRelationshipTypeReport
	 {
		  [Documented("This index entry refers to a node record that is not in use.")]
		  void NodeNotInUse( NodeRecord referredNodeRecord );

		  [Documented("This index entry refers to a relationship record that is not in use.")]
		  void RelationshipNotInUse( RelationshipRecord referredRelationshipRecord );

		  [Documented("This index entry refers to a node that does not have the expected label.")]
		  void NodeDoesNotHaveExpectedLabel( NodeRecord referredNodeRecord, long expectedLabelId );

		  [Documented("This index entry refers to a relationship that does not have the expected relationship type.")]
		  void RelationshipDoesNotHaveExpectedRelationshipType( RelationshipRecord referredRelationshipRecord, long expectedRelationshipTypeId );

		  [Documented("This node record has a label that is not found in the index for this node")]
		  void NodeLabelNotInIndex( NodeRecord referredNodeRecord, long missingLabelId );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Warning @Documented("Index was not properly shutdown and rebuild is required.") void dirtyIndex();
		  [Documented("Index was not properly shutdown and rebuild is required.")]
		  void DirtyIndex();

		  [Documented("This index entry is for a relationship index, but it is used as a constraint index")]
		  void RelationshipConstraintIndex();
	 }

	 public interface ConsistencyReport_CountsConsistencyReport : ConsistencyReport
	 {
		  [Documented("The node count does not correspond with the expected count.")]
		  void InconsistentNodeCount( long expectedCount );

		  [Documented("The relationship count does not correspond with the expected count.")]
		  void InconsistentRelationshipCount( long expectedCount );

		  [Documented("The node key entries in the store does not correspond with the expected number.")]
		  void InconsistentNumberOfNodeKeys( long expectedCount );

		  [Documented("The relationship key entries in the store does not correspond with the expected number.")]
		  void InconsistentNumberOfRelationshipKeys( long expectedCount );
	 }

}