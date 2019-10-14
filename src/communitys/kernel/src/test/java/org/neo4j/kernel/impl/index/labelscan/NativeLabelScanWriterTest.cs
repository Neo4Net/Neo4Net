using System.Collections;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using ValueMergers = Neo4Net.Index.@internal.gbptree.ValueMergers;
	using Neo4Net.Index.@internal.gbptree;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.asArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.labelscan.NativeLabelScanStoreIT.flipRandom;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.labelscan.NativeLabelScanStoreIT.getLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.labelscan.NativeLabelScanStoreIT.nodesWithLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.LabelScanReader_Fields.NO_ID;

	public class NativeLabelScanWriterTest
	{
		 private const int LABEL_COUNT = 5;
		 private const int NODE_COUNT = 10_000;
		 private static readonly IComparer<LabelScanKey> _keyComparator = new LabelScanLayout();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddLabels()
		 {
			  // GIVEN
			  ControlledInserter inserter = new ControlledInserter();
			  long[] expected = new long[NODE_COUNT];
			  using ( NativeLabelScanWriter writer = new NativeLabelScanWriter( max( 5, NODE_COUNT / 100 ), NativeLabelScanWriter.EMPTY ) )
			  {
					writer.Initialize( inserter );

					// WHEN
					for ( int i = 0; i < NODE_COUNT * 3; i++ )
					{
						 NodeLabelUpdate update = RandomUpdate( expected );
						 writer.Write( update );
					}
			  }

			  // THEN
			  for ( int i = 0; i < LABEL_COUNT; i++ )
			  {
					long[] expectedNodeIds = nodesWithLabel( expected, i );
					long[] actualNodeIds = asArray( new LabelScanValueIterator( inserter.NodesFor( i ), new List<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>>(), NO_ID ) );
					assertArrayEquals( "For label " + i, expectedNodeIds, actualNodeIds );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptUnsortedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptUnsortedLabels()
		 {
			  // GIVEN
			  ControlledInserter inserter = new ControlledInserter();
			  bool failed = false;
			  try
			  {
					  using ( NativeLabelScanWriter writer = new NativeLabelScanWriter( 1, NativeLabelScanWriter.EMPTY ) )
					  {
						writer.Initialize( inserter );
      
						// WHEN
						writer.Write( NodeLabelUpdate.labelChanges( 0, EMPTY_LONG_ARRAY, new long[] { 2, 1 } ) );
						// we can't do the usual "fail( blabla )" here since the actual write will happen
						// when closing this writer, i.e. in the curly bracket below.
					  }
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN
					assertTrue( e.Message.contains( "unsorted" ) );
					failed = true;
			  }

			  assertTrue( failed );
		 }

		 private NodeLabelUpdate RandomUpdate( long[] expected )
		 {
			  int nodeId = Random.Next( expected.Length );
			  long labels = expected[nodeId];
			  long[] before = getLabels( labels );
			  int changeCount = Random.Next( 4 ) + 1;
			  for ( int i = 0; i < changeCount; i++ )
			  {
					labels = flipRandom( labels, LABEL_COUNT, Random.random() );
			  }
			  expected[nodeId] = labels;
			  return NodeLabelUpdate.labelChanges( nodeId, before, getLabels( labels ) );
		 }

		 private class ControlledInserter : Writer<LabelScanKey, LabelScanValue>
		 {
			  internal readonly IDictionary<int, IDictionary<LabelScanKey, LabelScanValue>> Data = new Dictionary<int, IDictionary<LabelScanKey, LabelScanValue>>();

			  public override void Close()
			  { // Nothing to close
			  }

			  public override void Put( LabelScanKey key, LabelScanValue value )
			  {
					Merge( key, value, ValueMergers.overwrite() );
			  }

			  public override void Merge( LabelScanKey key, LabelScanValue value, ValueMerger<LabelScanKey, LabelScanValue> amender )
			  {
					// Clone since these instances are reused between calls, internally in the writer
					key = Clone( key );
					value = Clone( value );

					IDictionary<LabelScanKey, LabelScanValue> forLabel = Data.computeIfAbsent( key.LabelId, labelId => new SortedDictionary<LabelScanKey, LabelScanValue>( _keyComparator ) );
					LabelScanValue existing = forLabel[key];
					if ( existing == null )
					{
						 forLabel[key] = value;
					}
					else
					{
						 amender.Merge( key, key, existing, value );
					}
			  }

			  internal static LabelScanValue Clone( LabelScanValue value )
			  {
					LabelScanValue result = new LabelScanValue();
					result.Bits = value.Bits;
					return result;
			  }

			  internal static LabelScanKey Clone( LabelScanKey key )
			  {
					return new LabelScanKey( key.LabelId, key.IdRange );
			  }

			  public override LabelScanValue Remove( LabelScanKey key )
			  {
					throw new System.NotSupportedException( "Should not be called" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.cursor.RawCursor<org.neo4j.index.internal.gbptree.Hit<LabelScanKey,LabelScanValue>,java.io.IOException> nodesFor(int labelId)
			  internal virtual RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> NodesFor( int labelId )
			  {
					IDictionary<LabelScanKey, LabelScanValue> forLabel = Data[labelId];
					if ( forLabel == null )
					{
						 forLabel = Collections.emptyMap();
					}

					KeyValuePair<LabelScanKey, LabelScanValue>[] entries = forLabel.SetOfKeyValuePairs().toArray(new DictionaryEntry[forLabel.Count]);
					return new RawCursorAnonymousInnerClass( this, entries );
			  }

			  private class RawCursorAnonymousInnerClass : RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>
			  {
				  private readonly ControlledInserter _outerInstance;

				  private KeyValuePair<LabelScanKey, LabelScanValue>[] _entries;

				  public RawCursorAnonymousInnerClass( ControlledInserter outerInstance, KeyValuePair<LabelScanKey, LabelScanValue>[] entries )
				  {
					  this.outerInstance = outerInstance;
					  this._entries = entries;
					  arrayIndex = -1;
				  }

				  private int arrayIndex;

				  public Hit<LabelScanKey, LabelScanValue> get()
				  {
						KeyValuePair<LabelScanKey, LabelScanValue> entry = _entries[arrayIndex];
						return new MutableHit<LabelScanKey, LabelScanValue>( entry.Key, entry.Value );
				  }

				  public bool next()
				  {
						arrayIndex++;
						return arrayIndex < _entries.Length;
				  }

				  public void close()
				  { // Nothing to close
				  }
			  }
		 }
	}

}