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
namespace Neo4Net.Consistency.checking
{
	using FullCheck = Neo4Net.Consistency.checking.full.FullCheck;
	using ConsistencyReport_DynamicLabelConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_DynamicLabelConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.fieldPointsToDynamicRecordOfLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.firstDynamicLabelRecordId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeStore.readOwnerFromDynamicLabelsRecord;

	/// <summary>
	/// Used by <seealso cref="FullCheck"/> to verify orphanage for node dynamic label records.
	/// 
	/// Actual list of labels is verified from <seealso cref="NodeRecordCheck"/>
	/// </summary>
	public class NodeDynamicLabelOrphanChainStartCheck : RecordCheck<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport>, ComparativeRecordChecker<DynamicRecord, DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport>
	{

		 private static readonly ComparativeRecordChecker<DynamicRecord, NodeRecord, ConsistencyReport_DynamicLabelConsistencyReport> _validNodeRecord = ( record, nodeRecord, engine, records ) =>
		 {
					 if ( !nodeRecord.inUse() )
					 {
						  // if this node record is not in use it is not a valid owner
						  engine.report().orphanDynamicLabelRecordDueToInvalidOwner(nodeRecord);
					 }
					 else
					 {
						  // if this node record is in use but doesn't point to the dynamic label record
						  // that label record has an invalid owner
						  long recordId = record.Id;
						  if ( fieldPointsToDynamicRecordOfLabels( nodeRecord.LabelField ) )
						  {
								long dynamicLabelRecordId = firstDynamicLabelRecordId( nodeRecord.LabelField );
								if ( dynamicLabelRecordId != recordId )
								{
									 engine.report().orphanDynamicLabelRecordDueToInvalidOwner(nodeRecord);
								}
						  }
					 }
		 };

		 public override void Check( DynamicRecord record, CheckerEngine<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport> engine, RecordAccess records )
		 {
			  if ( record.InUse() && record.StartRecord )
			  {
					long? ownerId = readOwnerFromDynamicLabelsRecord( record );
					if ( null == ownerId )
					{
						 // no owner but in use indicates a broken record
						 engine.Report().orphanDynamicLabelRecord();
					}
					else
					{
						 // look at owning node record to verify consistency
						 engine.ComparativeCheck( records.Node( ownerId.Value ), _validNodeRecord );
					}
			  }
		 }

		 public override void CheckReference( DynamicRecord record, DynamicRecord record2, CheckerEngine<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport> engine, RecordAccess records )
		 {
		 }
	}

}