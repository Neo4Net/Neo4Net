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
namespace Neo4Net.Consistency.checking.full
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using DynamicNodeLabels = Neo4Net.Kernel.impl.store.DynamicNodeLabels;
	using InlineNodeLabels = Neo4Net.Kernel.impl.store.InlineNodeLabels;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	public class NodeLabelReader
	{
		 private NodeLabelReader()
		 {
		 }

		 public static ISet<long> GetListOfLabels<RECORD, REPORT>( NodeRecord nodeRecord, RecordAccess records, CheckerEngine<RECORD, REPORT> engine ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<long> labels = new java.util.HashSet<>();
			  ISet<long> labels = new HashSet<long>();

			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( nodeRecord );
			  if ( nodeLabels is DynamicNodeLabels )
			  {

					DynamicNodeLabels dynamicNodeLabels = ( DynamicNodeLabels ) nodeLabels;
					long firstRecordId = dynamicNodeLabels.FirstDynamicRecordId;
					RecordReference<DynamicRecord> firstRecordReference = records.NodeLabels( firstRecordId );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: engine.comparativeCheck(firstRecordReference, new org.neo4j.consistency.checking.LabelChainWalker<>(new org.neo4j.consistency.checking.LabelChainWalker.Validator<RECORD, REPORT>()
					engine.ComparativeCheck( firstRecordReference, new LabelChainWalker<RECORD, ?, REPORT>( new ValidatorAnonymousInnerClass( records, engine, labels ) ) );
			  }
			  else
			  {
					CopyToSet( nodeLabels.Get( null ), labels );
			  }

			  return labels;
		 }

		 private class ValidatorAnonymousInnerClass : LabelChainWalker.Validator<RECORD, REPORT>
		 {
			 private RecordAccess _records;
			 private CheckerEngine<RECORD, REPORT> _engine;
			 private ISet<long> _labels;

			 public ValidatorAnonymousInnerClass( RecordAccess records, CheckerEngine<RECORD, REPORT> engine, ISet<long> labels )
			 {
				 this._records = records;
				 this._engine = engine;
				 this._labels = labels;
			 }

			 public void onRecordNotInUse( DynamicRecord dynamicRecord, CheckerEngine<RECORD, REPORT> engine )
			 {
			 }

			 public void onRecordChainCycle( DynamicRecord record, CheckerEngine<RECORD, REPORT> engine )
			 {
			 }

			 public void onWellFormedChain( long[] labelIds, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
			 {
				  CopyToSet( labelIds, _labels );
			 }
		 }

		 public static long[] GetListOfLabels( NodeRecord nodeRecord, RecordStore<DynamicRecord> labels )
		 {
			  long field = nodeRecord.LabelField;
			  if ( NodeLabelsField.fieldPointsToDynamicRecordOfLabels( field ) )
			  {
					IList<DynamicRecord> recordList = new List<DynamicRecord>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet alreadySeen = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
					MutableLongSet alreadySeen = new LongHashSet();
					long id = NodeLabelsField.firstDynamicLabelRecordId( field );
					while ( !Record.NULL_REFERENCE.@is( id ) )
					{
						 DynamicRecord record = labels.GetRecord( id, labels.NewRecord(), FORCE );
						 if ( !record.InUse() || !alreadySeen.add(id) )
						 {
							  return PrimitiveLongCollections.EMPTY_LONG_ARRAY;
						 }
						 recordList.Add( record );
					}
					return LabelChainWalker.labelIds( recordList );
			  }
			  return InlineNodeLabels.get( nodeRecord );
		 }

		 public static ISet<long> GetListOfLabels( long labelField )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<long> labels = new java.util.HashSet<>();
			  ISet<long> labels = new HashSet<long>();
			  CopyToSet( InlineNodeLabels.parseInlined( labelField ), labels );

			  return labels;
		 }

		 private static void CopyToSet( long[] array, ISet<long> set )
		 {
			  foreach ( long labelId in array )
			  {
					set.Add( labelId );
			  }
		 }
	}

}