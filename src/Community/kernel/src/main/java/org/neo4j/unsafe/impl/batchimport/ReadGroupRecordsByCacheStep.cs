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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Neo4Net.Kernel.impl.store;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using GroupVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache.GroupVisitor;
	using NodeChangeVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache.NodeChangeVisitor;
	using NodeType = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeType;
	using ProducerStep = Neo4Net.@unsafe.Impl.Batchimport.staging.ProducerStep;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;

	/// <summary>
	/// Using the <seealso cref="NodeRelationshipCache"/> efficiently looks for changed nodes and reads those
	/// <seealso cref="NodeRecord"/> and sends downwards.
	/// </summary>
	public class ReadGroupRecordsByCacheStep : ProducerStep
	{
		 private readonly RecordStore<RelationshipGroupRecord> _store;
		 private readonly NodeRelationshipCache _cache;

		 public ReadGroupRecordsByCacheStep( StageControl control, Configuration config, RecordStore<RelationshipGroupRecord> store, NodeRelationshipCache cache ) : base( control, config )
		 {
			  this._store = store;
			  this._cache = cache;
		 }

		 protected internal override void Process()
		 {
			  using ( NodeVisitor visitor = new NodeVisitor( this ) )
			  {
					_cache.visitChangedNodes( visitor, NodeType.NODE_TYPE_DENSE );
			  }
		 }

		 private class NodeVisitor : NodeRelationshipCache.NodeChangeVisitor, AutoCloseable, NodeRelationshipCache.GroupVisitor, System.Func<RelationshipGroupRecord[]>
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 Assembler = new RecordDataAssembler<RelationshipGroupRecord>( outerInstance.store.newRecord );
				 Batch = Get();
			 }

			 private readonly ReadGroupRecordsByCacheStep _outerInstance;

			 public NodeVisitor( ReadGroupRecordsByCacheStep outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			  internal RecordDataAssembler<RelationshipGroupRecord> Assembler;
			  internal RelationshipGroupRecord[] Batch;
			  internal int Cursor;
			  internal long Time = nanoTime();

			  public override void Change( long nodeId, ByteArray array )
			  {
					outerInstance.cache.GetFirstRel( nodeId, this );
			  }

			  public override long Visit( long nodeId, int typeId, long @out, long @in, long loop )
			  {
					long id = outerInstance.store.nextId();
					RelationshipGroupRecord record = Batch[Cursor++];
					record.Id = id;
					record.Initialize( true, typeId, @out, @in, loop, nodeId, loop );
					if ( Cursor == outerInstance.BatchSize )
					{
						 Send();
						 Batch = control.reuse( this );
						 Cursor = 0;
					}
					return id;
			  }

			  internal virtual void Send()
			  {
					totalProcessingTime.add( nanoTime() - Time );
					outerInstance.SendDownstream( Batch );
					Time = nanoTime();
					assertHealthy();
			  }

			  public override void Close()
			  {
					if ( Cursor > 0 )
					{
						 Batch = Assembler.cutOffAt( Batch, Cursor );
						 Send();
					}
			  }

			  public override RelationshipGroupRecord[] Get()
			  {
					return Assembler.newBatchObject( outerInstance.BatchSize );
			  }
		 }

		 protected internal override long Position()
		 {
			  return _store.HighId * _store.RecordSize;
		 }
	}

}