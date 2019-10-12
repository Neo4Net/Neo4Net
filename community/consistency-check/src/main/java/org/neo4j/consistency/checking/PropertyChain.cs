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
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using MandatoryProperties = Org.Neo4j.Consistency.checking.full.MandatoryProperties;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;

	public class PropertyChain<RECORD, REPORT> : RecordField<RECORD, REPORT>, ComparativeRecordChecker<RECORD, PropertyRecord, REPORT> where RECORD : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
	{
		 private readonly System.Func<RECORD, MandatoryProperties.Check<RECORD, REPORT>> _mandatoryProperties;

		 public PropertyChain( System.Func<RECORD, MandatoryProperties.Check<RECORD, REPORT>> mandatoryProperties )
		 {
			  this._mandatoryProperties = mandatoryProperties;
		 }

		 public override void CheckConsistency( RECORD record, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( !Record.NO_NEXT_PROPERTY.@is( record.NextProp ) )
			  {
					// Check the whole chain here instead of scattered during multiple checks.
					// This type of check obviously favors chains with good locality, performance-wise.
					IEnumerator<PropertyRecord> props = records.RawPropertyChain( record.NextProp );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					PropertyRecord firstProp = props.next();
					if ( !Record.NO_PREVIOUS_PROPERTY.@is( firstProp.PrevProp ) )
					{
						 engine.Report().propertyNotFirstInChain(firstProp);
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableIntSet keys = new org.eclipse.collections.impl.set.mutable.primitive.IntHashSet();
					MutableIntSet keys = new IntHashSet();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet propertyRecordIds = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet(8);
					MutableLongSet propertyRecordIds = new LongHashSet( 8 );
					propertyRecordIds.add( firstProp.Id );
					using ( MandatoryProperties.Check<RECORD, REPORT> mandatory = _mandatoryProperties.apply( record ) )
					{
						 CheckChainItem( firstProp, engine, keys, mandatory );

						 // Check the whole chain here. We also take the opportunity to check mandatory property constraints.
						 PropertyRecord prop = firstProp;
						 while ( props.MoveNext() )
						 {
							  PropertyRecord nextProp = props.Current;
							  if ( !propertyRecordIds.add( nextProp.Id ) )
							  {
									engine.Report().propertyChainContainsCircularReference(prop);
									break;
							  }
							  CheckChainItem( nextProp, engine, keys, mandatory );
							  prop = nextProp;
						 }
					}
			  }
		 }

		 private void CheckChainItem( PropertyRecord property, CheckerEngine<RECORD, REPORT> engine, MutableIntSet keys, MandatoryProperties.Check<RECORD, REPORT> mandatory )
		 {
			  if ( !property.InUse() )
			  {
					engine.Report().propertyNotInUse(property);
			  }
			  else
			  {
					int[] keysInRecord = ChainCheck.Keys( property );
					if ( mandatory != null )
					{
						 mandatory.Receive( keysInRecord );
					}
					foreach ( int key in keysInRecord )
					{
						 if ( !keys.add( key ) )
						 {
							  engine.Report().propertyKeyNotUniqueInChain();
						 }
					}
			  }
		 }

		 public override long ValueFrom( RECORD record )
		 {
			  return record.NextProp;
		 }

		 public override void CheckReference( RECORD record, PropertyRecord property, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( !property.InUse() )
			  {
					engine.Report().propertyNotInUse(property);
			  }
			  else
			  {
					if ( !Record.NO_PREVIOUS_PROPERTY.@is( property.PrevProp ) )
					{
						 engine.Report().propertyNotFirstInChain(property);
					}
					( new ChainCheck<RECORD, REPORT>() ).CheckReference(record, property, engine, records);
			  }
		 }
	}

}