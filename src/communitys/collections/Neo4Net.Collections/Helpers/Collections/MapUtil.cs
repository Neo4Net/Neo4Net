using System;
using System.Collections.Generic;
using System.IO;

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
namespace Neo4Net.Helpers.Collections
{

	/// <summary>
	/// Utility to create <seealso cref="System.Collections.IDictionary"/>s.
	/// </summary>
	public abstract class MapUtil
	{
		 /// <summary>
		 /// A short-hand method for creating a <seealso cref="System.Collections.IDictionary"/> of key/value pairs.
		 /// </summary>
		 /// <param name="objects"> alternating key and value. </param>
		 /// @param <K> type of keys </param>
		 /// @param <V> type of values </param>
		 /// <returns> a Map with the entries supplied by {@code objects}. </returns>
		 public static IDictionary<K, V> GenericMap<K, V>( params object[] objects )
		 {
			  return GenericMap( new Dictionary<K, V>(), objects );
		 }

		 /// <summary>
		 /// A short-hand method for adding key/value pairs into a <seealso cref="System.Collections.IDictionary"/>.
		 /// </summary>
		 /// <param name="targetMap"> the <seealso cref="System.Collections.IDictionary"/> to put the objects into. </param>
		 /// <param name="objects"> alternating key and value. </param>
		 /// @param <K> type of keys </param>
		 /// @param <V> type of values </param>
		 /// <returns> a Map with the entries supplied by {@code objects}. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <K, V> java.util.Map<K,V> genericMap(java.util.Map<K,V> targetMap, Object... objects)
		 public static IDictionary<K, V> GenericMap<K, V>( IDictionary<K, V> targetMap, params object[] objects )
		 {
			  int i = 0;
			  while ( i < objects.Length )
			  {
					targetMap[( K ) objects[i++]] = ( V ) objects[i++];
			  }
			  return targetMap;
		 }

		 /// <summary>
		 /// A short-hand method for creating a <seealso cref="System.Collections.IDictionary"/> of key/value pairs where
		 /// both keys and values are <seealso cref="string"/>s.
		 /// </summary>
		 /// <param name="strings"> alternating key and value. </param>
		 /// <returns> a Map with the entries supplied by {@code strings}. </returns>
		 public static IDictionary<string, string> StringMap( params string[] strings )
		 {
			  return GenericMap( ( object[] ) strings );
		 }

		 /// <summary>
		 /// A short-hand method for creating a <seealso cref="System.Collections.IDictionary"/> of key/value pairs where
		 /// both keys and values are <seealso cref="string"/>s.
		 /// </summary>
		 /// <param name="targetMap"> the <seealso cref="System.Collections.IDictionary"/> to put the objects into. </param>
		 /// <param name="strings"> alternating key and value. </param>
		 /// <returns> a Map with the entries supplied by {@code strings}. </returns>
		 public static IDictionary<string, string> StringMap( IDictionary<string, string> targetMap, params string[] strings )
		 {
			  return GenericMap( targetMap, ( object[] ) strings );
		 }

		 /// <summary>
		 /// A short-hand method for creating a <seealso cref="System.Collections.IDictionary"/> of key/value pairs where
		 /// keys are <seealso cref="string"/>s and values are <seealso cref="object"/>s.
		 /// </summary>
		 /// <param name="objects"> alternating key and value. </param>
		 /// <returns> a Map with the entries supplied by {@code objects}. </returns>
		 public static IDictionary<string, object> Map( params object[] objects )
		 {
			  return GenericMap( objects );
		 }

		 /// <summary>
		 /// A short-hand method for creating a <seealso cref="System.Collections.IDictionary"/> of key/value pairs where
		 /// keys are <seealso cref="string"/>s and values are <seealso cref="object"/>s.
		 /// </summary>
		 /// <param name="targetMap"> the <seealso cref="System.Collections.IDictionary"/> to put the objects into. </param>
		 /// <param name="objects"> alternating key and value. </param>
		 /// <returns> a Map with the entries supplied by {@code objects}. </returns>
		 public static IDictionary<string, object> Map( IDictionary<string, object> targetMap, params object[] objects )
		 {
			  return GenericMap( targetMap, objects );
		 }

