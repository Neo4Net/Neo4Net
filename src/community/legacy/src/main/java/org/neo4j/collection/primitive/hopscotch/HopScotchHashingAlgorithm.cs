using System.Diagnostics;

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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using IHashFunction = Neo4Net.Hashing.HashFunction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.numberOfLeadingZeros;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.numberOfTrailingZeros;

	/// <summary>
	/// <para>
	/// An implementation of the hop-scotch algorithm, see http://en.wikipedia.org/wiki/Hopscotch_hashing.
	/// It's a static set of methods that implements the essence of the algorithm, where storing and retrieving data is
	/// abstracted into the <seealso cref="Table"/> interface. Also things like <seealso cref="Monitor monitoring"/> and choice of
	/// <seealso cref="HashFunction"/> gets passed in.
	/// </para>
	/// 
	/// <para>
	/// About hop scotching and hop bits: Each index in the table has hop bits which is a list of which nearby
	/// indexes contain entries with a key hashing to the same index as itself - its neighborhood.
	/// A neighbor can at most be {@code H} indexes away. This is the collision resolution method of choice
	/// for the hop scotch algorithm. Getting and putting entries at an index first checks the index at hand.
	/// If occupied by another entry its neighbors, dictated by the hop bits, are checked.
	/// </para>
	/// <para>
	/// When putting an entry and the index and all its neighbors are occupied, the hop scotching begins, where a free
	/// index further away is picked and iteratively moved closer and closer until it's within the neighborhood
	/// of the intended index. The entry is then placed at that newly "freed" index.
	/// </para>
	/// <para>
	/// Removing an entry will put some effort into do reverse hop scotching as well, i.e. moving a neighbor into
	/// the newly removed index and iteratively also move neighbors of the moved neighbor, and so forth.
	/// </para>
	/// <para>
	/// This behavior has the benefit of keeping entries hashing to the same index very close together,
	/// and so will take more advantage of CPU caches and pre-fetching in general, especially for lookups.
	/// </para>
	/// 
	/// <para>
	/// Why are these methods (like <seealso cref="put(Table, Monitor, IHashFunction, long, object, ResizeMonitor)"/>,
	/// <seealso cref="get(Table, Monitor, IHashFunction, long)"/> a.s.o. static? To reduce garbage and also reduce overhead of each
	/// set or map object making use of hop-scotch hashing where they won't need to have a reference to an algorithm
	/// object, merely use its static methods. Also, all essential state is managed by <seealso cref="Table"/>.
	/// </para>
	/// </summary>
	public class HopScotchHashingAlgorithm
	{
		 /// <summary>
		 /// Default number of hop bits per index, i.e. size of neighborhood.
		 /// </summary>
		 public const int DEFAULT_H = 32;

		 private HopScotchHashingAlgorithm()
		 {
		 }

		 public static VALUE Get<VALUE>( Table<VALUE> table, Monitor monitor, IHashFunction hashFunction, long key )
		 {
			  int tableMask = table.Mask();
			  int index = IndexOf( hashFunction, key, tableMask );
			  long existingKey = table.Key( index );
			  if ( existingKey == key )
			  { // Bulls eye
					return table.Value( index );
			  }

			  // Look in its neighborhood
			  long hopBits = table.HopBits( index );
			  while ( hopBits > 0 )
			  {
					int hopIndex = NextIndex( index, numberOfTrailingZeros( hopBits ) + 1, tableMask );
					if ( table.Key( hopIndex ) == key )
					{ // There it is
						 return table.Value( hopIndex );
					}
					hopBits &= hopBits - 1;
			  }

			  return default( VALUE );
		 }

		 public static VALUE Remove<VALUE>( Table<VALUE> table, Monitor monitor, IHashFunction hashFunction, long key )
		 {
			  int tableMask = table.Mask();
			  int index = IndexOf( hashFunction, key, tableMask );
			  int freedIndex = -1;
			  VALUE result = default( VALUE );
			  if ( table.Key( index ) == key )
			  { // Bulls eye
					freedIndex = index;
					result = table.Remove( index );
			  }

			  // Look in its neighborhood
			  long hopBits = table.HopBits( index );
			  while ( hopBits > 0 )
			  {
					int hd = numberOfTrailingZeros( hopBits );
					int hopIndex = NextIndex( index, hd + 1, tableMask );
					if ( table.Key( hopIndex ) == key )
					{ // there it is
						 freedIndex = hopIndex;
						 result = table.Remove( hopIndex );
						 table.RemoveHopBit( index, hd );
					}
					hopBits &= hopBits - 1;
			  }

			  // reversed hop-scotching, i.e. pull in the most distant neighbor, iteratively as long as the
			  // pulled index has neighbors of its own
			  while ( freedIndex != -1 )
			  {
					long freedHopBits = table.HopBits( freedIndex );
					if ( freedHopBits > 0 )
					{ // It's got a neighbor, go ahead and move it here
						 int hd = 63 - numberOfLeadingZeros( freedHopBits );
						 int candidateIndex = NextIndex( freedIndex, hd + 1, tableMask );
						 // move key/value
						 long candidateKey = table.Move( candidateIndex, freedIndex );
						 // remove that hop bit, since that one is no longer a neighbor, it's "the one" at the index
						 table.RemoveHopBit( freedIndex, hd );
						 Debug.Assert( monitor.PulledToFreeIndex( index, table.HopBits( freedIndex ), candidateKey, candidateIndex, freedIndex ) );
						 freedIndex = candidateIndex;
					}
					else
					{
						 freedIndex = -1;
					}
			  }

			  return result;
		 }

		 public static VALUE Put<VALUE>( Table<VALUE> table, Monitor monitor, IHashFunction hashFunction, long key, VALUE value, ResizeMonitor<VALUE> resizeMonitor )
		 {
			  long nullKey = table.NullKey();
			  Debug.Assert( key != nullKey );
			  int tableMask = table.Mask();
			  int index = IndexOf( hashFunction, key, tableMask );
			  long keyAtIndex = table.Key( index );
			  if ( keyAtIndex == nullKey )
			  { // this index is free, just place it there
					table.Put( index, key, value );
					Debug.Assert( monitor.PlacedAtFreeAndIntendedIndex( key, index ) );
					return default( VALUE );
			  }
			  else if ( keyAtIndex == key )
			  { // this index is occupied, but actually with the same key
					return table.PutValue( index, value );
			  }
			  else
			  { // look at the neighbors of this entry to see if any is the requested key
					long hopBits = table.HopBits( index );
					while ( hopBits > 0 )
					{
						 int hopIndex = NextIndex( index, numberOfTrailingZeros( hopBits ) + 1, tableMask );
						 if ( table.Key( hopIndex ) == key )
						 {
							  return table.PutValue( hopIndex, value );
						 }
						 hopBits &= hopBits - 1;
					}
			  }

			  // this key does not exist in this set. put it there using hop-scotching
			  if ( HopScotchPut( table, monitor, hashFunction, key, value, index, tableMask, nullKey ) )
			  { // we managed to wiggle our way to a free spot and put it there
					return default( VALUE );
			  }

			  // we couldn't add this value, even in the H-1 neighborhood, so grow table...
			  GrowTable( table, monitor, hashFunction, resizeMonitor );
			  Table<VALUE> resizedTable = resizeMonitor.LastTable;

			  // ...and try again
			  return Put( resizedTable, monitor, hashFunction, key, value, resizeMonitor );
		 }

		 private static bool HopScotchPut<VALUE>( Table<VALUE> table, Monitor monitor, IHashFunction hashFunction, long key, VALUE value, int index, int tableMask, long nullKey )
		 {
			  int freeIndex = NextIndex( index, 1, tableMask );
			  int h = table.H();
			  int totalHd = 0; // h delta, i.e. distance from first neighbor to current tentative index, the first neighbor has hd=0
			  bool foundFreeSpot = false;

			  // linear probe for finding a free slot in ASC index direction
			  while ( freeIndex != index ) // one round is enough, albeit far, but at the same time very unlikely
			  {
					if ( table.Key( freeIndex ) == nullKey )
					{ // free slot found
						 foundFreeSpot = true;
						 break;
					}

					// move on to the next index in the search for a free slot
					freeIndex = NextIndex( freeIndex, 1, tableMask );
					totalHd++;
			  }

			  if ( !foundFreeSpot )
			  {
					return false;
			  }

			  while ( totalHd >= h )
			  { // grab a closer index and see which of its neighbors is OK to move further away,
					// so that there will be a free space to place the new value. I.e. move the free space closer
					// and some close neighbors a bit further away (although never outside its neighborhood)
					int neighborIndex = NextIndex( freeIndex, -( h - 1 ), tableMask ); // hopscotch hashing says to try h-1 entries closer

					bool swapped = false;
					for ( int d = 0; d < ( h >> 1 ) && !swapped; d++ )
					{ // examine hop information (i.e. is there's someone in the neighborhood here to swap with 'hopIndex'?)
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long neighborHopBitsFixed = table.hopBits(neighborIndex);
						 long neighborHopBitsFixed = table.HopBits( neighborIndex );
						 long neighborHopBits = neighborHopBitsFixed;
						 while ( neighborHopBits > 0 && !swapped )
						 {
							  int hd = numberOfTrailingZeros( neighborHopBits );
							  if ( hd + d >= h - 1 )
							  { // that would be too far
									break;
							  }
							  neighborHopBits &= neighborHopBits - 1;
							  int candidateIndex = NextIndex( neighborIndex, hd + 1, tableMask );

							  // OK, here's a neighbor, let's examine it's neighbors (candidates to move)
							  //  - move the candidate entry (incl. updating its hop bits) to the free index
							  int distance = ( freeIndex - candidateIndex ) & tableMask;
							  long candidateKey = table.Move( candidateIndex, freeIndex );
							  //  - update the neighbor entry with the move of the candidate entry
							  table.MoveHopBit( neighborIndex, hd, distance );
							  Debug.Assert( monitor.PushedToFreeIndex( index, neighborHopBitsFixed, table.HopBits( neighborIndex ), neighborIndex, candidateKey, candidateIndex, freeIndex ) );
							  freeIndex = candidateIndex;
							  swapped = true;
							  totalHd -= distance;
						 }
						 if ( !swapped )
						 {
							  neighborIndex = NextIndex( neighborIndex, 1, tableMask );
						 }
					}

					if ( !swapped )
					{ // we could not make any room to swap, tell that to the outside world
						 return false;
					}
			  }

			  // OK, now we're within distance to just place it there. Do it
			  table.Put( freeIndex, key, value );
			  // and update the hop bits of "index"
			  table.PutHopBit( index, totalHd );
			  Debug.Assert( monitor.PlacedAtFreedIndex( index, table.HopBits( index ), key, freeIndex ) );

			  return true;
		 }

		 private static int NextIndex( int index, int delta, int mask )
		 {
			  return ( index + delta ) & mask;
		 }

		 private static int IndexOf( IHashFunction hashFunction, long key, int tableMask )
		 {
			  return hashFunction.HashSingleValueToInt( key ) & tableMask;
		 }

		 private static void GrowTable<VALUE>( Table<VALUE> oldTable, Monitor monitor, IHashFunction hashFunction, ResizeMonitor<VALUE> resizeMonitor )
		 {
			  Debug.Assert( monitor.TableGrowing( oldTable.Capacity(), oldTable.Size() ) );
			  Table<VALUE> newTable = oldTable.Grow();
			  // Install the new table before populating it with the old data, in case we find it needs to grow even more
			  // while we are populating it. If that happens, we want to end up with the table installed by the final grow.
			  resizeMonitor.TableGrew( newTable );
			  long nullKey = oldTable.NullKey();

			  // place all entries in the new table
			  int capacity = oldTable.Capacity();
			  for ( int i = 0; i < capacity; i++ )
			  {
					long key = oldTable.Key( i );
					if ( key != nullKey )
					{
						 // Always use the table from the resize monitor, because any put can cause a grow.
						 Table<VALUE> table = resizeMonitor.LastTable;
						 VALUE putResult = Put( table, monitor, hashFunction, key, oldTable.Value( i ), resizeMonitor );
						 if ( putResult != default( VALUE ) )
						 {
							  // If we somehow fail to populate the new table, reinstall the old one.
							  resizeMonitor.TableGrew( oldTable );
							  newTable.Close();
							  throw new System.InvalidOperationException( "Couldn't add " + key + " when growing table" );
						 }
					}
			  }
			  Debug.Assert( monitor.TableGrew( oldTable.Capacity(), newTable.Capacity(), newTable.Size() ) );
			  oldTable.Close();
		 }

		 /// <summary>
		 /// Monitor for what how a <seealso cref="HopScotchHashingAlgorithm"/> changes the items in a <seealso cref="Table"/>.
		 /// </summary>
		 public interface Monitor
		 {
			  bool TableGrowing( int fromCapacity, int currentSize );

			  bool TableGrew( int fromCapacity, int toCapacity, int currentSize );

			  bool PlacedAtFreeAndIntendedIndex( long key, int index );

			  bool PushedToFreeIndex( int intendedIndex, long oldHopBits, long newHopBits, int neighborIndex, long key, int fromIndex, int toIndex );

			  bool PlacedAtFreedIndex( int intendedIndex, long newHopBits, long key, int actualIndex );

			  bool PulledToFreeIndex( int intendedIndex, long newHopBits, long key, int fromIndex, int toIndex );
		 }

		  public abstract class Monitor_Adapter : Monitor
		  {
			  private readonly HopScotchHashingAlgorithm _outerInstance;

			  public Monitor_Adapter( HopScotchHashingAlgorithm outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override bool PlacedAtFreedIndex( int intendedIndex, long newHopBits, long key, int actualIndex )
				{
					 return true;
				}

				public override bool PlacedAtFreeAndIntendedIndex( long key, int index )
				{
					 return true;
				}

				public override bool PushedToFreeIndex( int intendedIndex, long oldHopBits, long newHopBits, int neighborIndex, long key, int fromIndex, int toIndex )
				{
					 return true;
				}

				public override bool PulledToFreeIndex( int intendedIndex, long newHopBits, long key, int fromIndex, int toIndex )
				{
					 return true;
				}

				public override bool TableGrowing( int fromCapacity, int currentSize )
				{
					 return true;
				}

				public override bool TableGrew( int fromCapacity, int toCapacity, int currentSize )
				{
					 return true;
				}
		  }

		 public static readonly Monitor NO_MONITOR = new Monitor_AdapterAnonymousInnerClass();

		 private class Monitor_AdapterAnonymousInnerClass : Monitor_Adapter
		 {
					/*No additional logic*/
		 }

		 /// <summary>
		 /// The default hash function for primitive collections. This hash function is quite fast but has mediocre
		 /// statistics.
		 /// </summary>
		 /// <seealso cref= HashFunctionHelper#XorShift32() </seealso>
		 internal static readonly IHashFunction DefaultHashing = HashFunctionHelper.XorShift32();

		 public interface ResizeMonitor<VALUE>
		 {
			  void TableGrew( Table<VALUE> newTable );

			  Table<VALUE> LastTable { get; }
		 }
	}

}