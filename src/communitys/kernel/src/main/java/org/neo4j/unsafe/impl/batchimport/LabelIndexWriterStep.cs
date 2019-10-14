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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.labelscan.NodeLabelUpdate.labelChanges;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.get;

	public class LabelIndexWriterStep : ProcessorStep<NodeRecord[]>
	{
		 private readonly LabelScanWriter _writer;
		 private readonly NodeStore _nodeStore;

		 public LabelIndexWriterStep( StageControl control, Configuration config, LabelScanStore store, NodeStore nodeStore ) : base( control, "LABEL INDEX", config, 1 )
		 {
			  this._writer = store.NewWriter();
			  this._nodeStore = nodeStore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void process(org.neo4j.kernel.impl.store.record.NodeRecord[] batch, org.neo4j.unsafe.impl.batchimport.staging.BatchSender sender) throws Throwable
		 protected internal override void Process( NodeRecord[] batch, BatchSender sender )
		 {
			  foreach ( NodeRecord node in batch )
			  {
					if ( node.InUse() )
					{
						 _writer.write( labelChanges( node.Id, EMPTY_LONG_ARRAY, get( node, _nodeStore ) ) );
					}
			  }
			  sender.Send( batch );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  base.Close();
			  _writer.Dispose();
		 }
	}

}