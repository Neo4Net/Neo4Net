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
namespace Org.Neo4j.Consistency.checking
{

	using IndexAccessors = Org.Neo4j.Consistency.checking.index.IndexAccessors;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using MalformedSchemaRuleException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaProcessor;
	using SchemaRuleAccess = Org.Neo4j.Kernel.impl.store.SchemaRuleAccess;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Note that this class builds up an in-memory representation of the complete schema store by being used in
	/// multiple phases.
	/// 
	/// This differs from other store checks, where we deliberately avoid building up state, expecting store to generally be
	/// larger than available memory. However, it is safe to make the assumption that schema storage will fit in memory
	/// because the same assumption is also made by the online database.
	/// </summary>
	public class SchemaRecordCheck : RecordCheck<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport>
	{
		 internal readonly SchemaRuleAccess RuleAccess;

		 private readonly IndexAccessors _indexAccessors;
		 private readonly IDictionary<long, DynamicRecord> _indexObligations;
		 private readonly IDictionary<long, DynamicRecord> _constraintObligations;
		 private readonly IDictionary<SchemaRule, DynamicRecord> _verifiedRulesWithRecords;
		 private readonly CheckStrategy _strategy;

		 public SchemaRecordCheck( SchemaRuleAccess ruleAccess, IndexAccessors indexAccessors )
		 {
			  this.RuleAccess = ruleAccess;
			  this._indexAccessors = indexAccessors;
			  this._indexObligations = new Dictionary<long, DynamicRecord>();
			  this._constraintObligations = new Dictionary<long, DynamicRecord>();
			  this._verifiedRulesWithRecords = new Dictionary<SchemaRule, DynamicRecord>();
			  this._strategy = new RulesCheckStrategy( this );
		 }

		 private SchemaRecordCheck( SchemaRuleAccess ruleAccess, IndexAccessors indexAccessors, IDictionary<long, DynamicRecord> indexObligations, IDictionary<long, DynamicRecord> constraintObligations, IDictionary<SchemaRule, DynamicRecord> verifiedRulesWithRecords, CheckStrategy strategy )
		 {
			  this.RuleAccess = ruleAccess;
			  this._indexAccessors = indexAccessors;
			  this._indexObligations = indexObligations;
			  this._constraintObligations = constraintObligations;
			  this._verifiedRulesWithRecords = verifiedRulesWithRecords;
			  this._strategy = strategy;
		 }

		 public virtual SchemaRecordCheck ForObligationChecking()
		 {
			  return new SchemaRecordCheck( RuleAccess, _indexAccessors, _indexObligations, _constraintObligations, _verifiedRulesWithRecords, new ObligationsCheckStrategy( this ) );
		 }

		 public override void Check( DynamicRecord record, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine, RecordAccess records )
		 {
			  if ( record.InUse() && record.StartRecord )
			  {
					// parse schema rule
					SchemaRule rule;
					try
					{
						 rule = RuleAccess.loadSingleSchemaRule( record.Id );
					}
					catch ( MalformedSchemaRuleException )
					{
						 engine.Report().malformedSchemaRule();
						 return;
					}

					if ( rule is StoreIndexDescriptor )
					{
						 _strategy.checkIndexRule( ( StoreIndexDescriptor )rule, record, records, engine );
					}
					else if ( rule is ConstraintRule )
					{
						 _strategy.checkConstraintRule( ( ConstraintRule ) rule, record, records, engine );
					}
					else
					{
						 engine.Report().unsupportedSchemaRuleKind(null);
					}
			  }
		 }

		 private interface CheckStrategy
		 {
			  void CheckIndexRule( StoreIndexDescriptor rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine );

			  void CheckConstraintRule( ConstraintRule rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine );
		 }

		 /// <summary>
		 /// Verify rules can be de-serialized, have valid forward references, and build up internal state
		 /// for checking in back references in later phases (obligations)
		 /// </summary>
		 private class RulesCheckStrategy : CheckStrategy
		 {
			 private readonly SchemaRecordCheck _outerInstance;

