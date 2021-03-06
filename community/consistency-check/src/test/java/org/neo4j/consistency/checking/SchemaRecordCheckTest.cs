﻿/*
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
	using Test = org.junit.jupiter.api.Test;

	using IndexAccessors = Org.Neo4j.Consistency.checking.index.IndexAccessors;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccessStub = Org.Neo4j.Consistency.store.RecordAccessStub;
	using MalformedSchemaRuleException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaStorage = Org.Neo4j.Kernel.impl.store.SchemaStorage;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.constraintIndexRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.indexRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.uniquenessConstraintRule;

	internal class SchemaRecordCheckTest : RecordCheckTestBase<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport, SchemaRecordCheck>
	{
		 private readonly int _labelId = 1;
		 private readonly int _propertyKeyId = 2;

		 internal SchemaRecordCheckTest() : base(new SchemaRecordCheck(ConfigureSchemaStore(), ConfigureIndexAccessors()), typeof(org.neo4j.consistency.report.ConsistencyReport_SchemaConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportMalformedSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportMalformedSchemaRule()
		 {
			  // given
			  DynamicRecord badRecord = InUse( new DynamicRecord( 0 ) );
			  badRecord.SetType( RecordAccessStub.SCHEMA_RECORD_TYPE );

			  when( Checker().ruleAccess.loadSingleSchemaRule(0) ).thenThrow(new MalformedSchemaRuleException("Bad Record"));

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( badRecord );

			  // then
			  verify( report ).malformedSchemaRule();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportInvalidLabelReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportInvalidLabelReferences()
		 {
			  // given
			  int schemaRuleId = 0;

			  DynamicRecord record = InUse( DynamicRecord( schemaRuleId ) );
			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );
			  StoreIndexDescriptor rule = indexRule( schemaRuleId, _labelId, _propertyKeyId, providerDescriptor );
			  when( Checker().ruleAccess.loadSingleSchemaRule(schemaRuleId) ).thenReturn(rule);

			  LabelTokenRecord labelTokenRecord = Add( NotInUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( record );

			  // then
			  verify( report ).labelNotInUse( labelTokenRecord );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportInvalidPropertyReferenceFromIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportInvalidPropertyReferenceFromIndexRule()
		 {
			  // given
			  int schemaRuleId = 0;

			  DynamicRecord record = InUse( DynamicRecord( schemaRuleId ) );
			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );
			  StoreIndexDescriptor rule = indexRule( schemaRuleId, _labelId, _propertyKeyId, providerDescriptor );
			  when( Checker().ruleAccess.loadSingleSchemaRule(schemaRuleId) ).thenReturn(rule);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  PropertyKeyTokenRecord propertyKeyToken = Add( NotInUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( record );

			  // then
			  verify( report ).propertyKeyNotInUse( propertyKeyToken );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportInvalidPropertyReferenceFromUniquenessConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportInvalidPropertyReferenceFromUniquenessConstraintRule()
		 {
			  // given
			  int schemaRuleId = 0;
			  int indexRuleId = 1;

			  DynamicRecord record = InUse( DynamicRecord( schemaRuleId ) );

			  ConstraintRule rule = uniquenessConstraintRule( schemaRuleId, _labelId, _propertyKeyId, indexRuleId );

			  when( Checker().ruleAccess.loadSingleSchemaRule(schemaRuleId) ).thenReturn(rule);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  PropertyKeyTokenRecord propertyKeyToken = Add( NotInUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( record );

			  // then
			  verify( report ).propertyKeyNotInUse( propertyKeyToken );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportUniquenessConstraintNotReferencingBack() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportUniquenessConstraintNotReferencingBack()
		 {
			  // given
			  int ruleId1 = 0;
			  int ruleId2 = 1;

			  DynamicRecord record1 = InUse( DynamicRecord( ruleId1 ) );
			  DynamicRecord record2 = InUse( DynamicRecord( ruleId2 ) );

			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

			  StoreIndexDescriptor rule1 = constraintIndexRule( ruleId1, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId2 );
			  ConstraintRule rule2 = uniquenessConstraintRule( ruleId2, _labelId, _propertyKeyId, ruleId2 );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId1) ).thenReturn(rule1);
			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId2) ).thenReturn(rule2);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record1 );
			  Check( record2 );
			  SchemaRecordCheck obligationChecker = Checker().forObligationChecking();
			  Check( obligationChecker, record1 );
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( obligationChecker, record2 );

			  // then
			  verify( report ).uniquenessConstraintNotReferencingBack( record1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportConstraintIndexRuleWithoutBackReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotReportConstraintIndexRuleWithoutBackReference()
		 {
			  // given
			  int ruleId = 1;

			  DynamicRecord record = InUse( DynamicRecord( ruleId ) );

			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

			  StoreIndexDescriptor rule = constraintIndexRule( ruleId, _labelId, _propertyKeyId, providerDescriptor );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId) ).thenReturn(rule);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record );
			  SchemaRecordCheck obligationChecker = Checker().forObligationChecking();
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( obligationChecker, record );

			  // then
			  verifyZeroInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTwoUniquenessConstraintsReferencingSameIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportTwoUniquenessConstraintsReferencingSameIndex()
		 {
			  // given
			  int ruleId1 = 0;
			  int ruleId2 = 1;

			  DynamicRecord record1 = InUse( DynamicRecord( ruleId1 ) );
			  DynamicRecord record2 = InUse( DynamicRecord( ruleId2 ) );

			  ConstraintRule rule1 = uniquenessConstraintRule( ruleId1, _labelId, _propertyKeyId, ruleId2 );
			  ConstraintRule rule2 = uniquenessConstraintRule( ruleId2, _labelId, _propertyKeyId, ruleId2 );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId1) ).thenReturn(rule1);
			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId2) ).thenReturn(rule2);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record1 );
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( record2 );

			  // then
			  verify( report ).duplicateObligation( record1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportUnreferencedUniquenessConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportUnreferencedUniquenessConstraint()
		 {
			  // given
			  int ruleId = 0;

			  DynamicRecord record = InUse( DynamicRecord( ruleId ) );

			  ConstraintRule rule = uniquenessConstraintRule( ruleId, _labelId, _propertyKeyId, ruleId );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId) ).thenReturn(rule);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record );
			  SchemaRecordCheck obligationChecker = Checker().forObligationChecking();
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( obligationChecker, record );

			  // then
			  verify( report ).missingObligation( Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind.ConstraintIndexRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportConstraintIndexNotReferencingBack() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportConstraintIndexNotReferencingBack()
		 {
			  // given
			  int ruleId1 = 0;
			  int ruleId2 = 1;

			  DynamicRecord record1 = InUse( DynamicRecord( ruleId1 ) );
			  DynamicRecord record2 = InUse( DynamicRecord( ruleId2 ) );

			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

			  StoreIndexDescriptor rule1 = constraintIndexRule( ruleId1, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId1 );
			  ConstraintRule rule2 = uniquenessConstraintRule( ruleId2, _labelId, _propertyKeyId, ruleId1 );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId1) ).thenReturn(rule1);
			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId2) ).thenReturn(rule2);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record1 );
			  Check( record2 );
			  SchemaRecordCheck obligationChecker = Checker().forObligationChecking();
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( obligationChecker, record1 );
			  Check( obligationChecker, record2 );

			  // then
			  verify( report ).constraintIndexRuleNotReferencingBack( record2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTwoConstraintIndexesReferencingSameConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportTwoConstraintIndexesReferencingSameConstraint()
		 {
			  // given
			  int ruleId1 = 0;
			  int ruleId2 = 1;

			  DynamicRecord record1 = InUse( DynamicRecord( ruleId1 ) );
			  DynamicRecord record2 = InUse( DynamicRecord( ruleId2 ) );

			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

			  StoreIndexDescriptor rule1 = constraintIndexRule( ruleId1, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId1 );
			  StoreIndexDescriptor rule2 = constraintIndexRule( ruleId2, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId1 );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId1) ).thenReturn(rule1);
			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId2) ).thenReturn(rule2);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record1 );
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( record2 );

			  // then
			  verify( report ).duplicateObligation( record1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportUnreferencedConstraintIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportUnreferencedConstraintIndex()
		 {
			  // given
			  int ruleId = 0;

			  DynamicRecord record = InUse( DynamicRecord( ruleId ) );

			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

			  StoreIndexDescriptor rule = constraintIndexRule( ruleId, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId) ).thenReturn(rule);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record );
			  SchemaRecordCheck obligationChecker = Checker().forObligationChecking();
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( obligationChecker, record );

			  // then
			  verify( report ).missingObligation( Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind.UniquenessConstraint );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTwoIndexRulesWithDuplicateContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportTwoIndexRulesWithDuplicateContent()
		 {
			  // given
			  int ruleId1 = 0;
			  int ruleId2 = 1;

			  DynamicRecord record1 = InUse( DynamicRecord( ruleId1 ) );
			  DynamicRecord record2 = InUse( DynamicRecord( ruleId2 ) );

			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

			  StoreIndexDescriptor rule1 = constraintIndexRule( ruleId1, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId1 );
			  StoreIndexDescriptor rule2 = constraintIndexRule( ruleId2, _labelId, _propertyKeyId, providerDescriptor, ( long ) ruleId2 );

			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId1) ).thenReturn(rule1);
			  when( Checker().ruleAccess.loadSingleSchemaRule(ruleId2) ).thenReturn(rule2);

			  Add( InUse( new LabelTokenRecord( _labelId ) ) );
			  Add( InUse( new PropertyKeyTokenRecord( _propertyKeyId ) ) );

			  // when
			  Check( record1 );
			  Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport report = Check( record2 );

			  // then
			  verify( report ).duplicateRuleContent( record1 );
		 }

		 private static IndexAccessors ConfigureIndexAccessors()
		 {
			  return mock( typeof( IndexAccessors ) );
		 }

		 private static SchemaStorage ConfigureSchemaStore()
		 {
			  return mock( typeof( SchemaStorage ) );
		 }

		 private static DynamicRecord DynamicRecord( long id )
		 {
			  DynamicRecord record = new DynamicRecord( id );
			  record.SetType( RecordAccessStub.SCHEMA_RECORD_TYPE );
			  return record;
		 }
	}

}