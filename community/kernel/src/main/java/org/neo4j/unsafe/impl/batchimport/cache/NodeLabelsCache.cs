using System.Diagnostics;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	using Bits = Org.Neo4j.Kernel.impl.util.Bits;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.numberOfLeadingZeros;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bitsFromLongs;

	/// <summary>
	/// Caches labels for each node. Tries to keep memory as 8b (a long) per node. If a particular node has many labels
	/// it will spill over into two or more longs in a separate array.
	/// </summary>
	public class NodeLabelsCache : MemoryStatsVisitor_Visitable, AutoCloseable
	{
		 public class Client
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 FieldBits = bitsFromLongs( FieldScratch );
			 }

			  internal readonly long[] LabelScratch;
			  internal readonly Bits LabelBits;
			  internal readonly long[] FieldScratch = new long[1];
			  internal Bits FieldBits;

			  public Client( int worstCaseLongsNeeded )
			  {
				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
					this.LabelScratch = new long[worstCaseLongsNeeded];
					this.LabelBits = bitsFromLongs( LabelScratch );
			  }
		 }

		 private readonly LongArray _cache;
		 private readonly LongArray _spillOver;
		 private long _spillOverIndex;
		 private readonly int _bitsPerLabel;
		 private readonly int _worstCaseLongsNeeded;
		 private readonly Client _putClient;

		 public NodeLabelsCache( NumberArrayFactory cacheFactory, int highLabelId ) : this( cacheFactory, highLabelId, 10_000_000 )
		 {
		 }

		 public NodeLabelsCache( NumberArrayFactory cacheFactory, int highLabelId, int chunkSize )
		 {
			  this._cache = cacheFactory.NewDynamicLongArray( chunkSize, 0 );
			  this._spillOver = cacheFactory.NewDynamicLongArray( chunkSize / 5, 0 ); // expect way less of these
			  this._bitsPerLabel = max( ( sizeof( int ) * 8 ) - numberOfLeadingZeros( highLabelId ), 1 );
			  this._worstCaseLongsNeeded = ( ( _bitsPerLabel * ( highLabelId + 1 ) ) - 1 ) / ( sizeof( long ) * 8 ) + 1;
			  this._putClient = new Client( _worstCaseLongsNeeded );
		 }

		 /// <returns> a new <seealso cref="Client"/> used in <seealso cref="get(Client, long, int[])"/>. <seealso cref="Client"/> contains
		 /// mutable state and so each thread calling <seealso cref="get(Client, long, int[])"/> must create their own
		 /// client instance once and (re)use it for every get-call they do. </returns>
		 public virtual Client NewClient()
		 {
			  return new Client( _worstCaseLongsNeeded );
		 }

		 /// <summary>
		 /// Keeps label ids for the given node id. Labels ids are int[] really, but by accident they arrive
		 /// from the store disguised as long[]. When looping over them there can be assumed that they are ints.
		 /// 
		 /// The format is that the longs in this cache are divided up into bit slots of size whatever bitsPerLabel is.
		 /// The first slot will contain number of labels for this node. If those labels fit in the long, after the
		 /// length slot, they will be stored there. Otherwise the rest of the bits will point to the index into
		 /// the spillOver array.
		 /// 
		 /// This method may only be called by a single thread, putting from multiple threads may cause undeterministic
		 /// behaviour.
		 /// </summary>
		 public virtual void Put( long nodeId, long[] labelIds )
		 {
			  _putClient.labelBits.clear( true );
			  _putClient.labelBits.put( labelIds.Length, _bitsPerLabel );
			  foreach ( long labelId in labelIds )
			  {
					_putClient.labelBits.put( ( int ) labelId, _bitsPerLabel );
			  }

			  int longsInUse = _putClient.labelBits.longsInUse();
			  Debug.Assert( longsInUse > 0, "Uhm" );
			  if ( longsInUse == 1 )
			  { // We only require one long, so put it right in there
					_cache.set( nodeId, _putClient.labelScratch[0] );
			  }
			  else
			  { // Now it gets tricky, we have to spill over into another array
					// So create the reference
					_putClient.fieldBits.clear( true );
					_putClient.fieldBits.put( labelIds.Length, _bitsPerLabel );
					_putClient.fieldBits.put( _spillOverIndex, ( sizeof( long ) * 8 ) - _bitsPerLabel );
					_cache.set( nodeId, _putClient.fieldBits.Longs[0] );

					// And set the longs in the spill over array. For simplicity we put the encoded bits as they
					// are right into the spill over array, where the first slot will have the length "again".
					for ( int i = 0; i < longsInUse; i++ )
					{
						 _spillOver.set( _spillOverIndex++, _putClient.labelScratch[i] );
					}
			  }
		 }

		 /// <summary>
		 /// Write labels for a node into {@code target}. If target isn't big enough it will grow.
		 /// The target, intact or grown, will be returned.
		 /// 
		 /// Multiple threads may call this method simultaneously, given that they do so with each their own <seealso cref="Client"/>
		 /// instance.
		 /// </summary>
		 public virtual int[] Get( Client client, long nodeId, int[] target )
		 {
			  // make this field available to our Bits instance, hackish? meh
			  client.FieldBits.clear( false );
			  client.FieldScratch[0] = _cache.get( nodeId );
			  if ( client.FieldScratch[0] == 0 )
			  { // Nothing here
					target[0] = -1; // mark the end
					return target;
			  }

			  int length = client.FieldBits.getInt( _bitsPerLabel );
			  int longsInUse = ( ( _bitsPerLabel * ( length + 1 ) ) - 1 ) / ( sizeof( long ) * 8 ) + 1;
			  target = EnsureCapacity( target, length );
			  if ( longsInUse == 1 )
			  {
					Decode( client.FieldBits, length, target );
			  }
			  else
			  {
					// Read data from spill over cache into the label bits array for decoding
					long spillOverIndex = client.FieldBits.getLong( ( sizeof( long ) * 8 ) - _bitsPerLabel );
					client.LabelBits.clear( false );
					for ( int i = 0; i < longsInUse; i++ )
					{
						 client.LabelScratch[i] = _spillOver.get( spillOverIndex + i );
					}
					client.LabelBits.getInt( _bitsPerLabel ); // first one ignored, since it's just the length
					Decode( client.LabelBits, length, target );
			  }

			  return target;
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  _cache.acceptMemoryStatsVisitor( visitor );
			  _spillOver.acceptMemoryStatsVisitor( visitor );
		 }

		 private void Decode( Bits bits, int length, int[] target )
		 {
			  for ( int i = 0; i < length; i++ )
			  {
					target[i] = bits.GetInt( _bitsPerLabel );
			  }

			  if ( target.Length > length )
			  { // we have to mark the end here, since the target array is larger
					target[length] = -1;
			  }
		 }

		 private static int[] EnsureCapacity( int[] target, int capacity )
		 {
			  return capacity > target.Length ? new int[capacity] : target;
		 }

		 public override void Close()
		 {
			  _cache.close();
			  _spillOver.close();
		 }
	}

}