			 public RulesCheckStrategy( SchemaRecordCheck outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void CheckIndexRule( StoreIndexDescriptor rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine )
			  {
					outerInstance.checkSchema( rule, record, records, engine );

					if ( rule.CanSupportUniqueConstraint() && rule.OwningConstraint != null )
					{
						 DynamicRecord previousObligation = outerInstance.constraintObligations[rule.OwningConstraint] = record.Clone();
						 if ( previousObligation != null )
						 {
							  engine.Report().duplicateObligation(previousObligation);
						 }
					}
			  }

			  public override void CheckConstraintRule( ConstraintRule rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine )
			  {
					outerInstance.checkSchema( rule, record, records, engine );

					if ( rule.ConstraintDescriptor.enforcesUniqueness() )
					{
						 DynamicRecord previousObligation = outerInstance.indexObligations[rule.OwnedIndex] = record.Clone();
						 if ( previousObligation != null )
						 {
							  engine.Report().duplicateObligation(previousObligation);
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Verify obligations, that is correct back references
		 /// </summary>
		 private class ObligationsCheckStrategy : CheckStrategy
		 {
			 private readonly SchemaRecordCheck _outerInstance;

			 public ObligationsCheckStrategy( SchemaRecordCheck outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void CheckIndexRule( StoreIndexDescriptor rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine )
			  {
					if ( rule.CanSupportUniqueConstraint() )
					{
						 DynamicRecord obligation = outerInstance.indexObligations[rule.Id];
						 if ( obligation == null ) // no pointer to here
						 {
							  if ( rule.OwningConstraint != null ) // we only expect a pointer if we have an owner
							  {
									engine.Report().missingObligation(Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind.UniquenessConstraint);
							  }
						 }
						 else
						 {
							  // if someone points to here, it must be our owner
							  if ( obligation.Id != rule.OwningConstraint.Value )
							  {
									engine.Report().constraintIndexRuleNotReferencingBack(obligation);
							  }
						 }
					}
					if ( outerInstance.indexAccessors.NotOnlineRules().Contains(rule) )
					{
						 engine.Report().schemaRuleNotOnline(rule);
					}
			  }

			  public override void CheckConstraintRule( ConstraintRule rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine )
			  {
					if ( rule.ConstraintDescriptor.enforcesUniqueness() )
					{
						 DynamicRecord obligation = outerInstance.constraintObligations[rule.Id];
						 if ( obligation == null )
						 {
							  engine.Report().missingObligation(Org.Neo4j.Storageengine.Api.schema.SchemaRule_Kind.ConstraintIndexRule);
						 }
						 else
						 {
							  if ( obligation.Id != rule.OwnedIndex )
							  {
									engine.Report().uniquenessConstraintNotReferencingBack(obligation);
							  }
						 }
					}
			  }
		 }

		 private void CheckSchema( SchemaRule rule, DynamicRecord record, RecordAccess records, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine )
		 {
			  rule.Schema().processWith(new CheckSchema(engine, records));
			  CheckForDuplicates( rule, record, engine );
		 }

		 internal class CheckSchema : SchemaProcessor
		 {
			  internal readonly CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> Engine;
			  internal readonly RecordAccess Records;

			  internal CheckSchema( CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine, RecordAccess records )
			  {
					this.Engine = engine;
					this.Records = records;
			  }

			  public override void ProcessSpecific( LabelSchemaDescriptor schema )
			  {
					Engine.comparativeCheck( Records.label( Schema.LabelId ), _validLabel );
					CheckProperties( Schema.PropertyIds );
			  }

			  public override void ProcessSpecific( RelationTypeSchemaDescriptor schema )
			  {
					Engine.comparativeCheck( Records.relationshipType( Schema.RelTypeId ), _validRelationshipType );
					CheckProperties( Schema.PropertyIds );
			  }

			  public override void ProcessSpecific( SchemaDescriptor schema )
			  {
					switch ( Schema.entityType() )
					{
					case NODE:
						 foreach ( int entityTokenId in Schema.EntityTokenIds )
						 {
							  Engine.comparativeCheck( Records.label( entityTokenId ), _validLabel );
						 }
						 break;
					case RELATIONSHIP:
						 foreach ( int entityTokenId in Schema.EntityTokenIds )
						 {
							  Engine.comparativeCheck( Records.relationshipType( entityTokenId ), _validRelationshipType );
						 }
						 break;
					default:
						 throw new System.ArgumentException( "Schema with given entity type is not supported: " + Schema.entityType() );
					}

					CheckProperties( Schema.PropertyIds );
			  }

			  internal virtual void CheckProperties( int[] propertyIds )
			  {
					foreach ( int propertyId in propertyIds )
					{
						 Engine.comparativeCheck( Records.propertyKey( propertyId ), _validPropertyKey );
					}
			  }
		 }

		 private void CheckForDuplicates( SchemaRule rule, DynamicRecord record, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> engine )
		 {
			  DynamicRecord previousContentRecord = _verifiedRulesWithRecords[rule] = record.Clone();
			  if ( previousContentRecord != null )
			  {
					engine.Report().duplicateRuleContent(previousContentRecord);
			  }
		 }

		 private static readonly ComparativeRecordChecker<DynamicRecord, LabelTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> _validLabel = ( record, labelTokenRecord, engine, records ) =>
		 {
					 if ( !labelTokenRecord.inUse() )
					 {
						  engine.report().labelNotInUse(labelTokenRecord);
					 }
		 };

		 private static readonly ComparativeRecordChecker<DynamicRecord, RelationshipTypeTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> _validRelationshipType = ( record, relTypeTokenRecord, engine, records ) =>
		 {
					 if ( !relTypeTokenRecord.inUse() )
					 {
						  engine.report().relationshipTypeNotInUse(relTypeTokenRecord);
					 }
		 };

		 private static readonly ComparativeRecordChecker<DynamicRecord, PropertyKeyTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> _validPropertyKey = ( record, propertyKeyTokenRecord, engine, records ) =>
		 {
					 if ( !propertyKeyTokenRecord.inUse() )
					 {
						  engine.report().propertyKeyNotInUse(propertyKeyTokenRecord);
					 }
		 };
	}

}