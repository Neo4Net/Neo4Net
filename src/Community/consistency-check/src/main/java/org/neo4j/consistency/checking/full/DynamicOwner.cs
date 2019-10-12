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
namespace Neo4Net.Consistency.checking.full
{
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using Neo4Net.Consistency.report;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using TokenRecord = Neo4Net.Kernel.impl.store.record.TokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.store.RecordReference_SkippingReference.skipReference;

	internal abstract class DynamicOwner<RECORD> : Owner where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	{
		 internal static readonly ComparativeRecordChecker<DynamicRecord, AbstractBaseRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> OrphanCheck = ( record, ignored, engine, records ) => engine.report().orphanDynamicRecord();

		 internal abstract RecordReference<RECORD> Record( RecordAccess records );

		 public override void CheckOrphanage()
		 {
			  // default: do nothing
		 }

		 internal class Property : DynamicOwner<PropertyRecord>, ComparativeRecordChecker<PropertyRecord, AbstractBaseRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport>
		 {
			  internal readonly long Id;
			  internal readonly RecordType Type;

			  internal Property( RecordType type, PropertyRecord record )
			  {
					this.Type = type;
					this.Id = record.Id;
			  }

			  internal override RecordReference<PropertyRecord> Record( RecordAccess records )
			  {
					return records.Property( Id );
			  }

			  public override void CheckReference( PropertyRecord property, AbstractBaseRecord record, CheckerEngine<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, RecordAccess records )
			  {
					if ( record is PropertyRecord )
					{
						 if ( Type == RecordType.STRING_PROPERTY )
						 {
							  engine.Report().stringMultipleOwners((PropertyRecord) record);
						 }
						 else
						 {
							  engine.Report().arrayMultipleOwners((PropertyRecord) record);
						 }
					}
					else if ( record is DynamicRecord )
					{
						 if ( Type == RecordType.STRING_PROPERTY )
						 {
							  engine.Report().stringMultipleOwners((DynamicRecord) record);
						 }
						 else
						 {
							  engine.Report().arrayMultipleOwners((DynamicRecord) record);
						 }
					}
			  }
		 }

		 internal class Dynamic : DynamicOwner<DynamicRecord>, ComparativeRecordChecker<DynamicRecord, AbstractBaseRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport>
		 {
			  internal readonly long Id;
			  internal readonly RecordType Type;

			  internal Dynamic( RecordType type, DynamicRecord record )
			  {
					this.Type = type;
					this.Id = record.Id;
			  }

			  internal override RecordReference<DynamicRecord> Record( RecordAccess records )
			  {
					switch ( Type )
					{
					case RecordType.STRING_PROPERTY:
						 return records.String( Id );
					case RecordType.ARRAY_PROPERTY:
						 return records.Array( Id );
					case RecordType.PROPERTY_KEY_NAME:
						 return records.PropertyKeyName( ( int )Id );
					case RecordType.RELATIONSHIP_TYPE_NAME:
						 return records.RelationshipTypeName( ( int ) Id );
					default:
						 return skipReference();
					}
			  }

			  public override void CheckReference( DynamicRecord block, AbstractBaseRecord record, CheckerEngine<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> engine, RecordAccess records )
			  {
					if ( record is PropertyRecord )
					{
						 engine.Report().nextMultipleOwners((PropertyRecord) record);
					}
					else if ( record is DynamicRecord )
					{
						 engine.Report().nextMultipleOwners((DynamicRecord) record);
					}
					else if ( record is RelationshipTypeTokenRecord )
					{
						 engine.Report().nextMultipleOwners((RelationshipTypeTokenRecord) record);
					}
					else if ( record is PropertyKeyTokenRecord )
					{
						 engine.Report().nextMultipleOwners((PropertyKeyTokenRecord) record);
					}
			  }
		 }

