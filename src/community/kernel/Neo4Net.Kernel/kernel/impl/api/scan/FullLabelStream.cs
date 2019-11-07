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
namespace Neo4Net.Kernel.Impl.Api.scan
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using Neo4Net.Collections.Helpers;
	using LabelScanWriter = Neo4Net.Kernel.Api.LabelScan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.Api.LabelScan.NodeLabelUpdate;
	using IndexStoreView = Neo4Net.Kernel.Impl.Api.index.IndexStoreView;
	using Neo4Net.Kernel.Impl.Api.index;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.function.Predicates.ALWAYS_TRUE_INT;

	/// <summary>
	/// <seealso cref="FullStoreChangeStream"/> using a <seealso cref="IndexStoreView"/> to get its data.
	/// </summary>
	public class FullLabelStream : FullStoreChangeStream, Visitor<NodeLabelUpdate, IOException>
	{
		 private readonly IndexStoreView _indexStoreView;
		 private LabelScanWriter _writer;
		 private long _count;

		 public FullLabelStream( IndexStoreView indexStoreView )
		 {
			  this._indexStoreView = indexStoreView;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long applyTo(Neo4Net.kernel.api.labelscan.LabelScanWriter writer) throws java.io.IOException
		 public override long ApplyTo( LabelScanWriter writer )
		 {
			  // Keep the write for using it in visit
			  this._writer = writer;
			  StoreScan<IOException> scan = _indexStoreView.visitNodes( ArrayUtils.EMPTY_INT_ARRAY, ALWAYS_TRUE_INT, null, this, true );
			  scan.Run();
			  return _count;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(Neo4Net.kernel.api.labelscan.NodeLabelUpdate update) throws java.io.IOException
		 public override bool Visit( NodeLabelUpdate update )
		 {
			  _writer.write( update );
			  _count++;
			  return false;
		 }
	}

}