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
namespace Neo4Net.Kernel.Api.Impl.Schema.verification
{
	using Document = org.apache.lucene.document.Document;
	using LeafReader = Org.Apache.Lucene.Index.LeafReader;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using SimpleCollector = org.apache.lucene.search.SimpleCollector;

	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using BucketsDuplicateCheckStrategy = Neo4Net.Kernel.Api.Impl.Schema.verification.DuplicateCheckStrategy.BucketsDuplicateCheckStrategy;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using Value = Neo4Net.Values.Storable.Value;

	public class DuplicateCheckingCollector : SimpleCollector
	{
		 protected internal readonly NodePropertyAccessor Accessor;
		 private readonly int _propertyKeyId;
		 protected internal LeafReader Reader;
		 internal DuplicateCheckStrategy DuplicateCheckStrategy;

		 internal static DuplicateCheckingCollector ForProperties( NodePropertyAccessor accessor, int[] propertyKeyIds )
		 {
			  return ( propertyKeyIds.Length == 1 ) ? new DuplicateCheckingCollector( accessor, propertyKeyIds[0] ) : new CompositeDuplicateCheckingCollector( accessor, propertyKeyIds );
		 }

		 internal DuplicateCheckingCollector( NodePropertyAccessor accessor, int propertyKeyId )
		 {
			  this.Accessor = accessor;
			  this._propertyKeyId = propertyKeyId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void collect(int doc) throws java.io.IOException
		 public override void Collect( int doc )
		 {
			  try
			  {
					DoCollect( doc );
			  }
			  catch ( KernelException e )
			  {
					throw new System.InvalidOperationException( "Indexed node should exist and have the indexed property.", e );
			  }
			  catch ( IndexEntryConflictException e )
			  {
					throw new IOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void doCollect(int doc) throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.KernelException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 protected internal virtual void DoCollect( int doc )
		 {
			  Document document = Reader.document( doc );
			  long nodeId = LuceneDocumentStructure.getNodeId( document );
			  Value value = Accessor.getNodePropertyValue( nodeId, _propertyKeyId );
			  DuplicateCheckStrategy.checkForDuplicate( value, nodeId );
		 }

		 protected internal override void DoSetNextReader( LeafReaderContext context )
		 {
			  this.Reader = context.reader();
		 }

		 public override bool NeedsScores()
		 {
			  return false;
		 }

		 /// <summary>
		 /// Initialise collector for unknown number of entries that are suspected to be duplicates.
		 /// </summary>
		 public virtual void Init()
		 {
			  DuplicateCheckStrategy = new BucketsDuplicateCheckStrategy();
		 }

		 /// <summary>
		 /// Initialize collector for some known and expected number of entries that are suspected to be duplicates. </summary>
		 /// <param name="expectedNumberOfEntries"> expected number entries </param>
		 public virtual void Init( int expectedNumberOfEntries )
		 {
			  if ( UseFastCheck( expectedNumberOfEntries ) )
			  {
					DuplicateCheckStrategy = new DuplicateCheckStrategy.MapDuplicateCheckStrategy( expectedNumberOfEntries );
			  }
			  else
			  {
					DuplicateCheckStrategy = new BucketsDuplicateCheckStrategy( expectedNumberOfEntries );
			  }
		 }

		 private bool UseFastCheck( int expectedNumberOfEntries )
		 {
			  return expectedNumberOfEntries <= BucketsDuplicateCheckStrategy.BUCKET_STRATEGY_ENTRIES_THRESHOLD;
		 }
	}

}