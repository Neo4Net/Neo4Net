using System.Collections.Generic;

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
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using LabelIdArray = Neo4Net.Kernel.impl.store.LabelIdArray;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.AbstractDynamicStore.readFullByteArrayFromHeavyRecords;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicArrayStore.getRightArray;

	public class LabelChainWalker<RECORD, REPORT> : ComparativeRecordChecker<RECORD, DynamicRecord, REPORT> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
	{
		 private readonly Validator<RECORD, REPORT> _validator;

		 private readonly MutableLongObjectMap<DynamicRecord> _recordIds = new LongObjectHashMap<DynamicRecord>();
		 private readonly IList<DynamicRecord> _recordList = new List<DynamicRecord>();
		 private bool _allInUse = true;

		 public LabelChainWalker( Validator<RECORD, REPORT> validator )
		 {
			  this._validator = validator;
		 }

		 public override void CheckReference( RECORD record, DynamicRecord dynamicRecord, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  _recordIds.put( dynamicRecord.Id, dynamicRecord );

			  if ( dynamicRecord.InUse() )
			  {
					_recordList.Add( dynamicRecord );
			  }
			  else
			  {
					_allInUse = false;
					_validator.onRecordNotInUse( dynamicRecord, engine );
			  }

			  long nextBlock = dynamicRecord.NextBlock;
			  if ( Record.NO_NEXT_BLOCK.@is( nextBlock ) )
			  {
					if ( _allInUse )
					{
						 // only validate label ids if all dynamic records seen were in use
						 _validator.onWellFormedChain( LabelIds( _recordList ), engine, records );
					}
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.record.DynamicRecord nextRecord = recordIds.get(nextBlock);
					DynamicRecord nextRecord = _recordIds.get( nextBlock );
					if ( nextRecord != null )
					{
						 _validator.onRecordChainCycle( nextRecord, engine );
					}
					else
					{
						 engine.ComparativeCheck( records.NodeLabels( nextBlock ), this );
					}
			  }
		 }

		 public static long[] LabelIds( IList<DynamicRecord> recordList )
		 {
			  long[] idArray = ( long[] ) getRightArray( readFullByteArrayFromHeavyRecords( recordList, PropertyType.ARRAY ) ).asObject();
			  return LabelIdArray.stripNodeId( idArray );
		 }

		 public interface Validator<RECORD, REPORT> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
		 {
			  void OnRecordNotInUse( DynamicRecord dynamicRecord, CheckerEngine<RECORD, REPORT> engine );
			  void OnRecordChainCycle( DynamicRecord record, CheckerEngine<RECORD, REPORT> engine );
			  void OnWellFormedChain( long[] labelIds, CheckerEngine<RECORD, REPORT> engine, RecordAccess records );
		 }
	}

}