﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Api.scan
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using Org.Neo4j.Helpers.Collection;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using Org.Neo4j.Kernel.Impl.Api.index;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.ALWAYS_TRUE_INT;

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
//ORIGINAL LINE: public long applyTo(org.neo4j.kernel.api.labelscan.LabelScanWriter writer) throws java.io.IOException
		 public override long ApplyTo( LabelScanWriter writer )
		 {
			  // Keep the write for using it in visit
			  this._writer = writer;
			  StoreScan<IOException> scan = _indexStoreView.visitNodes( ArrayUtils.EMPTY_INT_ARRAY, ALWAYS_TRUE_INT, null, this, true );
			  scan.Run();
			  return _count;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.kernel.api.labelscan.NodeLabelUpdate update) throws java.io.IOException
		 public override bool Visit( NodeLabelUpdate update )
		 {
			  _writer.write( update );
			  _count++;
			  return false;
		 }
	}

}