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
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;

	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;

	public class ChainCheck<RECORD, REPORT> : ComparativeRecordChecker<RECORD, PropertyRecord, REPORT> where RECORD : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
	{
		 private const int MAX_BLOCK_PER_RECORD_COUNT = 4;
		 private readonly MutableIntSet _keys = new IntHashSet();

		 public override void CheckReference( RECORD record, PropertyRecord property, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  foreach ( int key in Keys( property ) )
			  {
					if ( !_keys.add( key ) )
					{
						 engine.Report().propertyKeyNotUniqueInChain();
					}
			  }
			  if ( !Record.NO_NEXT_PROPERTY.@is( property.NextProp ) )
			  {
					engine.ComparativeCheck( records.Property( property.NextProp ), this );
			  }
		 }

		 public static int[] Keys( PropertyRecord property )
		 {
			  int[] toStartWith = new int[MAX_BLOCK_PER_RECORD_COUNT];
			  int index = 0;
			  foreach ( PropertyBlock propertyBlock in property )
			  {
					toStartWith[index++] = propertyBlock.KeyIndexId;
			  }
			  return Arrays.copyOf( toStartWith, index );
		 }
	}

}