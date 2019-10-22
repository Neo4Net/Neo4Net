using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Values.@virtual
{

	using Neo4Net.Functions;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Values;
	using Neo4Net.Values;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	public abstract class MapValue : VirtualValue
	{
		 public static MapValue EMPTY = new MapValueAnonymousInnerClass();

		 private class MapValueAnonymousInnerClass : MapValue
		 {
			 public override IEnumerable<string> keySet()
			 {
				  return Collections.emptyList();
			 }

			 public override void @foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception
			 {
				  //do nothing
			 }

			 public override bool containsKey( string key )
			 {
				  return false;
			 }

			 public override AnyValue get( string key )
			 {
				  return NO_VALUE;
			 }

			 public override int size()
			 {
				  return 0;
			 }
		 }

		 internal sealed class MapWrappingMapValue : MapValue
		 {
			  internal readonly IDictionary<string, AnyValue> Map;

			  internal MapWrappingMapValue( IDictionary<string, AnyValue> map )
			  {
					this.Map = map;
			  }

			  public override IEnumerable<string> KeySet()
			  {
					return Map.Keys;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void foreach(org.Neo4Net.function.ThrowingBiConsumer<String,org.Neo4Net.values.AnyValue,E> f) throws E
			  public override void Foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception
			  {
					foreach ( KeyValuePair<string, AnyValue> entry in Map.SetOfKeyValuePairs() )
					{
						 f.Accept( entry.Key, entry.Value );
					}
			  }

			  public override bool ContainsKey( string key )
			  {
					return Map.ContainsKey( key );
			  }

			  public override AnyValue Get( string key )
			  {
					return Map.getOrDefault( key, NO_VALUE );
			  }

			  public override int Size()
			  {
					return Map.Count;
			  }
		 }

		 private sealed class FilteringMapValue : MapValue
		 {
			  internal readonly MapValue Map;
			  internal readonly System.Func<string, AnyValue, bool> Filter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int SizeConflict = -1;

			  internal FilteringMapValue( MapValue map, System.Func<string, AnyValue, bool> filter )
			  {
					this.Map = map;
					this.Filter = filter;
			  }

			  public override IEnumerable<string> KeySet()
			  {
					IList<string> keys = SizeConflict >= 0 ? new List<string>( SizeConflict ) : new List<string>();
					Foreach((key, value) =>
					{
					if ( Filter.apply( key, value ) )
					{
						keys.Add( key );
					}
					});

					return keys;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void foreach(org.Neo4Net.function.ThrowingBiConsumer<String,org.Neo4Net.values.AnyValue,E> f) throws E
			  public override void Foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception
			  {
					Map.@foreach((s, anyValue) =>
					{
					if ( Filter.apply( s, anyValue ) )
					{
						f.Accept( s, anyValue );
					}
					});
			  }

			  public override bool ContainsKey( string key )
			  {
					AnyValue value = Map.get( key );
					if ( value == NO_VALUE )
					{
						 return false;
					}
					else
					{
						 return Filter.apply( key, value );
					}
			  }

			  public override AnyValue Get( string key )
			  {
					AnyValue value = Map.get( key );
					if ( value == NO_VALUE )
					{
						 return NO_VALUE;
					}
					else if ( Filter.apply( key, value ) )
					{
						 return value;
					}
					else
					{
						 return NO_VALUE;
					}
			  }

			  public override int Size()
			  {
					if ( SizeConflict < 0 )
					{
						 SizeConflict = 0;
						 Foreach((k, v) =>
						 {
						 if ( Filter.apply( k, v ) )
						 {
							 SizeConflict++;
						 }
						 });
					}
					return SizeConflict;
			  }
		 }

		 private sealed class MappedMapValue : MapValue
		 {
			  internal readonly MapValue Map;
			  internal readonly System.Func<string, AnyValue, AnyValue> MapFunction;

			  internal MappedMapValue( MapValue map, System.Func<string, AnyValue, AnyValue> mapFunction )
			  {
					this.Map = map;
					this.MapFunction = mapFunction;
			  }

			  public override ListValue Keys()
			  {
					return Map.keys();
			  }

			  public override IEnumerable<string> KeySet()
			  {
					return Map.Keys;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void foreach(org.Neo4Net.function.ThrowingBiConsumer<String,org.Neo4Net.values.AnyValue,E> f) throws E
			  public override void Foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception
			  {
					Map.@foreach( ( s, anyValue ) => f.accept( s, MapFunction.apply( s, anyValue ) ) );
			  }

			  public override bool ContainsKey( string key )
			  {
					return Map.containsKey( key );
			  }

			  public override AnyValue Get( string key )
			  {
					return MapFunction.apply( key, Map.get( key ) );
			  }

			  public override int Size()
			  {
					return Map.size();
			  }
		 }

		 private sealed class UpdatedMapValue : MapValue
		 {
			  internal readonly MapValue Map;
			  internal readonly string[] UpdatedKeys;
			  internal readonly AnyValue[] UpdatedValues;

			  internal UpdatedMapValue( MapValue map, string[] updatedKeys, AnyValue[] updatedValues )
			  {
					Debug.Assert( updatedKeys.Length == updatedValues.Length );
					Debug.Assert( !Overlaps( map, updatedKeys ) );
					this.Map = map;
					this.UpdatedKeys = updatedKeys;
					this.UpdatedValues = updatedValues;
			  }

			  internal static bool Overlaps( MapValue map, string[] updatedKeys )
			  {
					foreach ( string key in updatedKeys )
					{
						 if ( map.ContainsKey( key ) )
						 {
							  return true;
						 }
					}

					return false;
			  }

			  public override ListValue Keys()
			  {
					return VirtualValues.Concat( Map.keys(), VirtualValues.FromArray(Values.stringArray(UpdatedKeys)) );
			  }

			  public override IEnumerable<string> KeySet()
			  {
					return () => new IteratorAnonymousInnerClass(this);
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<string>
			  {
				  private readonly UpdatedMapValue _outerInstance;

				  public IteratorAnonymousInnerClass( UpdatedMapValue outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  @internal = outerInstance.Map.Keys.GetEnumerator();
				  }

				  private IEnumerator<string> @internal;
				  private int index;

				  public bool hasNext()
				  {
						if ( @internal.hasNext() )
						{
							 return true;
						}
						else
						{
							 return index < _outerInstance.updatedKeys.Length;
						}
				  }

				  public string next()
				  {
						if ( @internal.hasNext() )
						{
							 return @internal.next();
						}
						else if ( index < _outerInstance.updatedKeys.Length )
						{
							 return _outerInstance.updatedKeys[index++];
						}
						else
						{
							 throw new NoSuchElementException();
						}
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void foreach(org.Neo4Net.function.ThrowingBiConsumer<String,org.Neo4Net.values.AnyValue,E> f) throws E
			  public override void Foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception
			  {
					Map.@foreach( f );
					for ( int i = 0; i < UpdatedKeys.Length; i++ )
					{
						 f.Accept( UpdatedKeys[i], UpdatedValues[i] );
					}
			  }

			  public override bool ContainsKey( string key )
			  {
					foreach ( string updatedKey in UpdatedKeys )
					{
						 if ( updatedKey.Equals( key ) )
						 {
							  return true;
						 }
					}

					return Map.containsKey( key );
			  }

			  public override AnyValue Get( string key )
			  {
					for ( int i = 0; i < UpdatedKeys.Length; i++ )
					{
						 if ( UpdatedKeys[i].Equals( key ) )
						 {
							  return UpdatedValues[i];
						 }
					}
					return Map.get( key );
			  }

			  public override int Size()
			  {
					return Map.size() + UpdatedKeys.Length;
			  }
		 }

		 private sealed class CombinedMapValue : MapValue
		 {
			  internal readonly MapValue[] Maps;

			  internal CombinedMapValue( params MapValue[] mapValues )
			  {
					this.Maps = mapValues;
			  }

			  public override IEnumerable<string> KeySet()
			  {
				  return () => new PrefetchingIteratorAnonymousInnerClass(this);
			  }

			  private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<string>
			  {
				  private readonly CombinedMapValue _outerInstance;

				  public PrefetchingIteratorAnonymousInnerClass( CombinedMapValue outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  seen = new HashSet<>();
				  }

				  private int mapIndex;
				  private IEnumerator<string> @internal;
				  private HashSet<string> seen;

				  protected internal override string fetchNextOrNull()
				  {
						while ( mapIndex < _outerInstance.maps.Length || @internal != null && @internal.hasNext() )
						{
							 if ( @internal == null || !@internal.hasNext() )
							 {
								  @internal = _outerInstance.maps[mapIndex++].Keys.GetEnumerator();
							 }

							 while ( @internal.hasNext() )
							 {
								  string key = @internal.next();
								  if ( seen.add( key ) )
								  {
										return key;
								  }
							 }
						}
						return null;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void foreach(org.Neo4Net.function.ThrowingBiConsumer<String,org.Neo4Net.values.AnyValue,E> f) throws E
			  public override void Foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception
			  {
					HashSet<string> seen = new HashSet<string>();
					ThrowingBiConsumer<string, AnyValue, E> consume = ( key, value ) =>
					{
					 if ( seen.Add( key ) )
					 {
						  f.Accept( key, value );
					 }
					};
					for ( int i = Maps.Length - 1; i >= 0; i-- )
					{
						 Maps[i].@foreach( consume );
					}
			  }

			  public override bool ContainsKey( string key )
			  {
					foreach ( MapValue map in Maps )
					{
						 if ( map.ContainsKey( key ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  public override AnyValue Get( string key )
			  {
					for ( int i = Maps.Length - 1; i >= 0; i-- )
					{
						 AnyValue value = Maps[i].get( key );
						 if ( value != NO_VALUE )
						 {
							  return value;
						 }
					}
					return NO_VALUE;
			  }

			  public override int Size()
			  {
					int[] size = new int[] { 0 };
					HashSet<string> seen = new HashSet<string>();
					ThrowingBiConsumer<string, AnyValue, Exception> consume = ( key, value ) =>
					{
					 if ( seen.Add( key ) )
					 {
						  size[0]++;
					 }
					};
					for ( int i = Maps.Length - 1; i >= 0; i-- )
					{
						 Maps[i].@foreach( consume );
					}
					return size[0];
			  }
		 }

		 public override int ComputeHash()
		 {
			  int[] h = new int[1];
			  Foreach( ( key, value ) => h[0] += key.GetHashCode() ^ value.GetHashCode() );
			  return h[0];
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(org.Neo4Net.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  writer.BeginMap( Size() );
			  Foreach((s, anyValue) =>
			  {
			  writer.writeString( s );
			  anyValue.writeTo( writer );
			  });
			  writer.EndMap();
		 }

		 public override bool Equals( VirtualValue other )
		 {
			  if ( !( other is MapValue ) )
			  {
					return false;
			  }
			  MapValue that = ( MapValue ) other;
			  int size = size();
			  if ( size != that.Size() )
			  {
					return false;
			  }

			  IEnumerable<string> keys = KeySet();
			  foreach ( string key in keys )
			  {
					if ( !Get( key ).Equals( that.Get( key ) ) )
					{
						 return false;
					}
			  }

			  return true;
		 }

		 public abstract IEnumerable<string> KeySet();

		 public virtual ListValue Keys()
		 {
			  string[] keys = new string[Size()];
			  int i = 0;
			  foreach ( string key in KeySet() )
			  {
					keys[i++] = key;
			  }
			  return VirtualValues.FromArray( Values.stringArray( keys ) );
		 }

		 public override VirtualValueGroup ValueGroup()
		 {
			  return VirtualValueGroup.Map;
		 }

		 public override int CompareTo( VirtualValue other, IComparer<AnyValue> comparator )
		 {
			  if ( !( other is MapValue ) )
			  {
					throw new System.ArgumentException( "Cannot compare different virtual values" );
			  }
			  MapValue otherMap = ( MapValue ) other;
			  int size = size();
			  int compare = Integer.compare( size, otherMap.Size() );
			  if ( compare == 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					string[] thisKeys = StreamSupport.stream( KeySet().spliterator(), false ).toArray(string[]::new);
					Arrays.sort( thisKeys, string.compareTo );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					string[] thatKeys = StreamSupport.stream( otherMap.Keys.spliterator(), false ).toArray(string[]::new);
					Arrays.sort( thatKeys, string.compareTo );
					for ( int i = 0; i < size; i++ )
					{
						 compare = thisKeys[i].CompareTo( thatKeys[i] );
						 if ( compare != 0 )
						 {
							  return compare;
						 }
					}

					for ( int i = 0; i < size; i++ )
					{
						 string key = thisKeys[i];
						 compare = comparator.Compare( Get( key ), otherMap.Get( key ) );
						 if ( compare != 0 )
						 {
							  return compare;
						 }
					}
			  }
			  return compare;
		 }

		 public override bool? TernaryEquals( AnyValue other )
		 {
			  if ( other == null || other == NO_VALUE )
			  {
					return null;
			  }
			  else if ( !( other is MapValue ) )
			  {
					return false;
			  }
			  MapValue otherMap = ( MapValue ) other;
			  int size = size();
			  if ( size != otherMap.Size() )
			  {
					return false;
			  }
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] thisKeys = StreamSupport.stream( KeySet().spliterator(), false ).toArray(string[]::new);
			  Arrays.sort( thisKeys, string.compareTo );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] thatKeys = StreamSupport.stream( otherMap.Keys.spliterator(), false ).toArray(string[]::new);
			  Arrays.sort( thatKeys, string.compareTo );
			  for ( int i = 0; i < size; i++ )
			  {
					if ( thisKeys[i].CompareTo( thatKeys[i] ) != 0 )
					{
						 return false;
					}
			  }
			  bool? equalityResult = true;

			  for ( int i = 0; i < size; i++ )
			  {
					string key = thisKeys[i];
					bool? s = Get( key ).ternaryEquals( otherMap.Get( key ) );
					if ( s == null )
					{
						 equalityResult = null;
					}
					else if ( !s.Value )
					{
						 return false;
					}
			  }
			  return equalityResult;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapMap( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract <E extends Exception> void foreach(org.Neo4Net.function.ThrowingBiConsumer<String,org.Neo4Net.values.AnyValue,E> f) throws E;
		 public abstract void @foreach<E>( ThrowingBiConsumer<string, AnyValue, E> f ) where E : Exception;

		 public abstract bool ContainsKey( string key );

		 public abstract AnyValue Get( string key );

		 public virtual MapValue Filter( System.Func<string, AnyValue, bool> filterFunction )
		 {
			  return new FilteringMapValue( this, filterFunction );
		 }

		 public virtual MapValue UpdatedWith( string key, AnyValue value )
		 {
			  AnyValue current = Get( key );
			  if ( current.Equals( value ) )
			  {
					return this;
			  }
			  else if ( current == NO_VALUE )
			  {
					return new UpdatedMapValue( this, new string[]{ key }, new AnyValue[]{ value } );
			  }
			  else
			  {
					return new MappedMapValue(this, (k, v) =>
					{
					if ( k.Equals( key ) )
					{
						return value;
					}
					else
					{
						return v;
					}
					});
			  }
		 }

		 public virtual MapValue UpdatedWith( MapValue other )
		 {
			  return new CombinedMapValue( this, other );
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Map";
			 }
		 }

		 public override string ToString()
		 {
			  StringBuilder sb = new StringBuilder( TypeName + "{" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] sep = new String[]{""};
			  string[] sep = new string[]{ "" };
			  Foreach((key, value) =>
			  {
				sb.Append( sep[0] );
				sb.Append( key );
				sb.Append( " -> " );
				sb.Append( value );
				sep[0] = ", ";
			  });
			  sb.Append( '}' );
			  return sb.ToString();
		 }

		 public abstract int Size();
	}

}