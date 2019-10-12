using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.util
{

	using Org.Neo4j.Function;

	/// <summary>
	/// <seealso cref="System.Collections.Hashtable"/> that automatically creates missing values on <seealso cref="get(object)"/>, using a supplied <seealso cref="Factory"/>.
	/// Merely for convenience. For example code like:
	/// 
	/// <pre>
	/// Map<String,Map<String,Map<String,AtomicLong>>> data = new HashMap<>();
	/// ...
	/// for ( String a : ... )
	/// {
	///    Map<String,Map<String,AtomicLong>> levelOne = data.get( a );
	///    if ( levelOne == null )
	///    {
	///        levelOne = new HashMap<>();
	///        data.put( a, levelOne );
	///    }
	/// 
	///    for ( String b : ... )
	///    {
	///        Map<String,AtomicLong> levelTwo = levelOne.get( b );
	///        if ( levelTwo == null )
	///        {
	///            levelTwo = new HashMap<>();
	///            levelOne.put( b, levelTwo );
	///        }
	/// 
	///        for ( String c : ... )
	///        {
	///            AtomicLong count = levelTwo.get( c );
	///            if ( count == null )
	///            {
	///                count = new AtomicLong();
	///                levelTwo.put( c, count );
	///            }
	///            count.incrementAndGet();
	///        }
	///    }
	/// }
	/// </pre>
	/// 
	/// Can be replaced with:
	/// 
	/// <pre>
	/// Map<String,Map<String,Map<String,AtomicLong>>> data = new AutoCreatingHashMap<>(
	///     nested( String.class, nested( String.class, values( AtomicLong.class ) ) ) );
	/// ...
	/// for ( String a : ... )
	/// {
	///     Map<String,Map<String,AtomicLong>> levelOne = data.get( a );
	///     for ( String b : ... )
	///     {
	///         Map<String,AtomicLong> levelTwo = levelOne.get( b );
	///         for ( String c : ... )
	///         {
	///             levelTwo.get( c ).incrementAndGet();
	///         }
	///     }
	/// }
	/// </pre>
	/// 
	/// An enormous improvement in readability. The only reflection used is in the <seealso cref="values()"/> <seealso cref="Factory"/>,
	/// however that's just a convenience as well. Any <seealso cref="Factory"/> can be supplied instead.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public class AutoCreatingHashMap<K, V> : Dictionary<K, V>
	{
		 private readonly Factory<V> _valueCreator;

		 public AutoCreatingHashMap( Factory<V> valueCreator ) : base()
		 {
			  this._valueCreator = valueCreator;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public V get(Object key)
		 public override V Get( object key )
		 {
			  if ( !base.ContainsKey( key ) )
			  {
					// Since this is just a test class, we can force all users of it to call get with K instances.
					this.put( ( K ) key, _valueCreator.newInstance() );
			  }
			  return base[key];
		 }

		 /// <returns> a <seealso cref="Factory"/> that via reflection instantiates objects of the supplied {@code valueType},
		 /// assuming zero-argument constructor. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <V> org.neo4j.function.Factory<V> values(final Class<V> valueType)
		 public static Factory<V> Values<V>( Type valueType )
		 {
				 valueType = typeof( V );
			  return () =>
			  {
				try
				{
					 return System.Activator.CreateInstance( valueType );
				}
				catch ( Exception e ) when ( e is InstantiationException || e is IllegalAccessException )
				{
					 throw new Exception( e );
				}
			  };
		 }

		 /// <returns> a <seealso cref="Factory"/> that creates <seealso cref="AutoCreatingHashMap"/> instances as values, and where the
		 /// created maps have the supplied {@code nested} <seealso cref="Factory"/> as value factory. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <K,V> org.neo4j.function.Factory<java.util.Map<K,V>> nested(Class<K> keyClass, final org.neo4j.function.Factory<V> nested)
		 public static Factory<IDictionary<K, V>> Nested<K, V>( Type keyClass, Factory<V> nested )
		 {
				 keyClass = typeof( K );
			  return () => new AutoCreatingHashMap<IDictionary<K, V>>(nested);
		 }

		 public static Factory<V> DontCreate<V>()
		 {
			  return () => null;
		 }

		 public static Factory<ISet<V>> ValuesOfTypeHashSet<V>()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return HashSet<object>::new;
		 }
	}

}