		 internal abstract class NameOwner<RECORD, REPORT> : DynamicOwner<RECORD>, ComparativeRecordChecker<RECORD, AbstractBaseRecord, REPORT> where RECORD : Neo4Net.Kernel.impl.store.record.TokenRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_NameConsistencyReport
		 {
			 public abstract void CheckReference( RECORD record, REFERRED referred, CheckerEngine<RECORD, REPORT> engine, RecordAccess records );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ConstantConditions") @Override public void checkReference(RECORD name, org.neo4j.kernel.impl.store.record.AbstractBaseRecord record, org.neo4j.consistency.checking.CheckerEngine<RECORD, REPORT> engine, org.neo4j.consistency.store.RecordAccess records)
			  public override void CheckReference( RECORD name, AbstractBaseRecord record, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
			  {
					Neo4Net.Consistency.report.ConsistencyReport_NameConsistencyReport report = engine.Report();
					if ( record is RelationshipTypeTokenRecord )
					{
						 ( ( Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport ) report ).NameMultipleOwners( ( RelationshipTypeTokenRecord ) record );
					}
					else if ( record is PropertyKeyTokenRecord )
					{
						 ( ( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport ) report ).NameMultipleOwners( ( PropertyKeyTokenRecord ) record );
					}
					else if ( record is DynamicRecord )
					{
						 report.NameMultipleOwners( ( DynamicRecord ) record );
					}
			  }
		 }

		 internal class PropertyKey : NameOwner<PropertyKeyTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport>
		 {
			  internal readonly int Id;

			  internal PropertyKey( PropertyKeyTokenRecord record )
			  {
					this.Id = record.IntId;
			  }

			  internal override RecordReference<PropertyKeyTokenRecord> Record( RecordAccess records )
			  {
					return records.PropertyKey( Id );
			  }
		 }

		 internal class LabelToken : NameOwner<LabelTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport>
		 {
			  internal readonly int Id;

			  internal LabelToken( LabelTokenRecord record )
			  {
					this.Id = record.IntId;
			  }

			  internal override RecordReference<LabelTokenRecord> Record( RecordAccess records )
			  {
					return records.Label( Id );
			  }
		 }

		 internal class RelationshipTypeToken : NameOwner<RelationshipTypeTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport>
		 {
			  internal readonly int Id;

			  internal RelationshipTypeToken( RelationshipTypeTokenRecord record )
			  {
					this.Id = record.IntId;
			  }

			  internal override RecordReference<RelationshipTypeTokenRecord> Record( RecordAccess records )
			  {
					return records.RelationshipType( Id );
			  }
		 }

		 internal class Unknown : DynamicOwner<AbstractBaseRecord>, RecordReference<AbstractBaseRecord>
		 {
			  internal PendingReferenceCheck<AbstractBaseRecord> Reporter;

			  internal override RecordReference<AbstractBaseRecord> Record( RecordAccess records )
			  {
					// Getting the record for this owner means that some other owner replaced it
					// that means that it isn't an orphan, so we skip this orphan check
					// and return a record for conflict check that always is ok (by skipping the check)
					this.MarkInCustody();
					return skipReference();
			  }

			  public override void CheckOrphanage()
			  {
					PendingReferenceCheck<AbstractBaseRecord> reporter = Pop();
					if ( reporter != null )
					{
						 reporter.CheckReference( null, null );
					}
			  }

			  internal virtual void MarkInCustody()
			  {
					PendingReferenceCheck<AbstractBaseRecord> reporter = Pop();
					if ( reporter != null )
					{
						 reporter.Skip();
					}
			  }

			  internal virtual PendingReferenceCheck<AbstractBaseRecord> Pop()
			  {
				  lock ( this )
				  {
						try
						{
							 return this.Reporter;
						}
						finally
						{
							 this.Reporter = null;
						}
				  }
			  }

			  public override void Dispatch( PendingReferenceCheck<AbstractBaseRecord> reporter )
			  {
				  lock ( this )
				  {
						this.Reporter = reporter;
				  }
			  }
		 }

		 private DynamicOwner()
		 {
			  // only internal subclasses
		 }
	}

}