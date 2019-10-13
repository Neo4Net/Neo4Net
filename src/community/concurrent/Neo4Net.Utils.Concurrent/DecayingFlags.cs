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
namespace Neo4Net.Utils.Concurrent
{

	using HexString = Neo4Net.@string.HexString;

	/// <summary>
	/// This is a concurrent data structure used to track
	/// some set if boolean flags. Any time a flag is set to high,
	/// it'll remain high for a period of time, after which
	/// it'll fall back to low again. How long the flags stay high
	/// depend on how often they are toggled - meaning this uses
	/// both recency and frequency to determine which flags to keep
	/// high.
	/// 
	/// The more often a flag is toggled high, the longer it'll
	/// take before it resets to low - if a flag gets set more
	/// often than sweep is called, it will always be high.
	/// 
	/// This data structure is coordination free, but sacrifices
	/// accuracy for performance.
	/// 
	/// Intended usage is that you'd have a set of keys you care
	/// about, and set a max time where you'd like to mark a key
	/// as low if that time passes; say 7 days.
	/// 
	/// So, you'd set <seealso cref="keepalive"/> to 7, and then you'd
	/// schedule a thread to call <seealso cref="sweep()"/> once per day.
	/// Now, flags that were toggled once will be set to low again
	/// the next day, while flags that were extensively used will
	/// take up to seven days before falling back to low.
	/// 
	/// Flags that are toggled at or more than once every day will
	/// always be high.
	/// </summary>
	public class DecayingFlags
	{
		 /// <summary>
		 /// A flag in the set, with a unique index pointing
		 /// to the bit that correlates to this flag.
		 /// </summary>
		 public class Key
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int IndexConflict;

			  public Key( int index )
			  {
					this.IndexConflict = index;
			  }

			  public virtual int Index()
			  {
					return IndexConflict;
			  }
		 }

		 /// <summary>
		 /// To model the time-based "decay" of the flags,
		 /// each flag is represented as an int that counts
		 /// the number of times the flag has been toggled, up to
		 /// <seealso cref="keepalive"/>. Each time #sweep is called, all
		 /// flags are decremented by 1, once a flag reaches 0 it
		 /// is considered low.
		 /// 
		 /// This way, if a flag is not "renewed", the flag will
		 /// eventually fall back to low.
		 /// </summary>
		 private int[] _flags;

		 /// <summary>
		 /// The max number of sweep iteration a flag is kept alive
		 /// if it is not toggled. The more toggles seen in a flag,
		 /// the more likely it is to hit this threshold.
		 /// </summary>
		 private readonly int _keepalive;

		 /// <param name="keepalive"> controls the maximum length of time
		 ///                     a flag will stay toggled if it is not
		 ///                     renewed, expressed as the number of times
		 ///                     <seealso cref="sweep()"/> needs to be called. </param>
		 public DecayingFlags( int keepalive )
		 {
			  this._keepalive = keepalive;
			  this._flags = new int[16];
		 }

		 public virtual void Flag( Key key )
		 {
			  // We dynamically size this up as needed
			  if ( key.IndexConflict >= _flags.Length )
			  {
					Resize( key.IndexConflict );
			  }

			  int flag = _flags[key.IndexConflict];
			  if ( flag < _keepalive )
			  {
					_flags[key.IndexConflict] = flag + 1;
			  }
		 }

		 /// <summary>
		 /// This is how decay happens, the interval at which
		 /// this method is called controls how long unused
		 /// flags are kept 'high'. Each invocation of this will
		 /// decrement the flag counters by 1, marking any that
		 /// reach 0 as low.
		 /// </summary>
		 public virtual void Sweep()
		 {
			  for ( int i = 0; i < _flags.Length; i++ )
			  {
					int count = _flags[i];
					if ( count > 0 )
					{
						 _flags[i] = count - 1;
					}
			  }
		 }

		 private void Resize( int minSize )
		 {
			 lock ( this )
			 {
				  int newSize = _flags.Length;
				  while ( newSize < minSize )
				  {
						newSize += 16;
				  }
      
				  if ( _flags.Length < newSize )
				  {
						_flags = Arrays.copyOf( _flags, newSize );
				  }
			 }
		 }

		 public virtual string AsHex()
		 {
			  // Convert the flags to a byte-array, each
			  // flag represented as a single bit.
			  sbyte[] bits = new sbyte[_flags.Length / 8];

			  // Go over the flags, eight at a time to align
			  // with sticking eight bits at a time into the
			  // output array.
			  for ( int i = 0; i < _flags.Length; i += 8 )
			  {
					bits[i / 8] = ( sbyte )( ( Bit( i ) << 7 ) | ( Bit( i + 1 ) << 6 ) | ( Bit( i + 2 ) << 5 ) | ( Bit( i + 3 ) << 4 ) | ( Bit( i + 4 ) << 3 ) | ( Bit( i + 5 ) << 2 ) | ( Bit( i + 6 ) << 1 ) | ( Bit( i + 7 ) ) );
			  }
			  return HexString.encodeHexString( bits );
		 }

		 private int Bit( int idx )
		 {
			  return _flags[idx] > 0 ? 1 : 0;
		 }
	}

}