using System;
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
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using Org.Neo4j.Consistency.store;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using PropertyType = Org.Neo4j.Kernel.impl.store.PropertyType;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;

	public class PropertyRecordCheck : RecordCheck<PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport>
	{
		 public override void Check( PropertyRecord record, CheckerEngine<PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, RecordAccess records )
		 {
			  if ( !record.InUse() )
			  {
					return;
			  }
			  foreach ( PropertyField field in PropertyField.values() )
			  {
					field.checkConsistency( record, engine, records );
			  }
			  foreach ( PropertyBlock block in record )
			  {
					CheckDataBlock( block, engine, records );
			  }
		 }

		 public static void CheckDataBlock( PropertyBlock block, CheckerEngine<PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, RecordAccess records )
		 {
			  if ( block.KeyIndexId < 0 )
			  {
					engine.Report().invalidPropertyKey(block);
			  }
			  else
			  {
					engine.ComparativeCheck( records.PropertyKey( block.KeyIndexId ), PropertyKey( block ) );
			  }
			  PropertyType type = block.ForceGetType();
			  if ( type == null )
			  {
					engine.Report().invalidPropertyType(block);
			  }
			  else
			  {
					switch ( type.innerEnumValue )
					{
					case PropertyType.InnerEnum.STRING:
						 engine.ComparativeCheck( records.String( block.SingleValueLong ), DynamicReference.String( block ) );
						 break;
					case PropertyType.InnerEnum.ARRAY:
						 engine.ComparativeCheck( records.Array( block.SingleValueLong ), DynamicReference.Array( block ) );
						 break;
					default:
						 try
						 {
							  type.value( block, null );
						 }
						 catch ( Exception )
						 {
							  engine.Report().invalidPropertyValue(block);
						 }
						 break;
					}
			  }
		 }

		 public abstract class PropertyField : RecordField<PropertyRecord, ConsistencyReport.PropertyConsistencyReport>, ComparativeRecordChecker<PropertyRecord, PropertyRecord, ConsistencyReport.PropertyConsistencyReport>
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           PREV(org.neo4j.kernel.impl.store.record.Record.NO_PREVIOUS_PROPERTY) { public long valueFrom(org.neo4j.kernel.impl.store.record.PropertyRecord record) { return record.getPrevProp(); } public long otherReference(org.neo4j.kernel.impl.store.record.PropertyRecord record) { return record.getNextProp(); } public void notInUse(org.neo4j.consistency.report.ConsistencyReport_PropertyConsistencyReport report, org.neo4j.kernel.impl.store.record.PropertyRecord property) { report.prevNotInUse(property); } public void noBackReference(org.neo4j.consistency.report.ConsistencyReport_PropertyConsistencyReport report, org.neo4j.kernel.impl.store.record.PropertyRecord property) { report.previousDoesNotReferenceBack(property); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NEXT(org.neo4j.kernel.impl.store.record.Record.NO_NEXT_PROPERTY) { public long valueFrom(org.neo4j.kernel.impl.store.record.PropertyRecord record) { return record.getNextProp(); } public long otherReference(org.neo4j.kernel.impl.store.record.PropertyRecord record) { return record.getPrevProp(); } public void notInUse(org.neo4j.consistency.report.ConsistencyReport_PropertyConsistencyReport report, org.neo4j.kernel.impl.store.record.PropertyRecord property) { report.nextNotInUse(property); } public void noBackReference(org.neo4j.consistency.report.ConsistencyReport_PropertyConsistencyReport report, org.neo4j.kernel.impl.store.record.PropertyRecord property) { report.nextDoesNotReferenceBack(property); } };

			  private static readonly IList<PropertyField> valueList = new List<PropertyField>();

			  static PropertyField()
			  {
				  valueList.Add( PREV );
				  valueList.Add( NEXT );
			  }

			  public enum InnerEnum
			  {
				  PREV,
				  NEXT
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private PropertyField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  internal readonly Org.Neo4j.Kernel.impl.store.record.Record NONE;

			  PropertyField( Org.Neo4j.Kernel.impl.store.record.Record none ) { this.NONE = none; } public abstract long otherReference( Org.Neo4j.Kernel.impl.store.record.PropertyRecord record );

			  public void checkReference( Org.Neo4j.Kernel.impl.store.record.PropertyRecord record, Org.Neo4j.Kernel.impl.store.record.PropertyRecord referred, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records )
			  {
				  if ( !referred.inUse() ) { notInUse(engine.report(), referred); } else
				  {
					  if ( otherReference( referred ) != record.getId() ) { noBackReference(engine.report(), referred); }
				  }
			  }
			  abstract void notInUse( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, Org.Neo4j.Kernel.impl.store.record.PropertyRecord property );

			  public abstract void noBackReference( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, Org.Neo4j.Kernel.impl.store.record.PropertyRecord property );

			 public static IList<PropertyField> values()
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

			 public static PropertyField valueOf( string name )
			 {
				 foreach ( PropertyField enumInstance in PropertyField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static ComparativeRecordChecker<org.neo4j.kernel.impl.store.record.PropertyRecord, org.neo4j.kernel.impl.store.record.PropertyKeyTokenRecord, org.neo4j.consistency.report.ConsistencyReport_PropertyConsistencyReport> propertyKey(final org.neo4j.kernel.impl.store.record.PropertyBlock block)
		 private static ComparativeRecordChecker<PropertyRecord, PropertyKeyTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> PropertyKey( PropertyBlock block )
		 {
			  return new ComparativeRecordCheckerAnonymousInnerClass( block );
		 }

		 private class ComparativeRecordCheckerAnonymousInnerClass : ComparativeRecordChecker<PropertyRecord, PropertyKeyTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport>
		 {
			 private PropertyBlock _block;

			 public ComparativeRecordCheckerAnonymousInnerClass( PropertyBlock block )
			 {
				 this._block = block;
			 }

			 public void checkReference( PropertyRecord record, PropertyKeyTokenRecord referred, CheckerEngine<PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, RecordAccess records )
			 {
				  if ( !referred.InUse() )
				  {
						engine.Report().keyNotInUse(_block, referred);
				  }
			 }

			 public override string ToString()
			 {
				  return "PROPERTY_KEY";
			 }
		 }

		 private abstract class DynamicReference : ComparativeRecordChecker<PropertyRecord, DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport>
		 {
			 public abstract void CheckReference( RECORD record, REFERRED referred, CheckerEngine<RECORD, REPORT> engine, RecordAccess records );
			  internal readonly PropertyBlock Block;

			  internal DynamicReference( PropertyBlock block )
			  {
					this.Block = block;
			  }

			  public static DynamicReference String( PropertyBlock block )
			  {
					return new DynamicReferenceAnonymousInnerClass( block );
			  }

			  private class DynamicReferenceAnonymousInnerClass : DynamicReference
			  {
				  private new PropertyBlock _block;

				  public DynamicReferenceAnonymousInnerClass( PropertyBlock block ) : base( block )
				  {
					  this._block = block;
				  }

				  internal override void notUsed( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, DynamicRecord value )
				  {
						report.StringNotInUse( _block, value );
				  }

				  internal override void empty( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, DynamicRecord value )
				  {
						report.StringEmpty( _block, value );
				  }
			  }

			  public static DynamicReference Array( PropertyBlock block )
			  {
					return new DynamicReferenceAnonymousInnerClass2( block );
			  }

			  private class DynamicReferenceAnonymousInnerClass2 : DynamicReference
			  {
				  private new PropertyBlock _block;

				  public DynamicReferenceAnonymousInnerClass2( PropertyBlock block ) : base( block )
				  {
					  this._block = block;
				  }

				  internal override void notUsed( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, DynamicRecord value )
				  {
						report.ArrayNotInUse( _block, value );
				  }

				  internal override void empty( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, DynamicRecord value )
				  {
						report.ArrayEmpty( _block, value );
				  }
			  }

			  public override void CheckReference( PropertyRecord record, DynamicRecord referred, CheckerEngine<PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, RecordAccess records )
			  {
					if ( !referred.InUse() )
					{
						 NotUsed( engine.Report(), referred );
					}
					else
					{
						 if ( referred.Length <= 0 )
						 {
							  Empty( engine.Report(), referred );
						 }
					}
			  }

			  internal abstract void NotUsed( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, DynamicRecord value );

			  internal abstract void Empty( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report, DynamicRecord value );
		 }
	}

}