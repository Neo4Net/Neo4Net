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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using Neo4Net.Consistency.Store;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using DynamicNodeLabels = Neo4Net.Kernel.impl.store.DynamicNodeLabels;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;

	public class NodeInUseWithCorrectLabelsCheck <RECORD, REPORT> : ComparativeRecordChecker<RECORD, NodeRecord, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_NodeInUseWithCorrectLabelsReport
	{
		 private readonly long[] _indexLabels;
		 private readonly Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType _propertySchemaType;
		 private readonly bool _checkStoreToIndex;

		 public NodeInUseWithCorrectLabelsCheck( long[] expectedEntityTokenIds, Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType propertySchemaType, bool checkStoreToIndex )
		 {
			  this._propertySchemaType = propertySchemaType;
			  this._checkStoreToIndex = checkStoreToIndex;
			  this._indexLabels = SortAndDeduplicate( expectedEntityTokenIds );
		 }

		 internal static long[] SortAndDeduplicate( long[] labels )
		 {
			  if ( ArrayUtils.isNotEmpty( labels ) )
			  {
					sort( labels );
					return PrimitiveLongCollections.deduplicate( labels );
			  }
			  return labels;
		 }

		 public override void CheckReference( RECORD record, NodeRecord nodeRecord, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( nodeRecord.InUse() )
			  {
					NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( nodeRecord );
					if ( nodeLabels is DynamicNodeLabels )
					{
						 DynamicNodeLabels dynamicNodeLabels = ( DynamicNodeLabels ) nodeLabels;
						 long firstRecordId = dynamicNodeLabels.FirstDynamicRecordId;
						 RecordReference<DynamicRecord> firstRecordReference = records.NodeLabels( firstRecordId );
						 ExpectedNodeLabelsChecker expectedNodeLabelsChecker = new ExpectedNodeLabelsChecker( this, nodeRecord );
						 LabelChainWalker<RECORD, REPORT> checker = new LabelChainWalker<RECORD, REPORT>( expectedNodeLabelsChecker );
						 engine.ComparativeCheck( firstRecordReference, checker );
						 nodeRecord.DynamicLabelRecords; // I think this is empty in production
					}
					else
					{
						 long[] storeLabels = nodeLabels.Get( null );
						 REPORT report = engine.Report();
						 ValidateLabelIds( nodeRecord, storeLabels, report );
					}
			  }
			  else if ( _indexLabels.Length != 0 )
			  {
					engine.Report().nodeNotInUse(nodeRecord);
			  }
		 }

		 private void ValidateLabelIds( NodeRecord nodeRecord, long[] storeLabels, REPORT report )
		 {
			  storeLabels = SortAndDeduplicate( storeLabels );

			  if ( _propertySchemaType == Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType.CompleteAllTokens )
			  {
					// The node must have all of the labels specified by the index.
					int indexLabelsCursor = 0;
					int storeLabelsCursor = 0;

					while ( indexLabelsCursor < _indexLabels.Length && storeLabelsCursor < storeLabels.Length )
					{
						 long indexLabel = _indexLabels[indexLabelsCursor];
						 long storeLabel = storeLabels[storeLabelsCursor];
						 if ( indexLabel < storeLabel )
						 { // node store has a label which isn't in label scan store
							  report.nodeDoesNotHaveExpectedLabel( nodeRecord, indexLabel );
							  indexLabelsCursor++;
						 }
						 else if ( indexLabel > storeLabel )
						 { // label scan store has a label which isn't in node store
							  ReportNodeLabelNotInIndex( report, nodeRecord, storeLabel );
							  storeLabelsCursor++;
						 }
						 else
						 { // both match
							  indexLabelsCursor++;
							  storeLabelsCursor++;
						 }
					}

					while ( indexLabelsCursor < _indexLabels.Length )
					{
						 report.nodeDoesNotHaveExpectedLabel( nodeRecord, _indexLabels[indexLabelsCursor++] );
					}
					while ( storeLabelsCursor < storeLabels.Length )
					{
						 ReportNodeLabelNotInIndex( report, nodeRecord, storeLabels[storeLabelsCursor] );
						 storeLabelsCursor++;
					}
			  }
			  else if ( _propertySchemaType == Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType.PartialAnyToken )
			  {
					// The node must have at least one label in the index.
					foreach ( long storeLabel in storeLabels )
					{
						 if ( Arrays.binarySearch( _indexLabels, storeLabel ) >= 0 )
						 {
							  // The node has one of the indexed labels, so we're good.
							  return;
						 }
					}
					// The node had none of the indexed labels, so we report all of them as missing.
					foreach ( long indexLabel in _indexLabels )
					{
						 report.nodeDoesNotHaveExpectedLabel( nodeRecord, indexLabel );
					}
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Unknown property schema type '" + _propertySchemaType + "'." );
			  }
		 }

		 private void ReportNodeLabelNotInIndex( REPORT report, NodeRecord nodeRecord, long storeLabel )
		 {
			  if ( _checkStoreToIndex )
			  {
					report.nodeLabelNotInIndex( nodeRecord, storeLabel );
			  }
		 }

		 private class ExpectedNodeLabelsChecker : LabelChainWalker.Validator<RECORD, REPORT>
		 {
			 private readonly NodeInUseWithCorrectLabelsCheck<RECORD, REPORT> _outerInstance;

			  internal readonly NodeRecord NodeRecord;

			  internal ExpectedNodeLabelsChecker( NodeInUseWithCorrectLabelsCheck<RECORD, REPORT> outerInstance, NodeRecord nodeRecord )
			  {
				  this._outerInstance = outerInstance;
					this.NodeRecord = nodeRecord;
			  }

			  public override void OnRecordNotInUse( DynamicRecord dynamicRecord, CheckerEngine<RECORD, REPORT> engine )
			  {
					// checked elsewhere
			  }

			  public override void OnRecordChainCycle( DynamicRecord record, CheckerEngine<RECORD, REPORT> engine )
			  {
					// checked elsewhere
			  }

			  public override void OnWellFormedChain( long[] labelIds, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
			  {
					outerInstance.validateLabelIds( NodeRecord, labelIds, engine.Report() );
			  }
		 }
	}

}