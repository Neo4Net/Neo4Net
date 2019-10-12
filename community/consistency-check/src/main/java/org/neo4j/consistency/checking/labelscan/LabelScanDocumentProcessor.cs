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
namespace Org.Neo4j.Consistency.checking.labelscan
{
	using Org.Neo4j.Consistency.checking.full;
	using ConsistencyReporter = Org.Neo4j.Consistency.report.ConsistencyReporter;
	using LabelScanDocument = Org.Neo4j.Consistency.store.synthetic.LabelScanDocument;
	using NodeLabelRange = Org.Neo4j.Kernel.api.labelscan.NodeLabelRange;

	public class LabelScanDocumentProcessor : Org.Neo4j.Consistency.checking.full.RecordProcessor_Adapter<NodeLabelRange>
	{
		 private readonly ConsistencyReporter _reporter;
		 private readonly LabelScanCheck _labelScanCheck;

		 public LabelScanDocumentProcessor( ConsistencyReporter reporter, LabelScanCheck labelScanCheck )
		 {
			  this._reporter = reporter;
			  this._labelScanCheck = labelScanCheck;
		 }

		 public override void Process( NodeLabelRange nodeLabelRange )
		 {
			  _reporter.forNodeLabelScan( new LabelScanDocument( nodeLabelRange ), _labelScanCheck );
		 }
	}

}