		 /// <summary>
		 /// Loads a <seealso cref="System.Collections.IDictionary"/> from a <seealso cref="Reader"/> assuming strings as keys
		 /// and values.
		 /// </summary>
		 /// <param name="reader"> the <seealso cref="Reader"/> containing a <seealso cref="Properties"/>-like
		 /// layout of keys and values. </param>
		 /// <returns> the read data as a <seealso cref="System.Collections.IDictionary"/>. </returns>
		 /// <exception cref="IOException"> if the {@code reader} throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String, String> load(java.io.Reader reader) throws java.io.IOException
		 public static IDictionary<string, string> Load( Reader reader )
		 {
			  Properties props = new Properties();
			  props.load( reader );
			  //noinspection unchecked
			  return new Dictionary<string, string>( ( System.Collections.IDictionary ) props );
		 }

		 /// <summary>
		 /// Loads a <seealso cref="System.Collections.IDictionary"/> from a <seealso cref="Reader"/> assuming strings as keys
		 /// and values. Any <seealso cref="IOException"/> is wrapped and thrown as a
		 /// <seealso cref="System.Exception"/> instead.
		 /// </summary>
		 /// <param name="reader"> the <seealso cref="Reader"/> containing a <seealso cref="Properties"/>-like
		 /// layout of keys and values. </param>
		 /// <returns> the read data as a <seealso cref="System.Collections.IDictionary"/>. </returns>
		 public static IDictionary<string, string> LoadStrictly( Reader reader )
		 {
			  try
			  {
					return Load( reader );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Loads a <seealso cref="System.Collections.IDictionary"/> from an <seealso cref="System.IO.Stream_Input"/> assuming strings as keys
		 /// and values.
		 /// </summary>
		 /// <param name="stream"> the <seealso cref="System.IO.Stream_Input"/> containing a
		 /// <seealso cref="Properties"/>-like layout of keys and values. </param>
		 /// <returns> the read data as a <seealso cref="System.Collections.IDictionary"/>. </returns>
		 /// <exception cref="IOException"> if the {@code stream} throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String,String> load(java.io.InputStream stream) throws java.io.IOException
		 public static IDictionary<string, string> Load( Stream stream )
		 {
			  Properties props = new Properties();
			  props.load( stream );

			  Dictionary<string, string> result = new Dictionary<string, string>();
			  foreach ( KeyValuePair<object, object> entry in props.entrySet() )
			  {
					// Properties does not trim whitespace from the right side of values
					result[( string ) entry.Key] = ( ( string ) entry.Value ).Trim();
			  }

			  return result;
		 }

		 /// <summary>
		 /// Loads a <seealso cref="System.Collections.IDictionary"/> from an <seealso cref="System.IO.Stream_Input"/> assuming strings as keys
		 /// and values. Any <seealso cref="IOException"/> is wrapped and thrown as a
		 /// <seealso cref="System.Exception"/> instead.
		 /// </summary>
		 /// <param name="stream"> the <seealso cref="System.IO.Stream_Input"/> containing a
		 /// <seealso cref="Properties"/>-like layout of keys and values. </param>
		 /// <returns> the read data as a <seealso cref="System.Collections.IDictionary"/>. </returns>
		 public static IDictionary<string, string> LoadStrictly( Stream stream )
		 {
			  try
			  {
					return Load( stream );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Loads a <seealso cref="System.Collections.IDictionary"/> from a <seealso cref="File"/> assuming strings as keys
		 /// and values.
		 /// </summary>
		 /// <param name="file"> the <seealso cref="File"/> containing a <seealso cref="Properties"/>-like
		 /// layout of keys and values. </param>
		 /// <returns> the read data as a <seealso cref="System.Collections.IDictionary"/>. </returns>
		 /// <exception cref="IOException"> if the file reader throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String, String> load(java.io.File file) throws java.io.IOException
		 public static IDictionary<string, string> Load( File file )
		 {
			  FileStream stream = null;
			  try
			  {
					stream = new FileStream( file, FileMode.Open, FileAccess.Read );
					return Load( stream );
			  }
			  finally
			  {
					CloseIfNotNull( stream );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void closeIfNotNull(java.io.Closeable closeable) throws java.io.IOException
		 private static void CloseIfNotNull( System.IDisposable closeable )
		 {
			  if ( closeable != null )
			  {
					closeable.Dispose();
			  }
		 }

		 /// <summary>
		 /// Stores the data in {@code config} into {@code file} in a standard java
		 /// <seealso cref="Properties"/> format. </summary>
		 /// <param name="config"> the data to store in the properties file. </param>
		 /// <param name="file"> the file to store the properties in. </param>
		 /// <exception cref="IOException"> IO error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void store(java.util.Map<String, String> config, java.io.File file) throws java.io.IOException
		 public static void Store( IDictionary<string, string> config, File file )
		 {
			  Stream stream = null;
			  try
			  {
					stream = new BufferedOutputStream( new FileStream( file, FileMode.Create, FileAccess.Write ) );
					Store( config, stream );
			  }
			  finally
			  {
					CloseIfNotNull( stream );
			  }
		 }

		 /// <summary>
		 /// Stores the data in {@code config} into {@code file} in a standard java
		 /// <seealso cref="Properties"/> format. Any <seealso cref="IOException"/> is wrapped and thrown as a
		 /// <seealso cref="System.Exception"/> instead. </summary>
		 /// <param name="config"> the data to store in the properties file. </param>
		 /// <param name="file"> the file to store the properties in. </param>
		 public static void StoreStrictly( IDictionary<string, string> config, File file )
		 {
			  try
			  {
					Store( config, file );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Stores the data in {@code config} into {@code stream} in a standard java
		 /// <seealso cref="Properties"/> format. </summary>
		 /// <param name="config"> the data to store in the properties file. </param>
		 /// <param name="stream"> the <seealso cref="System.IO.Stream_Output"/> to store the properties in. </param>
		 /// <exception cref="IOException"> IO error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void store(java.util.Map<String, String> config, java.io.OutputStream stream) throws java.io.IOException
		 public static void Store( IDictionary<string, string> config, Stream stream )
		 {
			  Properties properties = new Properties();
			  foreach ( KeyValuePair<string, string> property in config.SetOfKeyValuePairs() )
			  {
					properties.setProperty( property.Key, property.Value );
			  }
			  properties.store( stream, null );
		 }

		 /// <summary>
		 /// Stores the data in {@code config} into {@code stream} in a standard java
		 /// <seealso cref="Properties"/> format. Any <seealso cref="IOException"/> is wrapped and thrown as a
		 /// <seealso cref="System.Exception"/> instead. </summary>
		 /// <param name="config"> the data to store in the properties file. </param>
		 /// <param name="stream"> the <seealso cref="System.IO.Stream_Output"/> to store the properties in. </param>
		 public static void StoreStrictly( IDictionary<string, string> config, Stream stream )
		 {
			  try
			  {
					Store( config, stream );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Stores the data in {@code config} into {@code writer} in a standard java
		 /// <seealso cref="Properties"/> format.
		 /// </summary>
		 /// <param name="config"> the data to store in the properties file. </param>
		 /// <param name="writer"> the <seealso cref="Writer"/> to store the properties in. </param>
		 /// <exception cref="IOException"> IO error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void store(java.util.Map<String, String> config, java.io.Writer writer) throws java.io.IOException
		 public static void Store( IDictionary<string, string> config, Writer writer )
		 {
			  Properties properties = new Properties();
			  properties.putAll( config );
			  properties.store( writer, null );
		 }

		 /// <summary>
		 /// Stores the data in {@code config} into {@code writer} in a standard java
		 /// <seealso cref="Properties"/> format. Any <seealso cref="IOException"/> is wrapped and thrown
		 /// as a <seealso cref="System.Exception"/> instead.
		 /// </summary>
		 /// <param name="config"> the data to store in the properties file. </param>
		 /// <param name="writer"> the <seealso cref="Writer"/> to store the properties in. </param>
		 public static void StoreStrictly( IDictionary<string, string> config, Writer writer )
		 {
			  try
			  {
					Store( config, writer );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Reversed a map, making the key value and the value key. </summary>
		 /// @param <K> the type of key in the map to reverse. These will be the
		 /// values in the returned map. </param>
		 /// @param <V> the type of values in the map to revert. These will be the
		 /// keys in the returned map. </param>
		 /// <param name="map"> the <seealso cref="System.Collections.IDictionary"/> to reverse. </param>
		 /// <returns> the reverse of {@code map}. A new <seealso cref="System.Collections.IDictionary"/> will be returned
		 /// where the keys from {@code map} will be the values and the values will
		 /// be the keys. </returns>
		 public static IDictionary<V, K> Reverse<K, V>( IDictionary<K, V> map )
		 {
			  IDictionary<V, K> reversedMap = new Dictionary<V, K>();
			  foreach ( KeyValuePair<K, V> entry in map.SetOfKeyValuePairs() )
			  {
					reversedMap[entry.Value] = entry.Key;
			  }
			  return reversedMap;
		 }

		 public static IDictionary<K, V> CopyAndPut<K, V>( IDictionary<K, V> map, K key, V value )
		 {
			  IDictionary<K, V> copy = new Dictionary<K, V>( map );
			  copy[key] = value;
			  return copy;
		 }

		 public static IDictionary<K, V> CopyAndRemove<K, V>( IDictionary<K, V> map, K key )
		 {
			  IDictionary<K, V> copy = new Dictionary<K, V>( map );
			  copy.Remove( key );
			  return copy;
		 }

		 public static IDictionary<K, V> ToMap<K, V>( IEnumerable<Pair<K, V>> pairs )
		 {
			  return ToMap( pairs.GetEnumerator() );
		 }

		 public static IDictionary<K, V> ToMap<K, V>( IEnumerator<Pair<K, V>> pairs )
		 {
			  IDictionary<K, V> result = new Dictionary<K, V>();
			  while ( pairs.MoveNext() )
			  {
					Pair<K, V> pair = pairs.Current;
					result[pair.First()] = pair.Other();
			  }
			  return result;
		 }

		 public static MapBuilder<K, V> Entry<K, V>( K key, V value )
		 {
			  return ( new MapBuilder<K, V>() ).Entry(key, value);
		 }

		 public class MapBuilder<K, V>
		 {
			  internal readonly IDictionary<K, V> Map = new Dictionary<K, V>();

			  public virtual MapBuilder<K, V> Entry( K key, V value )
			  {
					Map[key] = value;
					return this;
			  }

			  public virtual IDictionary<K, V> Create()
			  {
					return Map;
			  }
		 }

		 /// <summary>
		 /// Mutates the input map by removing entries which do not have keys in the new backing data, as extracted with
		 /// the keyExtractor. </summary>
		 /// <param name="map"> the map to mutate. </param>
		 /// <param name="newBackingData"> the backing data to retain. </param>
		 /// <param name="keyExtractor"> the function to extract keys from the backing data. </param>
		 /// @param <K> type of the key in the input map. </param>
		 /// @param <V> type of the values in the input map. </param>
		 /// @param <T> type of the keys in the new baking data. </param>
		 public static void TrimToList<K, V, T>( IDictionary<K, V> map, IList<T> newBackingData, System.Func<T, K> keyExtractor )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<K> retainedKeys = newBackingData.Select( keyExtractor ).collect( Collectors.toSet() );
			  TrimToList( map, retainedKeys );
		 }

		 /// <summary>
		 /// Mutates the input map by removing entries which do not have keys in the new backing data, as extracted with
		 /// the keyExtractor. </summary>
		 /// <param name="map"> the map to mutate. </param>
		 /// <param name="newBackingData"> the backing data to retain. </param>
		 /// <param name="keyExtractor"> the function to extract keys from the backing data. </param>
		 /// @param <K> type of the key in the input map. </param>
		 /// @param <V> type of the values in the input map. </param>
		 /// @param <T> type of the keys in the new backing data. </param>
		 public static void TrimToFlattenedList<K, V, T>( IDictionary<K, V> map, IList<T> newBackingData, System.Func<T, Stream<K>> keyExtractor )
		 {
			  ISet<K> retainedKeys = newBackingData.stream().flatMap(keyExtractor).collect(Collectors.toSet());
			  TrimToList( map, retainedKeys );
		 }

		 /// <summary>
		 /// Mutates the input map by removing entries which are not in the retained set of keys. </summary>
		 /// <param name="map"> the map to mutate. </param>
		 /// <param name="retainedKeys"> the keys to retain. </param>
		 /// @param <K> type of the key. </param>
		 /// @param <V> type of the values. </param>
		 public static void TrimToList<K, V>( IDictionary<K, V> map, ISet<K> retainedKeys )
		 {
			  ISet<K> keysToRemove = new HashSet<K>( map.Keys );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			  keysToRemove.removeAll( retainedKeys );
			  keysToRemove.forEach( map.remove );
		 }
	}

}