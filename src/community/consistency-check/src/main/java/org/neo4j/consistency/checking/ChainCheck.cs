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
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;

	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;

	public class ChainCheck<RECORD, REPORT> : ComparativeRecordChecker<RECORD, PropertyRecord, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
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