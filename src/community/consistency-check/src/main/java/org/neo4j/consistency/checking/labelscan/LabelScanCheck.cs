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
namespace Neo4Net.Consistency.checking.labelscan
{
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking.full;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using LabelScanDocument = Neo4Net.Consistency.store.synthetic.LabelScanDocument;
	using NodeLabelRange = Neo4Net.Kernel.api.labelscan.NodeLabelRange;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.schema.SchemaDescriptor_PropertySchemaType.COMPLETE_ALL_TOKENS;

	public class LabelScanCheck : RecordCheck<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport>
	{
		 public override void Check( LabelScanDocument record, CheckerEngine<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport> engine, RecordAccess records )
		 {
			  NodeLabelRange range = record.NodeLabelRange;
			  foreach ( long nodeId in range.Nodes() )
			  {
					long[] labels = record.NodeLabelRange.labels( nodeId );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: engine.comparativeCheck(records.node(nodeId), new org.Neo4Net.consistency.checking.full.NodeInUseWithCorrectLabelsCheck<>(labels, COMPLETE_ALL_TOKENS, true));
					engine.ComparativeCheck( records.Node( nodeId ), new NodeInUseWithCorrectLabelsCheck<RECORD, ?, REPORT>( labels, COMPLETE_ALL_TOKENS, true ) );
			  }
		 }
	}

}