using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Helpers
{

	using Org.Neo4j.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.flag;

	/// <summary>
	/// A utility for locating services. This implements the same functionality as <a
	/// href="https://docs.oracle.com/javase/8/docs/api/java/util/ServiceLoader.html">
	/// the Java ServiceLoader interface</a>.
	/// <para>
	/// Additionally this class can be used as a base class for implementing services
	/// that are differentiated by a String key. An example implementation might be:
	/// <pre>
	/// <code>
	/// public abstract class StringConverter extends org.neo4j.commons.Service
	/// {
	///     protected StringConverter(String id)
	///     {
	///         super( id );
	///     }
	/// 
	///     public abstract String convert( String input );
	/// 
	///     public static StringConverter load( String id )
	///     {
	///         return org.neo4j.commons.Service.load( StringConverter.class, id );
	///     }
	/// }
	/// </code>
	/// </pre>
	/// </para>
	/// <para>
	/// With for example these implementations:
	/// <pre>
	/// <code>
	/// public final class UppercaseConverter extends StringConverter
	/// {
	///     public UppercaseConverter()
	///     {
	///         super( "uppercase" );
	///     }
	/// 
	///     public String convert( String input )
	///     {
	///         return input.toUpperCase();
	///     }
	/// }
	/// 
	/// public final class ReverseConverter extends StringConverter
	/// {
	///     public ReverseConverter()
	///     {
	///         super( "reverse" );
	///     }
	/// 
	///     public String convert( String input )
	///     {
	///         char[] chars = input.toCharArray();
	///         for ( int i = 0; i &lt; chars.length/2; i++ )
	///         {
	///             char intermediate = chars[i];
	///             chars[i] = chars[chars.length-1-i];
	///             chars[chars.length-1-i] = chars[i];
	///         }
	///         return new String( chars );
	///     }
	/// }
	/// </code>
	/// </pre>
	/// </para>
	/// <para>
	/// This would then be used as:
	/// <pre>
	/// <code>
	/// String atad = StringConverter.load( "reverse" ).convert( "data" );
	/// </code>
	/// </pre>
	/// 
	/// @author Tobias Ivarsson
	/// </para>
	/// </summary>
	public abstract class Service
	{
		 /// <summary>
		 /// Enabling this is useful for debugging why services aren't loaded where you would expect them to.
		 /// </summary>
		 private static readonly bool _printServiceLoaderStackTraces = flag( typeof( Service ), "printServiceLoaderStackTraces", false );

		 private readonly ISet<string> _keys;

		 /// <summary>
		 /// Designates that a class implements the specified service and should be
		 /// added to the services listings file (META-INF/services/[service-name]).
		 /// <para>
		 /// The annotation in itself does not provide any functionality for adding
		 /// the implementation class to the services listings file. But it serves as
		 /// a handle for an Annotation Processing Tool to utilize for performing that
		 /// task.
		 /// </para>
		 /// <para>
		 /// This annotation is deprecated and will be removed in a future release.
		 /// 
		 /// @author Tobias Ivarsson
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Target(ElementType.TYPE) @Retention(RetentionPolicy.SOURCE) @Deprecated public class Implementation extends System.Attribute
		 [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false), Obsolete]
		 public class Implementation : System.Attribute
		 {
			 private readonly Service _outerInstance;

			 public Implementation;
			 {
			 }

			  /// <summary>
			  /// The service(s) this class implements.
			  /// </summary>
			  /// <returns> the services this class implements. </returns>
			  internal Type[] value;

			 public Implementation( public Implementation, Class[] value )
			 {
				 this.Implementation = Implementation;
				 this.value = value;
			 }
		 }

		 /// <summary>
		 /// Load all implementations of a Service.
		 /// </summary>
		 /// @param <T> the type of the Service </param>
		 /// <param name="type"> the type of the Service to load </param>
		 /// <returns> all registered implementations of the Service </returns>
		 public static IEnumerable<T> Load<T>( Type type )
		 {
				 type = typeof( T );
			  IEnumerable<T> loader;
			  if ( null != ( loader = Java6Loader( type ) ) )
			  {
					return loader;
			  }
			  return Collections.emptyList();
		 }

		 /// <summary>
		 /// Load the Service implementation with the specified key. This method will return null if requested service not
		 /// found.
		 /// </summary>
		 /// <param name="type"> the type of the Service to load </param>
		 /// <param name="key"> the key that identifies the desired implementation </param>
		 /// @param <T> the type of the Service to load </param>
		 /// <returns> requested service </returns>
		 public static T LoadSilently<T>( Type type, string key ) where T : Service
		 {
				 type = typeof( T );
			  foreach ( T service in Load( type ) )
			  {
					if ( service.Matches( key ) )
					{
						 return service;
					}
			  }
			  return default( T );
		 }

		 /// <summary>
		 /// Load the Service implementation with the specified key. This method should never return null.
		 /// </summary>
		 /// @param <T> the type of the Service </param>
		 /// <param name="type"> the type of the Service to load </param>
		 /// <param name="key"> the key that identifies the desired implementation </param>
		 /// <returns> the matching Service implementation </returns>
		 /// <exception cref="NoSuchElementException"> if no service could be loaded with the given key. </exception>
		 public static T Load<T>( Type type, string key ) where T : Service
		 {
				 type = typeof( T );
			  T service = LoadSilently( type, key );
			  if ( service == null )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new NoSuchElementException( string.Format( "Could not find any implementation of {0} with a key=\"{1}\"", type.FullName, key ) );
			  }
			  return service;
		 }

		 /// <summary>
		 /// Create a new instance of a service implementation identified with the
		 /// specified key(s).
		 /// </summary>
		 /// <param name="key"> the main key for identifying this service implementation </param>
		 /// <param name="altKeys"> alternative spellings of the identifier of this service
		 /// implementation </param>
		 protected internal Service( string key, params string[] altKeys )
		 {
			  if ( altKeys == null || altKeys.Length == 0 )
			  {
					this._keys = Collections.singleton( key );
			  }
			  else
			  {
					this._keys = new HashSet<string>( Arrays.asList( altKeys ) );
					this._keys.Add( key );
			  }
		 }

		 public override string ToString()
		 {
			  return this.GetType().BaseType.Name + "" + _keys;
		 }

		 public virtual bool Matches( string key )
		 {
			  return _keys.Contains( key );
		 }

		 public virtual IEnumerable<string> Keys
		 {
			 get
			 {
				  return _keys;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  Service service = ( Service ) o;
			  return _keys.SetEquals( service._keys );
		 }

		 public override int GetHashCode()
		 {
			  return _keys.GetHashCode();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T> Iterable<T> filterExceptions(final Iterable<T> iterable)
		 private static IEnumerable<T> FilterExceptions<T>( IEnumerable<T> iterable )
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass(iterable);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<T>
		 {
			 private IEnumerable<T> _iterable;

			 public PrefetchingIteratorAnonymousInnerClass( IEnumerable<T> iterable )
			 {
				 this._iterable = iterable;
			 }

			 private IEnumerator<T> iterator = _iterable.GetEnumerator();

			 protected internal override T fetchNextOrNull()
			 {
				  while ( iterator.hasNext() )
				  {
						try
						{
							 return iterator.next();
						}
						catch ( Exception e )
						{
							 if ( _printServiceLoaderStackTraces )
							 {
								  Console.WriteLine( e.ToString() );
								  Console.Write( e.StackTrace );
							 }
						}
				  }
				  return null;
			 }
		 }

		 private static IEnumerable<T> Java6Loader<T>( Type type )
		 {
				 type = typeof( T );
			  try
			  {
					Dictionary<string, T> services = new Dictionary<string, T>();
					ClassLoader currentCL = typeof( Service ).ClassLoader;
					ClassLoader contextCL = Thread.CurrentThread.ContextClassLoader;

					IEnumerable<T> contextClassLoaderServices = ServiceLoader.load( type, contextCL );

					if ( currentCL != contextCL )
					{
						 // JBoss 7 does not export content of META-INF/services to context
						 // class loader, so this call adds implementations defined in Neo4j
						 // libraries from the same module.
						 IEnumerable<T> currentClassLoaderServices = ServiceLoader.load( type, currentCL );
						 // Combine services loaded by both context and module class loaders.
						 // Service instances compared by full class name ( we cannot use
						 // equals for instances or classes because they can came from
						 // different class loaders ).
						 PutAllInstancesToMap( currentClassLoaderServices, services );
						 // Services from context class loader have higher precedence,
						 // so we load those later.
					}

					PutAllInstancesToMap( contextClassLoaderServices, services );
					return services.Values;
			  }
			  catch ( Exception e ) when ( e is Exception || e is LinkageError )
			  {
					if ( _printServiceLoaderStackTraces )
					{
						 e.printStackTrace();
					}
					return null;
			  }
		 }

		 private static void PutAllInstancesToMap<T>( IEnumerable<T> services, IDictionary<string, T> servicesMap )
		 {
			  foreach ( T instance in FilterExceptions( services ) )
			  {
					if ( default( T ) != instance )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 servicesMap[instance.GetType().FullName] = instance;
					}
			  }
		 }
	}

}