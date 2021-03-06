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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{

	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using ByteArray = Org.Neo4j.@unsafe.Impl.Batchimport.cache.ByteArray;
	using NodeRelationshipCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using NodeChangeVisitor = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache.NodeChangeVisitor;
	using ProducerStep = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ProducerStep;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.iterator;

	/// <summary>
	/// Using the <seealso cref="NodeRelationshipCache"/> efficiently looks for changed nodes and reads those
	/// <seealso cref="NodeRecord"/> and sends downwards.
	/// </summary>
	public class ReadNodeIdsByCacheStep : ProducerStep
	{
		 private readonly int _nodeTypes;
		 private readonly NodeRelationshipCache _cache;
		 private volatile long _highId;

		 public ReadNodeIdsByCacheStep( StageControl control, Configuration config, NodeRelationshipCache cache, int nodeTypes ) : base( control, config )
		 {
			  this._cache = cache;
			  this._nodeTypes = nodeTypes;
		 }

		 protected internal override void Process()
		 {
			  using ( NodeVisitor visitor = new NodeVisitor( this ) )
			  {
					_cache.visitChangedNodes( visitor, _nodeTypes );
			  }
		 }

		 private class NodeVisitor : NodeRelationshipCache.NodeChangeVisitor, AutoCloseable
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 Batch = new long[outerInstance.BatchSize];
			 }

			 private readonly ReadNodeIdsByCacheStep _outerInstance;

			 public NodeVisitor( ReadNodeIdsByCacheStep outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			  internal long[] Batch;
			  internal int Cursor;
			  internal long Time = nanoTime();

			  public override void Change( long nodeId, ByteArray array )
			  {
					Batch[Cursor++] = nodeId;
					if ( Cursor == outerInstance.BatchSize )
					{
						 Send();
						 Batch = new long[outerInstance.BatchSize];
						 Cursor = 0;
					}
			  }

			  internal virtual void Send()
			  {
					totalProcessingTime.add( nanoTime() - Time );
					outerInstance.SendDownstream( iterator( Batch ) );
					Time = nanoTime();
					assertHealthy();
					outerInstance.highId = Batch[Cursor - 1];
			  }

			  public override void Close()
			  {
					if ( Cursor > 0 )
					{
						 Batch = Arrays.copyOf( Batch, Cursor );
						 Send();
					}
			  }
		 }

		 protected internal override long Position()
		 {
			  return _highId * Long.BYTES;
		 }
	}

}